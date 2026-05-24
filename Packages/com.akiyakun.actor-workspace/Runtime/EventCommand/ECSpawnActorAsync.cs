#nullable enable
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using afl;
using afl.EventDirector;

namespace ActorWorkspace.EventCommand
{
    // Actorをスポーンするコマンド
    public class ECSpawnActorAsync : EventCommandBase
    {
        public override int Id => (int)AWEventCommandId.SpawnActorAsync;
        public override EventCommandExecuteMode ExecuteMode => EventCommandExecuteMode.Default;

        AWEventCommandFactory? awEventCommandFactory;

#nullable disable
        protected ECSpawnActorAsync() { }
#nullable enable

        public ECSpawnActorAsync(AWEventCommandFactory awEventCommandFactory)
        {
            this.awEventCommandFactory = awEventCommandFactory;
        }

        public override void Start(EventCommandParam param = default)
        {
            var actorManager = awEventCommandFactory!.ActorManager;

            State = EventCommandState.Running;

            if (param.UserData is ECSpawnActor.SpawnOption option)
            {
                EvaluateAsync(actorManager, param.Param1.Int, param.Param2.Int,
                    option.Parent, option.Position, GetCancellationToken()).Forget();
            }
            else
            {
                EvaluateAsync(actorManager, param.Param1.Int, param.Param2.Int,
                    null, Vector3.zero, GetCancellationToken()).Forget();
            }
        }

        protected async UniTask EvaluateAsync(IAWActorManager actorManager, int id, int category,
            GameObject? parent, Vector3 position, CancellationToken cancellationToken)
        {
            Debug.Log($"[ECSpawnActorAsync] EvaluateAsync start. id={id}, category={category}, parent={parent}, position={position}");

            await actorManager.AddToPoolAsync(id: id, category: category, count: 1, cancellationToken: cancellationToken);

            var actor = actorManager.Spawn(id: id, category: category, parent: parent);
            if (actor == null)
            {
                State = EventCommandState.Error;
                throw new System.Exception($"Failed to spawn actor after adding to pool. id={id}, category={category}");
            }

            actor.GameObject.transform.position = position;
            actor.GameObject.SetActive(true);

            State = EventCommandState.Completed;
        }

    }
}
#nullable restore