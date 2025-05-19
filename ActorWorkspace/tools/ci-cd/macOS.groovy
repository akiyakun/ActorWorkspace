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
                    JobConfig = Common.GetJobConfig('macOS')
                }
            }
        }

        stage('[Debub] Build') {
            agent { label "${JobConfig.agent}" }

            environment { 
                UNITY_PATH = "${Common.GetAgentInfo(JobConfig.agent).unity_path}"
                DEVELOPER_DIR = "${Common.GetAgentInfo(JobConfig.agent).xcode_path}"
            }

            steps {
                script {
                    //echo 'Debug Build' 
                    Common.CleanupBuild()

                    if (Common.IsJobEnable(JobConfig)) {
                        if(isUnix()) {
                            sh '''
                                cd tools/build/
                                python3 unity.py --unity_path "${UNITY_PATH}" --platform "StandaloneOSX" --development
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

        stage('[Release] Build') {
            agent { label "${JobConfig.agent}" }

            environment { 
                UNITY_PATH = "${Common.GetAgentInfo(JobConfig.agent).unity_path}"
                DEVELOPER_DIR = "${Common.GetAgentInfo(JobConfig.agent).xcode_path}"
            }

            steps {
                script {
                    // echo 'Release Build' 
                    Common.CleanupBuild()

                    if (Common.IsJobEnable(JobConfig)) {
                        if(isUnix()) {
                            sh '''
                                cd tools/build/
                                python3 unity.py --unity_path "${UNITY_PATH}" --platform "StandaloneOSX"
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
}
