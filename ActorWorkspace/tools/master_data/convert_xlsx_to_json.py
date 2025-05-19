import sys
import os
import shutil
import glob
import argparse
import pandas as pd
import json

# import math
import numpy as np


print_origin = print
def print(s):
    print_origin(s, flush=True)


OUT_DIR = "temp/json"
XLSX_DIR_NAME = "xlsx"
IN_DIR = "temp/" + XLSX_DIR_NAME


# Person オブジェクト用の、カスタムのエンコーダ
# class PersonEncoder(json.JSONEncoder):
#     def default(self, obj):
#         print("print")
#         print(type(obj).__name__)
#         if isinstance(obj, float('NaN')):
#             print("nan");
#             return "None"
#         if isinstance(obj, str):
#             print("str");
#             if obj == "nan":
#                 return "None"
#             # return {'class': 'Person', 'name': obj.name}
#             return super().default(obj)
#         else:
#             return super().default(obj)


def data_frame_validation(df):
    # IdカラムがNaNの行を削除
    df = df.dropna(subset=["Id"])

    # Idカラムをint型に変換
    # df.loc["Id"] = df["Id"].astype(int).values
    df = df.astype({"Id": int})

    # NaN要素は仮に文字列とし空文字に置換
    for index, row in enumerate(df.values):
        # print(f"raw={row}")
        for i, x in enumerate(row):
            # if (i == 0):
            #     df.iat[index, i] = df.iat[index, i].astype(int)
            #     continue

            if (x is np.nan):
                # print(f"i={i}, x={x}")
                df.iat[index, i] = ""

    return df


def convert_xlsx_to_json(file):
    df = pd.read_excel(file, sheet_name="MasterData")

    df = data_frame_validation(df)

    basename_without_ext = os.path.splitext(os.path.basename(file))[0]
    json_path = os.path.join(OUT_DIR, basename_without_ext + ".json")

    json.dump(
        df.to_dict(orient="records"),
        open(json_path, "w", encoding="utf-8"),
        indent=4,
        ensure_ascii=False
        # cls=PersonEncoder
    )


def load_app_config():
    with open("../../app_config.json", "r") as json_open:
        json_load = json.load(json_open)
    return json_load


def parse_args():
    parser = argparse.ArgumentParser(description="desc")
    # parser.add_argument('--out_dir', type=str, help='出力ディレクトリ', required=True)
    return parser.parse_args()


if __name__ == "__main__":
    args = parse_args()
    # OUT_DIR = os.path.join(args.out_dir, JSON_DIR_NAME)
    # IN_DIR = os.path.join(args.out_dir, XLSX_DIR_NAME)

    app_config = load_app_config()
    OUT_DIR = os.path.join("../../", app_config['master_data']['raw_data_directory'])
    IN_DIR = os.path.join("../../", app_config['master_data']['temp_directory'], XLSX_DIR_NAME);
    print(f"OUT_DIR={OUT_DIR}")
    print(f"IN_DIR={IN_DIR}")

    if os.path.exists(OUT_DIR):
        shutil.rmtree(OUT_DIR)
    os.makedirs(OUT_DIR)

    try:
        files = glob.glob(f"{IN_DIR}/*")

        print("Files:")
        for file in files:
            print(f"{file}")
            convert_xlsx_to_json(file)

    except Exception as e:
        print(e)
        sys.exit(1)

    print("finish.")
    sys.exit(0)
