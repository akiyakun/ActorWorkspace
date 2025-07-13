cd `dirname $0`
echo ${PWD}

src=../../../libs/afl
if [ -d ${src} ]; then
    ../bin/symlink --override --src ${src} --dest ../../Packages/cyou.sumomo.afl
fi

src=../../../libs/unitycicd
if [ -d ${src} ]; then
    ../bin/symlink --override --src ${src} --dest ../../Packages/com.akiyakun.unitycicd
fi

#read -p "Press any key to exit..."
