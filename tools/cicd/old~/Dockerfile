# jenkins-master:0.1
#
# Plugins
# https://plugins.jenkins.io/thinBackup
#
# 8080 : Jenkinsのポータルにアクセスするためのポート
# 50000 : Jenkinsのスレーブにアクセスするためのポート
#
# イメージのビルド
# $ docker build -t jenkins .
#
# 起動コマンド
# $ docker run -p 80:8080 -p 50000:50000 -e TZ=Asia/Tokyo -v ~/jenkins_home:/var/jenkins_home -d jenkins
#
# パーミッションのエラーが出る場合
# touch: cannot touch /var/jenkins_home/copy_reference_file.log: Permission denied
#
# chown -R 1000:1000 ~/jenkins_home
#


# Official Jenkins Docker image
# https://github.com/jenkinsci/docker/
#FROM jenkins/jenkins:lts
FROM jenkins/jenkins:2.208

# rootで作業開始
USER root

###############################################################################
# Jenkins Plugins
RUN install-plugins.sh \
    thinBackup:latest \
    jobConfigHistory:latest \
    # https://plugins.jenkins.io/rebuild
    rebuild:latest \

    extra-columns:latest \
    categorized-view:latest \

    # https://github.com/jenkinsci/gitlab-plugin
    gitlab-plugin:latest \

    # https://wiki.jenkins.io/display/JENKINS/RocketChat+Plugin
    rocketchatnotifier:latest \

    # https://plugins.jenkins.io/nunit
    nunit:latest \

    # https://plugins.jenkins.io/pipeline-utility-steps
    pipeline-utility-steps:latest \
    parameterized-trigger:latest \
    copyartifact:latest


# インストールを見送ったプラグインたち
# https://wiki.jenkins.io/display/JENKINS/Custom+Job+Icon+Plugin
# 古すぎてまともに動作しない
# custom-job-icon


###############################################################################
# 日本語localeの設定
# https://qiita.com/kazuyoshikakihara/items/0cf74c11d273b0064c83
RUN apt-get update && \
    apt-get install -y locales && \
    echo "ja_JP UTF-8" > /etc/locale.gen && \
    locale-gen

ENV LANG="ja_JP.UTF-8" \
    LANGUAGE="ja_JP:ja" \
    LC_ALL="ja_JP.UTF-8"


###############################################################################
# jenkinsユーザーに戻す
USER jenkins
