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
    // プールに空きがない場合は非同期生成コマンドを発行します
    public class ECSpawnActor : EventCommandBase
    {
        public override int Id => (int)AWEventCommandId.SpawnActor;
        public override string Name => nameof(AWEventCommandId.SpawnActor);
        public override EventCommandExecuteMode ExecuteMode => EventCommandExecuteMode.Default;

        public class SpawnOption
        {
            public GameObject? Parent;
            public Vector3 Position;
            public int Cluster;
        }
        static SpawnOption spawnOption = new SpawnOption();

        List<SerialHandle> handles = new();

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
            SetState(EventCommandState.Running);

            if (param.UserData is SpawnOption option)
            {
                Response = option.Cluster;
                SpawnFromPool(ActorManager,
                    param.Param1.Int, param.Param2.Int, option.Parent, option.Position, option.Cluster);
            }
            else
            {
                SpawnFromPool(ActorManager,
                    param.Param1.Int, param.Param2.Int, null, Vector3.zero, 0);
            }
        }

        protected IAWActor? SpawnFromPool(IAWActorManager actorManager, int id, int category, GameObject? parent, Vector3 position, int cluster)
        // SpawnOption? spawnOption = null)
        {
            var actor = actorManager.Spawn(id: id, category: category, parent: parent);
            if (actor == null)
            {
                // プールに空きがない場合は非同期生成コマンドを発行する
                spawnOption.Parent = parent;
                spawnOption.Position = position;
                spawnOption.Cluster = cluster;

                var handle = EventDirector.Instance.Request((int)AWEventCommandId.SpawnActorAsync,
                    new EventCommandParam
                    {
                        Param1 = { Int = id },
                        Param2 = { Int = category },
                        Param3 = { Int = cluster },
                        UserData = spawnOption
                    }
                );
                handles.Add(handle);

                return null;
            }

            actor.ActorParam.Cluster = cluster;
            actor.GameObject.transform.position = position;
            actor.GameObject.SetActive(true);

            return actor;
        }

        public override void Evaluate(float deltaTime)
        {
            int count = handles.Count;
            for (int i = 0; i < count; i++)
            {
                // まだ実行中のコマンドがある場合はメソッドを抜ける
                if (handles[i].IsValid == true) return;
            }

            // 全ての非同期生成コマンドが完了した
            handles.Clear();

            SetState(EventCommandState.Completed);
        }

    }
}
#nullable restore