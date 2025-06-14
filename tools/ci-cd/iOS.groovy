#!/usr/bin/env groovy

Common = null
JobConfig = null

pipeline {
    agent {
        label "${DEFAULT_AGENT}"
    }

    stages {
        stage('Startup') {
            steps {
                script {
                    Common = load("tools/ci-cd/Common.groovy")
                    Common.ReadConfig()
                    JobConfig = Common.GetJobConfig("iOS")
                }
            }
        }

        stage('[Debug] Export Xcode Project') {
            agent { label "${JobConfig.agent}" }

            environment { 
                UNITY_PATH = "${Common.GetAgentInfo(JobConfig.agent).unity_path}"
                DEVELOPER_DIR = "${Common.GetAgentInfo(JobConfig.agent).xcode_path}"
            }

            steps {
                script {
                    // echo '[Debug] Export Xode Project.' 
                    Common.CleanupBuild()

                    if (Common.IsJobEnable(JobConfig)) {
                        if(isUnix()) {
                            sh '''
                                # ログを出力するためにディレクトリを作成する
                                mkdir -p build/

                                cd tools/build/
                                python3 unity.py --unity_path "${UNITY_PATH}" --platform iOS --development \
                                    > ../../build/debug_ios_export.log
                            '''
                        } else {
                            bat '''
                                echo 'Error windows agent.'
                                exit 1
                            '''
                        }
                    }
                }// script
            }// steps
        }// stage

        stage('[Debug] Xcode Build .ipa') {
            agent { label "${JobConfig.agent}" }

            environment { 
                UNITY_PATH = "${Common.GetAgentInfo(JobConfig.agent).unity_path}"
                DEVELOPER_DIR = "${Common.GetAgentInfo(JobConfig.agent).xcode_path}"
            }

            steps {
                script {
                    // echo '[Debug] Xcode Build.' 
                    if (Common.IsJobEnable(JobConfig)) {
                        if(isUnix()) {
                            sh '''
                                cd tools/build

                                PROJ_DIR="../../build/debug/iOS/xcode/"
                                python3 build_ipa.py -proj_dir "${PROJ_DIR}" \
                                    -archive_path '../../build/debug/iOS/app.xcarchive' \
                                    -ipa_plist 'ipa_development.plist' \
                                    -out_ipa_dir '../../build/debug/iOS/ipa/' \
                                    > ../../build/debug_ios_ipa.log
                            '''
                        } else {
                            bat '''
                                echo 'Error windows agent.'
                                exit 1
                            '''
                        }
                    }
                }// script
            }// steps

            post { 
                success {
                    script {
                        Common.ArchiveArtifacts(JobConfig)
                    }
                }
            }
        }// stage

        stage('[Release] Export Xcode Project') {
            agent { label "${JobConfig.agent}" }

            environment { 
                UNITY_PATH = "${Common.GetAgentInfo(JobConfig.agent).unity_path}"
                DEVELOPER_DIR = "${Common.GetAgentInfo(JobConfig.agent).xcode_path}"
            }

            steps {
                // echo '[Release] Export Xode Project.' 
                script {
                    if (Common.IsJobEnable(JobConfig)) {
                        if(isUnix()) {
                            sh '''
                                cd tools/build/
                                python3 unity.py --unity_path "${UNITY_PATH}" --platform iOS \
                                    > ../../build/release_ios_export.log
                            '''
                        } else {
                            bat '''
                                echo 'Error windows agent.'
                                exit 1
                            '''
                        }
                    }
                }// script
            }// steps
        }// stage

        stage('[Release] Xcode Build .ipa') {
            agent { label "${JobConfig.agent}" }

            environment { 
                UNITY_PATH = "${Common.GetAgentInfo(JobConfig.agent).unity_path}"
                DEVELOPER_DIR = "${Common.GetAgentInfo(JobConfig.agent).xcode_path}"
            }

            steps {
                script {
                    if (Common.IsJobEnable(JobConfig)) {
                        // echo '[Release] Xcode Build.' 
                        if(isUnix()) {
                            sh '''
                                cd tools/build

                                PROJ_DIR="../../build/release/iOS/xcode/"
                                python3 build_ipa.py -proj_dir "${PROJ_DIR}" \
                                    -archive_path '../../build/release/iOS/app.xcarchive' \
                                    -ipa_plist 'ipa_development.plist' \
                                    -out_ipa_dir '../../build/release/iOS/ipa/' \
                                    > ../../build/release_ios_ipa.log
                            '''
                        } else {
                            bat '''
                                echo 'Error windows agent.'
                                exit 1
                            '''
                        }
                    }
                }// script
            }// steps

            post { 
                success {
                    script {
                        Common.ArchiveArtifacts(JobConfig)
                    }
                }
            }
        }// stage
    }

    // post { 
    //     // always { 
    //     //     script {
    //     //         // excludes: '**/*.plist, **/*.log',
    //     //         archiveArtifacts(artifacts: 'build/**/iOS/*.log, build/**/iOS/ipa/*', excludes: '.DS_Store, Thumbs.db', fingerprint: true)
    //     //         CleanupBuild()
    //     //     }
    //     // }
    //     success {
    //         script {
    //             Common.ArchiveArtifacts(JobConfig, artifacts: 'build/*.log, build/**/iOS/ipa/*')
    //             //archiveArtifacts(artifacts: 'build/**/iOS/*.log, build/**/iOS/ipa/*', excludes: '.DS_Store, Thumbs.db', fingerprint: true)

    //         }
    //         // archiveArtifacts(artifacts: 'build/**/iOS/*.log, build/**/iOS/ipa/*', excludes: '.DS_Store, Thumbs.db', fingerprint: true)
    //         // CleanupBuild()
    //     }
    // }
}
