#nullable enable
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using afl;
using afl.EventDirector;

namespace ActorWorkspace.EventCommand
{
    public class ECSpawnActor : EventCommandBase
    {
        public override int Id => (int)AWEventCommandId.SpawnActor;
        public override EventCommandExecuteMode ExecuteMode => EventCommandExecuteMode.Immediate;

        AWEventCommandFactory owner;

#nullable disable
        private ECSpawnActor() { }
#nullable enable

        public ECSpawnActor(AWEventCommandFactory owner, bool immediate)
        {
            this.owner = owner;
        }

        public override void Start(EventCommandParam param = default)
        {
            StartImmediateMode(param);
        }

        protected IAWActor? StartImmediateMode(EventCommandParam param)
        {
            var actor = owner.ActorManager.Spawn(param.Param1.Int, param.Param2.Int, parent: null);
            if (actor == null)
            {
                // プールに空きがない場合は非同期生成コマンドを発行する
                // afl.EventDirector.EventDirector.Instance.Execute(owner.ECAddToActorPool, param);
                // EventDirector.Instance
                return null;
            }

            // actor.GameObject.SetActive(true);
            return actor;
        }

        protected void StartAsyncMode(EventCommandParam param)
        {
            // if (string.IsNullOrEmpty(param.String)) throw new System.ArgumentException();
            State = EventCommandState.Running;
            EvaluateAsync(param.Param1.Int, param.Param2.Int, param.Param3.Int, GetCancellationToken()).Forget();
        }

        async UniTask EvaluateAsync(int id, int category, int count, CancellationToken cancellationToken)
        {
            Debug.Log($"[ECSpawnActor] id={id}, category={category}, count={count}");
            await owner.ActorManager.AddToPoolAsync(id, category, count, cancellationToken: cancellationToken);
            State = EventCommandState.Completed;
        }
    }
}
#nullable restore