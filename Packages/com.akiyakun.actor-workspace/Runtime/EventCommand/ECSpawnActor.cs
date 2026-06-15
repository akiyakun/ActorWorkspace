#nullable enable
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using afl;
using afl.EventDirector;

namespace ActorWorkspace.EventCommand
{
    // Actorをスポーンするコマンド
    // プールに空きがない場合は非同期生成コマンドを発行します
    public class ECSpawnActor : EventCommandBase
    {
        public override int Id => (int)AWEventCommandId.SpawnActor;
        public override EventCommandExecuteMode ExecuteMode => EventCommandExecuteMode.Immediate;

        public class SpawnOption
        {
            public GameObject? Parent;
            public Vector3 Position;
        }
        static SpawnOption spawnOption = new SpawnOption();

        protected IAWActorManager ActorManager { get; }

#nullable disable
        protected ECSpawnActor() { }
#nullable enable

        public ECSpawnActor(IAWActorManager actorManager)
        {
            ActorManager = actorManager;
        }

        public override void Start(IEventContext? context, EventCommandParam param)
        {
            if (param.UserData is SpawnOption option)
            {
                SpawnFromPool(ActorManager,
                    param.Param1.Int, param.Param2.Int, parent: option.Parent, position: option.Position);
            }
            else
            {
                SpawnFromPool(ActorManager,
                    param.Param1.Int, param.Param2.Int, parent: null, position: Vector3.zero);
            }
        }

        protected IAWActor? SpawnFromPool(IAWActorManager actorManager, int id, int category, GameObject? parent, Vector3 position)
        // SpawnOption? spawnOption = null)
        {
            var actor = actorManager.Spawn(id: id, category: category, parent: parent);
            if (actor == null)
            {
                // プールに空きがない場合は非同期生成コマンドを発行する
                spawnOption.Parent = parent;
                spawnOption.Position = position;
                EventDirector.Instance.Request((int)AWEventCommandId.SpawnActorAsync,
                    new EventCommandParam { Param1 = { Int = id }, Param2 = { Int = category }, UserData = spawnOption });

                return null;
            }

            actor.GameObject.transform.position = position;
            actor.GameObject.SetActive(true);

            return actor;
        }

        // protected virtual void RequestSpawnAsync(int commandId, EventCommandParam param)
        // {
        //     EventDirector.Instance.Request(commandId, param);
        // }

    }
}
#nullable restore