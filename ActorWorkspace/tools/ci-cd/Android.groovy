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
                    JobConfig = Common.GetJobConfig("Android")
                }
            }
        }

        stage('[Debub] Build .apk') {
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
                                python3 unity.py --unity_path "${UNITY_PATH}" --platform Android --development
                            '''
                        } else {
                            bat '''
                                cd tools/build/
                                py -3 unity.py --unity_path "%UNITY_PATH%" --platform Android --development
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

        stage('[Release] Build .apk') {
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
                                python3 unity.py --unity_path "${UNITY_PATH}" --platform Android
                            '''
                        } else {
                            bat '''
                                cd tools/build/
                                py -3 unity.py --unity_path "%UNITY_PATH%" --platform Android
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
}
