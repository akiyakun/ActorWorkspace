#!/usr/bin/env groovy

/*
メッセージは今のところRocketChat専用
*/

// このファイル内でも使うからここに定義
Config = null

/*
    参考
    http://javatechnology.net/java/url-encode/
*/
def URLEncodeEx(url) {
    String ret = URLEncoder.encode(url, "UTF-8")
    ret = ret.replace(" ", "%20");
    ret = ret.replace("+", "%20");
    ret = ret.replace("*", "%2a");
    ret = ret.replace("-", "%2d");
    return ret;
}

/*
    Linux系とWindows系で同じコマンドが使えるように
*/
def DoCommand(command) {
    if(isUnix()) {
        return sh(script:command, returnStdout:true).trim()
    } else {
        command = command.replace("%", "%%");
        command = "@echo off\n${command}"
        return bat(script:command, returnStdout:true).trim()
    }
}

// 専用のコンフィグファイルを読み込み
def ReadConfig() {
    // Config = readYaml(file: 'jenkins_config.yml')
    Config = readJSON(file: 'ci-cd_config.json')
    if (Config == null)
    {
        error "Do not read config file."
    }
}

// エージェント情報を取得
def GetAgentInfo(agentName)
{
    def ret = null
    Config.agents.each {
        if (it["name"] == agentName) {
            ret = it
            //return it // MEMO: 関数を抜けれない
        }
    }
    if (ret == null) {
        error "GetAgentInfo() : Agent情報が見つかりませんでした. agentName = " + agentName
        return null
    }
    return ret

}

// ジョブのコンフィグを取得
def GetJobConfig(platform)
{
    def ret = null
    Config.jobs.each {
        if (it["platform"] == platform) {
            ret = it
            //return it // MEMO: 関数を抜けれない
        }
    }
    if (ret == null) {
        error "GetJobConfig() : 対象のJob設定が見つかりませんでした. platform = " + platform
        return null
    }
    return ret
}

// ジョブが有効か判定
def IsJobEnable(jobConfig) {
    if (jobConfig['enable'] && !jobConfig['debug_skip']) {
        return true
    }
    return false
}

// ビルドディレクトリの中身を削除
def CleanupBuild() {
    echo 'CleanupBuild'

    //deleteDir()   // 全部消える
    dir('build/') {
        deleteDir()
    }
}

// ビルド成果物の保存
def ArchiveArtifacts(jobConfig, artifacts = null) {
    if (IsJobEnable(jobConfig)) {
        if (artifacts == null) {
            // if(isUnix()) {
                artifacts = 'build/**/*'
            // } else {
            //     artifacts = 'build/**/*.*'
            // }
        }

        echo "ArchiveArtifacts() : ${jobConfig['platform']} artifacts = " + artifacts

        archiveArtifacts(
            artifacts: artifacts,
            excludes: '.DS_Store, Thumbs.db',
            fingerprint: true)
    }
}


// 成果物リンクメのッセージ
def SendMsgArtifacts() {
    if (Config.notify.enable && Config.artifacts.enable) {
        def git_log = DoCommand('git log -4 --oneline')

        def base_url = "https://url"
        def enc = URLEncodeEx("=" + Config.notify.artifacts_url + "/${env.BUILD_NUMBER}/")
        def smb = GetAgentInfo(Config.artifacts.agent).external_output_artifacts;

        String text = "新しいビルドが出来ました！\n" \
                + "\n" \
                + "* [Output](${base_url}${enc})\n" \
                + "* Network Drive: ${smb}\n" \
                + "---------------------------------\n" \
                + "Git URL: ${env.GIT_URL.replace(".git", "")}/tree/${GIT_BRANCH.split('/')[1]}\n" \
                + "Commit Log:\n" \
                + "${git_log}"
        
        rocketSend emoji: ':floppy_disk:', attachments: [[color: '#0000ff', text: text]], channel: "${Config.notify.channel}", rawMessage: true, message: ""
    }
}

// テスト失敗のメッセージ
def SendMsgTestFailed() {
    if (Config.notify.enable) {
        def git_log = DoCommand("git log -4 --oneline")
        def git_user = DoCommand("git log -1 --pretty=format:'%an'")
        
        def text = "${git_log}"

        rocketSend emoji: ':sob:', attachments: [[color: '#ff0000', text: text]], channel: "${Config.notify.channel}", rawMessage: true,
            message: "Test Failed!\n" \
                        + "@${git_user} pushed. commito to *<${GIT_URL.minus('.git')}/commit/${GIT_COMMIT}|${GIT_BRANCH.split('/')[1]}>*\n" \
                        + "${env.JOB_NAME} - <${env.BUILD_URL}|#${env.BUILD_NUMBER}> - <${env.BUILD_URL}testReport/|TestReport>"
    }
}

// ビルド失敗のメッセージ
def SendMsgBuildFailed() {
    if (Config.notify.enable) {
        def git_log = DoCommand("git log -4 --oneline")
        def git_user = DoCommand("git log -1 --pretty=format:'%an'")

        def text = "${env.JOB_NAME} - [#${env.BUILD_NUMBER}](${env.BUILD_URL}):\n" \
                    + "user:${git_user} hash:${env.GIT_COMMIT.take(7)} branche:${env.GIT_BRANCH}\n" \
                    + "Git URL:${env.GIT_URL}\n" \
                    + "Job URL:${env.JOB_URL}\n" \
                    + "Log:\n ${git_log}"

        def avater = 'https://wiki.jenkins.io/download/attachments/2916393/fire-jenkins.svg'
        rocketSend avatar: avater, attachments: [[color: '#ff0000', text: text]], channel: "${Config.notify.channel}", message: 'Build Failed!', rawMessage: true
    }
}

return this