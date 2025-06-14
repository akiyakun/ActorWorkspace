#!/bin/sh

echo 'LaunchMacOS.sh'

# launchdから起動するとパスが通って無いので設定します
export PATH=/usr/local/bin:$PATH

# エージェント起動コマンド、書き換えてください
java -jar /Users/buildserver/local/jenkins/agent.jar -jnlpUrl http://192.168.0.2:8080/jenkins/computer/BuildServer/slave-agent.jnlp -secret 012345 -workDir "/Users/buildserver/local/jenkins/"

