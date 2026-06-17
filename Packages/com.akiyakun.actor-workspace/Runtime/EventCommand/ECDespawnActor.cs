#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using afl;
using afl.EventDirector;

namespace ActorWorkspace.EventCommand
{
    // Actorをデスポーンするコマンド
    //
    // Arguments
    //  int Param1: Cluster値
    //  int Param2: 動作モード
    //
    public class ECDespawnActor : EventCommandBase
    {
        public override int Id => (int)AWEventCommandId.DespawnActor;
        public override string Name => nameof(AWEventCommandId.DespawnActor);
        public override EventCommandExecuteMode ExecuteMode => EventCommandExecuteMode.Immediate;

        enum Mode
        {
            // 強制的にデスポーン。ActorManagerから即座に削除する
            Force,
            // 通常のデスポーン。ActorのEventBusにDespawnイベントを発行する
            Normal,
        }

        IAWActorManager actorManager;

        public ECDespawnActor(IAWActorManager actorManager)
        {
            this.actorManager = actorManager;
        }

        public override void Start(IEventContext context, EventCommandParam param)
        {
            // D.Log($"[ECDespawnActor] Start. param={param}");
            SetState(EventCommandState.Completed);

            // クラスター値を取得
            int cluster = GetResponseOrParam1(context, ref param);
            // 動作モードを取得
            int mode = param.Param2.Int;

            var list = actorManager.GetActorListAtCluster(cluster);
            Debug.Assert((cluster != 0) && (list.Count > 0), $"ECSetEnemyAI: 対象のクラスタを取得できませんでした。\n直前のコマンドをWaitしているかなど確認してください。");

            switch (mode)
            {
                case (int)Mode.Force:
                    foreach (var actor in list) { actorManager.Despawn(actor); }
                    break;
                case (int)Mode.Normal:
                    foreach (var actor in list) { actor.EventBus.Publish(AWCoreActorEvents.Despawn); }
                    break;
                default:
                    throw new System.ArgumentException($"Invalid mode value: {mode}");
            }
        }
    }
}
#nullable restore