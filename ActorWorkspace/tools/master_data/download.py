import sys
import os
import io
import shutil
import argparse
import json
from google.oauth2 import service_account
from googleapiclient.discovery import build
from googleapiclient.http import MediaIoBaseDownload


print_origin = print
def print(s):
    print_origin(s, flush=True)


SCOPES = [
    # 'https://www.googleapis.com/auth/drive.file',
    "https://www.googleapis.com/auth/drive",
    "https://www.googleapis.com/auth/spreadsheets",
]

XLSX_MIME_TYPE = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"

FOLDER_ID = "14LkEdc3Ve87uOGyrCArDTm1WDVYik8Aw"

XLSX_DIR_NAME = "xlsx"
OUT_DIR = "temp/" + XLSX_DIR_NAME


def download_xlsx(service, file):
    request = service.files().export_media(fileId=file["id"], mimeType=XLSX_MIME_TYPE)
    save_path = os.path.join(OUT_DIR, file["name"] + ".xlsx")

    fh = io.FileIO(save_path, mode="wb")
    downloader = MediaIoBaseDownload(fh, request)
    done = False
    while not done:
        status, done = downloader.next_chunk()
    print("download_xlsx: {}".format(save_path))


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
    # OUT_DIR = os.path.join(args.out_dir, XLSX_DIR_NAME)

    app_config = load_app_config()
    OUT_DIR = os.path.join("../../", app_config['master_data']['temp_directory'], XLSX_DIR_NAME);
    print(f"OUT_DIR={OUT_DIR}")

    if os.path.exists(OUT_DIR):
        shutil.rmtree(OUT_DIR)
    os.makedirs(OUT_DIR)

    credentials = service_account.Credentials.from_service_account_file(
        "service_account.json"
    )
    scoped_credentials = credentials.with_scopes(SCOPES)

    service = build("drive", "v3", credentials=scoped_credentials)

    try:
        # フォルダからファイルIDのリストを得る
        condition_list = [f"('{FOLDER_ID}' in parents)"]
        conditions = " and ".join(condition_list)
        results = (
            service.files()
            .list(q=conditions, pageSize=100, fields="nextPageToken, files(id, name)")
            .execute()
        )
        files = results.get("files", [])

        print("Files:")
        for file in files:
            print("{0} ({1})".format(file["name"], file["id"]))
            download_xlsx(service, file)

    except Exception as e:
        print(e)
        sys.exit(1)

    print("finish.")
    sys.exit(0)
