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
                    Common = load('tools/ci-cd/Common.groovy')
                    Common.ReadConfig()
                    JobConfig = Common.GetJobConfig('Windows')
                }
            }
        }

        stage('[Debub] Build') {
            agent { label "${JobConfig.agent}" }

            environment { 
                UNITY_PATH = "${Common.GetAgentInfo(JobConfig.agent).unity_path}"
            }

            steps {
                script {
                    // echo 'Debug Build' 
                    Common.CleanupBuild()

                    if (Common.IsJobEnable(JobConfig)) {
                        if(isUnix()) {
                            sh '''
                                cd tools/build/
                                python3 unity.py --unity_path "${UNITY_PATH}" --platform "StandaloneWindows64" --development
                            '''
                        } else {
                            bat '''
                                cd tools/build/
                                py -3 unity.py --unity_path "%UNITY_PATH%" --platform "StandaloneWindows64" --development
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

        stage('[Release] Build') {
            agent { label "${JobConfig.agent}" }

            environment { 
                UNITY_PATH = "${Common.GetAgentInfo(JobConfig.agent).unity_path}"
            }

            steps {
                script {
                    // echo 'Release Build' 
                    Common.CleanupBuild()

                    if (Common.IsJobEnable(JobConfig)) {
                        if(isUnix()) {
                            sh '''
                                cd tools/build/
                                python3 unity.py --unity_path "${UNITY_PATH}" --platform "StandaloneWindows64"
                            '''
                        } else {
                            bat '''
                                cd tools/build/
                                py -3 unity.py --unity_path "%UNITY_PATH%" --platform "StandaloneWindows64"
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
    //     always { 
    //         script {
    //             Common.CleanupBuild()
    //         }
    //     }
    //     // success {
    //     //     script {
    //     //         echo 'windows job.' 
    //     //         if(isUnix()) {
    //     //             echo 'running unix'
    //     //         } else {
    //     //             echo 'running windows'
    //     //         }
    //     //         // Common.ArchiveArtifacts(JobConfig)
    //     //     }
    //     //     // CleanupBuild()
    //     // }
    //     // failure {
    //     //     // ログファイルのみを抽出
    //     //     // archiveArtifacts(artifacts: 'build/**/*.log', excludes: '.DS_Store, Thumbs.db', fingerprint: true)
    //     //     // CleanupBuild(true)
    //     // }
    // }
}
