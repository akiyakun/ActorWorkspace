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

        AWEventCommandFactory? awEventCommandFactory;

#nullable disable
        protected ECSpawnActor() { }
#nullable enable

        public ECSpawnActor(AWEventCommandFactory awEventCommandFactory)
        {
            this.awEventCommandFactory = awEventCommandFactory;
        }

        public override void Start(EventCommandParam param = default)
        {
            StartImmediateMode(awEventCommandFactory!.ActorManager, param.Param1.Int, param.Param2.Int);
        }

        protected IAWActor? StartImmediateMode(IAWActorManager actorManager, int id, int category)
        {
            var actor = actorManager.Spawn(id: id, category: category, parent: null);
            if (actor == null)
            {
                // プールに空きがない場合は非同期生成コマンドを発行する
                var param = new EventCommandParam();
                param.Param1.Int = id;
                param.Param2.Int = category;
                EventDirector.Instance.Request(AWEventCommandId.SpawnActorAsync, param);
                return null;
            }

            // actor.GameObject.SetActive(true);
            return actor;
        }
    }
}
#nullable restore