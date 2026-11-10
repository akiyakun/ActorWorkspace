#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;

namespace ActorWorkspace.ArcaneLedger.ActorBehaviour
{
    // 当たりをどこで受け取るか
    public enum ALHurtBoxDetectMode
    {
        // アクターの MainCollisionDetector(Variable)で受け取る。どの HurtBox に当たったかは分からない(部位は 0)
        Main,
        // HurtBoxFolder の下の HurtBox ごとの ICollisionDetector で受け取る(どの HurtBox に当たったかが分かる)
        // MEMO: 部位・倍率・優先度をアニメーションの追加情報から入れるのは、今はコメントアウトしている(下の OnAwake)
        // MEMO: HurtBox のアタッチメントに CollisionDetector を付けておくこと(ゲーム側)
        PerHurtBox,
    }

    // やられ判定(HurtBox)
    // 当たり判定の通知を候補として溜め、OnUpdate でまとめて確定してから ArcaneLedgerBehaviour.ContactWithHurtBox() を呼ぶ
    // (陣営・ヒット履歴・同じフレームに複数の部位に当たったときの部位の選び方は ALHitResolver)
    // HurtBox のアタッチメントには ALCollisionExtraData を付ける(部位・倍率・優先度はその ALCollisionExtraData の値。今はアニメーションの追加情報からは入れない)
    //
    // HurtBoxはHitBoxとのみ接触する想定
    // IALCollisionExtraData が取得できる前提
    public class ALHurtBoxBehaviour : AWActorBehaviour<IAWActor>
    {
        // 当たる相手のレイヤー(物理で調べる前の大まかな絞り込み。陣営は ArcaneLedgerBehaviour.HostileMask で決める)
        public int LayerMask { get; set; }

        // 当たりにしない通知の種類(ALHitResolver.IgnoreEvents。既定は「離れた」(Exit))
        public CollisionEventType IgnoreEvents { get => resolver.IgnoreEvents; set => resolver.IgnoreEvents = value; }

        // 当たりをどこで受け取るか(変えると購読し直す)
        ALHurtBoxDetectMode detectMode = ALHurtBoxDetectMode.Main;
        public ALHurtBoxDetectMode DetectMode
        {
            get => detectMode;
            set
            {
                if (detectMode == value) return;
                detectMode = value;
                if (isAwaked) Subscribe();
            }
        }

        public override UpdateFlags UpdateFlags { get; protected set; } = UpdateFlags.Update;

        // ヒット履歴の古い記録を捨てる間隔(秒)
        const float PruneInterval = 5.0f;

        ArcaneLedgerBehaviour ledger = null!;
        readonly ALHitResolver resolver = new();
        // ヒット履歴の時刻(ワールドを止めている間は進まない)
        float elapsedTime;
        float pruneTimer;
        bool isAwaked;

        // 購読している当たり判定(購読し直すときに外す)
        readonly List<(ICollisionDetector detector, System.Action<CollisionContactInfo> callback)> subscriptions = new();
        readonly List<ALCollisionExtraData> hurtBoxes = new();

        public ALHitResolver Resolver => resolver;

        public override void OnRestore()
        {
            resolver.Clear();
            elapsedTime = 0.0f;
            pruneTimer = 0.0f;
        }

        public override void OnAwake()
        {
            ledger = Actor.GetLedger()!;
            if (ledger == null) throw new System.Exception("ALHurtBoxBehaviour DoAwake: ArcaneLedgerBehaviour not found in ActorBehaviourController");
            // 当たった瞬間の、受けた側の汎用パラメータを写す
            resolver.ReceiverParams = ledger.GeneralParam;

            // FolderObject
            // MEMO: HurtBoxFolder が無くても、Main(MainCollisionDetector で受け取る)なら当たる(今までどおり)。PerHurtBox は受け取る先が無い
            var folderObject = Variables.Get(AWCoreVariableKeys.HurtBoxFolder).GetGameObject();
            if (folderObject != null)
            {
                // FIXME: 部位・倍率・優先度をアニメーションの追加情報から入れるのは、使っていないのでコメントアウトした。AttackData の形が決まったら戻す
                // var hitAreaInfo = Actor.AnimationController?.ExtraData as IAWHitAreaInfoProvider;

                // GetComponentsInChildren()は自身も含む
                var followers = folderObject.GetComponentsInChildren<IAWFollower>(includeInactive: true);
                int count = followers.Length;
                for (int i = 0; i < count; i++)
                {
                    var follower = followers[i];

                    var extraInfo = follower.GameObject.GetComponent<ALCollisionExtraData>() ?? follower.GameObject.AddComponent<ALCollisionExtraData>();
                    extraInfo.Actor = Actor;
                    extraInfo.ALProcessor = ledger.Processor;

                    // 部位・倍率・優先度
                    // if (hitAreaInfo != null && hitAreaInfo.TryGetHurtBoxInfo(follower.GameObject.name, out var info))
                    // {
                    //     extraInfo.Region = info.Region;
                    //     extraInfo.DamageMultiplier = info.DamageMultiplier > 0.0f ? info.DamageMultiplier : 1.0f;
                    //     extraInfo.Priority = info.Priority;
                    // }
                    hurtBoxes.Add(extraInfo);
                }
            }

            isAwaked = true;
            Subscribe();
        }

        public override void OnDestroy()
        {
            // MEMO: 消したあとに DetectMode を変えても、購読し直さない
            isAwaked = false;
            Unsubscribe();
        }

        public override void OnUpdate(float deltaTime)
        {
            elapsedTime += deltaTime;

            if (resolver.PendingCount > 0)
            {
                var contacts = resolver.Resolve(elapsedTime, ledger.HostileMask);
                for (int i = 0; i < contacts.Count; i++)
                {
                    ledger.ContactWithHurtBox(contacts[i]);
                }
            }

            pruneTimer += deltaTime;
            if (pruneTimer >= PruneInterval)
            {
                pruneTimer = 0.0f;
                resolver.History.Prune(elapsedTime);
            }
        }

        void Subscribe()
        {
            Unsubscribe();

            switch (detectMode)
            {
                case ALHurtBoxDetectMode.Main:
                    {
                        var collisionDetector = Variables.Get(AWCoreVariableKeys.MainCollisionDetector).GetComponent<CollisionDetector>();
                        if (collisionDetector == null) throw new System.Exception($"ALHurtBoxBehaviour: CollisionDetector not found in variable {AWCoreVariableKeys.MainCollisionDetector}");
                        AddSubscription(collisionDetector, info => Collect(info, null));
                    }
                    break;
                case ALHurtBoxDetectMode.PerHurtBox:
                    foreach (var hurtBox in hurtBoxes)
                    {
                        var detector = hurtBox.GetComponent<ICollisionDetector>();
                        if (detector == null)
                        {
                            Debug.LogWarning($"ALHurtBoxBehaviour: HurtBox に ICollisionDetector がありません。name={hurtBox.gameObject.GetHierarchyPath()}");
                            continue;
                        }
                        var target = hurtBox;
                        AddSubscription(detector, info => Collect(info, target));
                    }
                    break;
            }
        }

        void AddSubscription(ICollisionDetector detector, System.Action<CollisionContactInfo> callback)
        {
            detector.OnCollision += callback;
            subscriptions.Add((detector, callback));
        }

        void Unsubscribe()
        {
            foreach (var (detector, callback) in subscriptions)
            {
                if (detector != null) detector.OnCollision -= callback;
            }
            subscriptions.Clear();
        }

        void Collect(CollisionContactInfo info, IALCollisionExtraData? hurtBox)
        {
            // 対象のレイヤー以外なら抜ける
            if (info.Other.HasLayerFromMask(LayerMask) == false) return;
            if (info.Other.TryGetComponent<IALCollisionExtraData>(out var attacker) == false) return;

            D.Log($"ALHurtBoxBehaviour OnCollision: EventType={info.EventType}\nOther: {info.Other.GetHierarchyPath()}", ColorEx.LightBlue);

#if UNITY_EDITOR
            if (attacker.DebugBreakPause)
            {
                D.DebugEditorPause();
                D.DebugBreak();
            }
#endif

            // MEMO: 陣営が当たらない候補は Add() で捨てる(汎用パラメータを写さない)
            resolver.HostileMask = ledger.HostileMask;
            resolver.Add(info, attacker, hurtBox);
        }
    }
}
#nullable restore
