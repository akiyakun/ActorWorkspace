#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using afl;
using afl.EventDirector;

namespace ActorWorkspace.EventCommand
{
    // Actorをスポーンするコマンド
    public class ECSpawnActorAsync : EventCommandBase
    {
        public override int Id => (int)AWEventCommandId.SpawnActorAsync;
        public override string Name => nameof(AWEventCommandId.SpawnActorAsync);
        public override EventCommandExecuteMode ExecuteMode => EventCommandExecuteMode.Default;

        IAWActorManager actorManager;

#nullable disable
        protected ECSpawnActorAsync() { }
#nullable enable

        public ECSpawnActorAsync(IAWActorManager actorManager)
        {
            this.actorManager = actorManager;
        }

        // int Param1: ActorId
        // int Param2: Category
        // int Param3: Cluster
        // UserData: ECSpawnActor.SpawnOption
        public override void Start(IEventContext? context, EventCommandParam param)
        {
            SetState(EventCommandState.Running);

            if (param.UserData is ECSpawnActor.SpawnOption option)
            {
                Response = option.Cluster;
                EvaluateAsync(actorManager, param.Param1.Int, param.Param2.Int,
                    option.Parent, option.Position, option.Cluster, GetCancellationToken()).Forget();
            }
            else
            {
                EvaluateAsync(actorManager, param.Param1.Int, param.Param2.Int,
                    null, Vector3.zero, param.Param3.Int, GetCancellationToken()).Forget();
            }
        }

        protected async UniTask EvaluateAsync(IAWActorManager actorManager, int id, int category,
            GameObject? parent, Vector3 position, int cluster,
            CancellationToken cancellationToken)
        {
            Debug.Log($"[ECSpawnActorAsync] EvaluateAsync start. {id}:{Name}, category={category}, parent={parent}, position={position}");

            // await UniTask.Delay(3000, cancellationToken: cancellationToken);

            await actorManager.AddToPoolAsync(id: id, category: category, count: 1, cancellationToken: cancellationToken);

            var actor = actorManager.Spawn(id: id, category: category, parent: parent);
            if (actor == null)
            {
                SetState(EventCommandState.Error);
                throw new System.Exception($"Failed to spawn actor after adding to pool. {id}:{Name}, category={category}");
            }

            actor.ActorParam.Cluster = cluster;
            actor.GameObject.transform.position = position;
            actor.GameObject.SetActive(true);

            SetState(EventCommandState.Completed);
        }

    }
}
#nullable restore