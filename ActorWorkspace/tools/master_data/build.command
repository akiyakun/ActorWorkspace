cd `dirname $0`
source ~/.zprofile
#echo ${PWD}

echo "============================="
echo "start download.py"
python3 download.py

echo "============================="
echo "start convert_xlsx_to_json.py"
python3 convert_xlsx_to_json.py

#read wait
exit 0
