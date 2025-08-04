import sys
import os
import argparse
import json
import subprocess


# def load_cicd_config():

def parse_args():
  parser = argparse.ArgumentParser(description ='desc')

  parser.add_argument('--runner', type=str,
    help='Target runner name.', required=True)
  parser.add_argument('--testPlatform', type=str,
    help='Unity -testPlatform', required=True)
  parser.add_argument('--assemblyNames', type=str,
    help='Unity -assemblyNames', required=True)
  # parser.add_argument('--override', action='store_true',
  #   help='既に存在する場合でも作成しなおす')

  return parser.parse_args()


if __name__ == "__main__":
  args = parse_args()
  # if args.params is None: args.params = ''
  # src = os.path.abspath(args.src)
  # dest = os.path.abspath(args.dest)

  # if args.override and os.path.exists(dest):
  #   os.remove(dest)

  # モノレポかポリレポか判断
  up_pass = None
  if os.path.exists('../../../cicd_config.json'):
    up_pass = '../../../'
  elif os.path.exists('../../cicd_config.json'):
    up_pass = '../../'
  else:
    print("cicd_config.json not found.")
    sys.exit(1)

  # cicd_config.jsonからrunner情報を取得
  runner = None
  try:
    with open('{}cicd_config.json'.format(up_pass), 'r', encoding='utf-8') as f:
      cicd_config = json.load(f)
    # print(cicd_config['runners'])

    # name が args.runner の要素を取得(見つからない場合はNone)
    runner = next((r for r in cicd_config['runners'] if r['name'] == args.runner), None)
    if runner is None: sys.exit(1)
    # print(runner['name'])
    # print(runner)
  except Exception as e:
    print(e)
    sys.exit(1)


  # Unityで実行
  try:
    result = subprocess.run(
      [
        runner['unity_path'],
        # "/Applications/Unity/Hub/Editor/6000.0.50f1-x86_64/Unity.app/Contents/MacOS/Unity",
        # '-projectPath', "C:/local/dev/itomon/survivors/survivors",
        '-projectPath',  '{}'.format(up_pass),
        '-forgetProjectPath'
        '-batchmode',
        '-runTests',
        # '-testPlatform', 'PlayMode',
        '-testPlatform', args.testPlatform,
        # '-playerHeartbeatTimeout', '1',
        # '-assemblyNames', 'Project.Tests',
        '-assemblyNames', args.assemblyNames,
        #'-testCategory', 'Project.Tests',
        # '-testResults', '../../../build/tests_playmode_results.xml',
        '-testResults', 'Logs/tests_results.xml'
      ],
      capture_output=True,  # 標準出力・標準エラー出力を取得
      text=True,            # 出力を文字列として取得
      check=True
      # , timeout=60*3
    )
    print("return code: {}".format(result.returncode))
    print("captured stdout: {}".format(result.stdout))
    print("captured stderr: {}".format(result.stderr))
    # print(result)
  except subprocess.TimeoutExpired as e:
    print("Timeout expired: {}".format(e.timeout))
    sys.exit(1)
  except subprocess.CalledProcessError as e:
    print(e)
    sys.exit(1)


  print("complate.")
  sys.exit(0)
