cd `dirname $0`
cd ../utility
# echo ${PWD}
./pyinstaller_builder.sh spine_actor_export_to_unity ../lib/spine_actor_export_to_unity.py
read -p "Press any key to exit..."
