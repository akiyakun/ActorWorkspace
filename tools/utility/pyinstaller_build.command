cd `dirname $0`
echo ${PWD}

build_dir=../../build/pyinstaller

build() {
  name=$2
  pyinstaller ./$1 --onefile --clean --name ${name} --distpath ${build_dir}/dist --workpath ${build_dir}/build --specpath ${build_dir}/build/
  cp -r ${build_dir}/dist/${name} ../bin/
}

build symlink.py symlink


#read -p "Press any key to exit..."
