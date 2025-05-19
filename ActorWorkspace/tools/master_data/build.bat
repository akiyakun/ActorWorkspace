@echo off
setlocal
pushd %‾dp0

echo "============================="
echo "start download.py"
py -3 download.py || exit /b 1 !ERRORLEVEL!

echo "============================="
echo "start convert_xlsx_to_json.py"
py -3 convert_xlsx_to_json.py || exit /b 1 !ERRORLEVEL!

exit /b 0
