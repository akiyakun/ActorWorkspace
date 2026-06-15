#nullable enable
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using afl;
using afl.EventDirector;

namespace ActorWorkspace.EventCommand
{
    public class ECAddToActorPool : EventCommandBase
    {
        public override int Id => (int)AWEventCommandId.AddToActorPool;
        public override EventCommandExecuteMode ExecuteMode => EventCommandExecuteMode.Default;

        IAWActorManager actorManager;

#nullable disable
        private ECAddToActorPool() {}
#nullable enable

        public ECAddToActorPool(IAWActorManager actorManager)
        {
            this.actorManager = actorManager;
            Debug.Assert(actorManager != null);
        }

        public override void Start(IEventContext? context, EventCommandParam param)
        {
            // if (string.IsNullOrEmpty(param.String)) throw new System.ArgumentException();
            State = EventCommandState.Running;
            EvaluateAsync(param.Param1.Int, param.Param2.Int, param.Param3.Int, GetCancellationToken()).Forget();
        }

        async UniTask EvaluateAsync(int id, int category, int count, CancellationToken cancellationToken)
        {
            Debug.Log($"[ECAddToActorPool] id={id}, category={category}, count={count}");
            await actorManager.AddToPoolAsync(id, category, count, cancellationToken: cancellationToken);
            State = EventCommandState.Completed;
        }
    }
}
#nullable restore