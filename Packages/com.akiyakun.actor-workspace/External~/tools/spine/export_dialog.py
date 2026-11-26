"""spine_actor_export_to_unity の、人が使うときのダイアログ(開始の確認・進み具合・終了・エラー)。

tkinter だけで作る(Windows と Mac の両方で動くように)。tk の操作はメインスレッドだけで行い、
書き出しは別のスレッドで走らせる(書き出しの間も窓が「応答なし」にならないように)。
"""

from __future__ import annotations

import queue
import threading
import tkinter as tk
from collections.abc import Callable
from tkinter import font, messagebox, ttk

TITLE = "spine_actor_export_to_unity"
WARNING_TEXT = "エクスポートが終わるまで、Unity を操作しないでください。"
WARNING_COLOR = "#d00000"
POLL_INTERVAL_MS = 100

# 書き出しの処理に渡す、進み具合を知らせる関数: (今の番号(0 から), 全体の数, 今のファイル名)
ProgressCallback = Callable[[int, int, str], None]


class ExportDialog:
    def __init__(self) -> None:
        # MEMO: Tk を作れない(ディスプレイが無いなど)ときは tk.TclError になる。呼ぶ側でコンソールだけに切り替える
        self._root = tk.Tk()
        # 中身を作ってから出す(_show)
        self._root.withdraw()
        self._root.title(TITLE)
        self._root.resizable(False, False)
        self._root.attributes("-topmost", True)
        self._frame: tk.Frame | None = None
        self._warning: tk.Label | None = None
        self._warning_font = font.nametofont("TkDefaultFont").copy()
        self._warning_font.configure(weight="bold")

    def close(self) -> None:
        self._root.destroy()

    def confirm_start(self, count: int) -> bool:
        """開始の確認。はいなら True。"""
        answer = tk.BooleanVar(value=False)
        decided = tk.BooleanVar(value=False)

        def decide(value: bool) -> None:
            answer.set(value)
            decided.set(True)

        frame = self._new_frame()
        tk.Label(frame, text=f"Spineのエクスポート処理を開始します。（{count} 件）", anchor="w", justify="left").pack(fill="x")
        self._add_warning(frame, pady=(8, 12))
        buttons = tk.Frame(frame)
        buttons.pack(side="right")
        yes = ttk.Button(buttons, text="はい", command=lambda: decide(True))
        yes.pack(side="left", padx=(0, 8))
        ttk.Button(buttons, text="いいえ", command=lambda: decide(False)).pack(side="left")
        # Enter はフォーカスのあるボタン(最初は「はい」)を押す
        self._root.bind("<Return>", lambda e: e.widget.invoke() if isinstance(e.widget, ttk.Button) else decide(True))
        self._root.bind("<Escape>", lambda _: decide(False))
        self._root.protocol("WM_DELETE_WINDOW", lambda: decide(False))

        self._show()
        yes.focus_set()
        self._root.wait_variable(decided)
        self._root.unbind("<Return>")
        self._root.unbind("<Escape>")
        return answer.get()

    def run(self, work: Callable[[ProgressCallback], None]) -> None:
        """進み具合の窓を出して、work を別のスレッドで走らせる。work の例外はここで投げ直す。"""
        frame = self._new_frame()
        self._add_warning(frame, pady=(0, 8))
        file_label = tk.Label(frame, text="準備中...", anchor="w", justify="left", wraplength=480)
        file_label.pack(fill="x")
        count_label = tk.Label(frame, text="", anchor="w")
        count_label.pack(fill="x")
        progress_bar = ttk.Progressbar(frame, length=480, mode="determinate", maximum=1)
        progress_bar.pack(fill="x", pady=(4, 0))
        # MEMO: 書き出しの途中では止められないので、閉じるボタンでは閉じない
        self._root.protocol("WM_DELETE_WINDOW", lambda: None)
        self._show()

        progress_queue: queue.Queue[tuple[int, int, str]] = queue.Queue()
        errors: list[BaseException] = []

        def worker() -> None:
            try:
                work(lambda index, total, name: progress_queue.put((index, total, name)))
            except BaseException as e:  # noqa: BLE001 メインスレッドで投げ直す
                errors.append(e)

        def poll() -> None:
            # MEMO: tk の操作はメインスレッドだけで行う。別のスレッドからは queue で受け取る
            # MEMO: 終わったかを先に見る(終わる直前に届いた進み具合も表示してから抜けるように)
            alive = thread.is_alive()
            while not progress_queue.empty():
                index, total, name = progress_queue.get_nowait()
                file_label.configure(text=name)
                count_label.configure(text=f"{index + 1} / {total}")
                progress_bar.configure(value=index / total if total else 0)
            if alive:
                self._root.after(POLL_INTERVAL_MS, poll)
            else:
                self._root.quit()

        callback_errors: list[BaseException] = []

        def report_callback_exception(exc_type, value, tb) -> None:
            # MEMO: tk はコールバックの中の例外(コンソールで Ctrl+C を押したときの KeyboardInterrupt など)を表示するだけで、
            #       mainloop を抜けない。抜けてから投げ直す
            callback_errors.append(value)
            self._root.quit()

        self._root.report_callback_exception = report_callback_exception
        # MEMO: daemon にしない。中断しても、書き出しの後始末(Unity のロックを外すなど)まで走らせてから終わるように
        thread = threading.Thread(target=worker)
        thread.start()
        self._root.after(POLL_INTERVAL_MS, poll)
        self._root.mainloop()
        if callback_errors:
            print("中断しました。書き出しの途中のものが終わるのを待ってから終わります")
            raise callback_errors[0]
        thread.join()
        if errors:
            raise errors[0]
        progress_bar.configure(value=1)

    def show_done(self) -> None:
        self._remove_warning()
        messagebox.showinfo(TITLE, "Spineのエクスポート処理が終了しました。", parent=self._root)

    def show_error(self, message: str) -> None:
        self._remove_warning()
        messagebox.showerror(TITLE, f"エクスポートに失敗しました。\n\n{message}\n\n詳しくはコンソールの出力を見てください。",
                             parent=self._root)

    def _new_frame(self) -> tk.Frame:
        if self._frame is not None:
            self._frame.destroy()
        self._frame = tk.Frame(self._root, padx=16, pady=16)
        self._frame.pack(fill="both", expand=True)
        return self._frame

    def _add_warning(self, parent: tk.Widget, pady: tuple[int, int]) -> None:
        self._warning = tk.Label(parent, text=WARNING_TEXT, fg=WARNING_COLOR, anchor="w", justify="left",
                                 font=self._warning_font)
        self._warning.pack(fill="x", pady=pady)

    def _remove_warning(self) -> None:
        # 終わったら「Unity を操作しないで」の注意を消す(終了の知らせと一緒に出ないように)
        if self._warning is not None:
            self._warning.pack_forget()
            self._warning = None

    def _show(self) -> None:
        # 画面の真ん中に、最前面で出す
        root = self._root
        root.update_idletasks()
        width, height = root.winfo_reqwidth(), root.winfo_reqheight()
        x = (root.winfo_screenwidth() - width) // 2
        y = (root.winfo_screenheight() - height) // 3
        root.geometry(f"+{x}+{y}")
        root.deiconify()
        root.lift()
        root.focus_force()
