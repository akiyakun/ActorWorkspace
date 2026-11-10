#nullable enable
using NUnit.Framework;
using UnityEngine;
using ActorWorkspace.ArcaneLedger.ActorBehaviour;
using ActorWorkspace.Tests;

namespace ActorWorkspace.ArcaneLedger.Tests
{
    // ALMoveRestrictStatusEffectBase・ALDamageOverTimeStatusEffectBase・ALKnockbackStatusEffect・ALKnockbackUtility の EditMode テスト
    public class StatusEffectImplementTests
    {
        const int MaxLife = 0;
        const int MoveSpeed = 1;

        const int SlowId = 1;
        const int Slow2Id = 2;
        const int ConfuseId = 3;
        const int Confuse2Id = 4;
        const int PoisonId = 5;
        const int OtherId = 6;

        class TestProcessor : DefaultALProcessor
        {
            public override int StatCapacity => 2;
            public override int MaxLifeStatId => MaxLife;
            public override int MoveSpeedStatId => MoveSpeed;
            public StatusEffectController Controller => statusEffectController;
        }

        class TestSlow : ALMoveRestrictStatusEffectBase
        {
            readonly int id;
            public override int Id => id;
            public TestSlow(int id) { this.id = id; }
        }

        class TestConfuse : ALMoveRestrictStatusEffectBase
        {
            readonly int id;
            readonly Vector3 direction;
            public override int Id => id;
            public TestConfuse(int id, Vector3 direction) { this.id = id; this.direction = direction; }
            protected override void OnUpdate(float deltaTime) => OverrideMoveDirectionIfEmpty(direction);
        }

        class TestPoison : ALDamageOverTimeStatusEffectBase
        {
            public override int Id => PoisonId;
        }

        class TestOther : StatusEffect
        {
            public override int Id => OtherId;
        }

        // StubAWActor には GameObject が無いので、動かした量を数える
        class TestKnockback : ALKnockbackStatusEffect
        {
            public Vector3 Moved;
            protected override void Move(Vector3 delta) => Moved += delta;
        }

        StubAWActor actor = null!;
        ArcaneLedgerBehaviour behaviour = null!;
        StatusEffectController controller = null!;
        ALStatTable stats = null!;

        [SetUp]
        public void SetUp()
        {
            actor = new StubAWActor(1, 0);
            behaviour = actor.ActorBehaviourController.Add<ArcaneLedgerBehaviour<TestProcessor>>()!;
            controller = ((TestProcessor)behaviour.Processor).Controller;
            stats = behaviour.Stats;
            stats.SetBase(MaxLife, 10.0f);
            stats.SetBase(MoveSpeed, 4.0f);
            behaviour.Restore();
        }

        [TearDown]
        public void TearDown()
        {
            actor.Dispose();
        }

        static ApplyStatusEffectParams Params(int id, float param1 = 0.0f, float duration = -1.0f, float tickInterval = 0.0f, float param2 = 0.0f)
        {
            var p = new ApplyStatusEffectParams() { Id = id, ApplyChance = 1.0f, DurationTime = duration, TickIntervalTime = tickInterval };
            p.Param1.Float = param1;
            p.Param2.Float = param2;
            return p;
        }

        #region 移動の制限
        [Test]
        public void かかっている間は移動速度が下がり終わると戻る()
        {
            controller.SetStatusEffect(new TestSlow(SlowId));
            controller.Request(Params(SlowId, param1: 0.25f, duration: 1.0f));
            Assert.AreEqual(3.0f, stats.Get(MoveSpeed), 0.0001f);

            behaviour.DoUpdate(1.1f);
            Assert.AreEqual(4.0f, stats.Get(MoveSpeed), 0.0001f);
        }

        [Test]
        public void 減速が重なるといちばん遅くなるものを使う()
        {
            controller.SetStatusEffect(new TestSlow(SlowId));
            controller.SetStatusEffect(new TestSlow(Slow2Id));
            controller.Request(Params(SlowId, param1: 0.25f));
            controller.Request(Params(Slow2Id, param1: 0.5f));
            Assert.AreEqual(2.0f, stats.Get(MoveSpeed), 0.0001f);

            controller.Revoke(Slow2Id);
            Assert.AreEqual(3.0f, stats.Get(MoveSpeed), 0.0001f);
        }

        [Test]
        public void かけ直すと新しい減速になる()
        {
            controller.SetStatusEffect(new TestSlow(SlowId));
            controller.Request(Params(SlowId, param1: 0.5f));
            controller.Request(Params(SlowId, param1: 0.25f));
            Assert.AreEqual(3.0f, stats.Get(MoveSpeed), 0.0001f);
        }

        [Test]
        public void 減速の割合が0なら補正を付けない()
        {
            controller.SetStatusEffect(new TestSlow(SlowId));
            controller.Request(Params(SlowId, param1: 0.0f));
            Assert.AreEqual(4.0f, stats.Get(MoveSpeed), 0.0001f);
        }

        [Test]
        public void 移動の向きはIdが小さい効果を優先し毎フレーム戻る()
        {
            controller.SetStatusEffect(new TestConfuse(ConfuseId, Vector3.right));
            controller.SetStatusEffect(new TestConfuse(Confuse2Id, Vector3.left));
            controller.Request(Params(Confuse2Id, duration: 1.0f));
            controller.Request(Params(ConfuseId, duration: 0.5f));

            behaviour.DoUpdate(0.1f);
            Assert.AreEqual(Vector3.right, behaviour.OverrideMoveDirection);

            // Id が小さい効果が終わると、もう一方の向きになる
            behaviour.DoUpdate(0.5f);
            behaviour.DoUpdate(0.1f);
            Assert.AreEqual(Vector3.left, behaviour.OverrideMoveDirection);

            // どちらも終わると上書きしない
            behaviour.DoUpdate(1.0f);
            behaviour.DoUpdate(0.1f);
            Assert.AreEqual(Vector3.zero, behaviour.OverrideMoveDirection);
        }
        #endregion

        #region 続くダメージ
        [Test]
        public void 一定間隔で続くダメージを受ける()
        {
            controller.SetStatusEffect(new TestPoison());
            controller.Request(Params(PoisonId, tickInterval: 1.0f, param2: 2.0f));

            behaviour.DoUpdate(0.5f);
            Assert.AreEqual(10.0f, behaviour.Health.Current);

            behaviour.DoUpdate(0.6f);
            Assert.AreEqual(8.0f, behaviour.Health.Current);

            // 続くダメージなので無敵にならない
            Assert.AreEqual(0.0f, behaviour.Health.InvincibleTime);
        }

        [Test]
        public void 間隔より長いフレームでも続くダメージの回数は減らない()
        {
            controller.SetStatusEffect(new TestPoison());
            controller.Request(Params(PoisonId, tickInterval: 0.1f, param2: 0.1f));

            // 1 フレーム 0.155 秒(間隔 0.1 秒より長い)を 10 回 = 1.55 秒 → 15 回
            for (int i = 0; i < 10; i++) behaviour.DoUpdate(0.155f);
            Assert.AreEqual(10.0f - 1.5f, behaviour.Health.Current, 1e-4f);
        }
        #endregion

        #region ノックバック
        [Test]
        public void ノックバックは時間をかけて距離だけ押し返し_その間は動かない()
        {
            var knockback = new TestKnockback();
            controller.SetStatusEffect(knockback);
            controller.Request(ALKnockbackUtility.CreateApplyParams(new Vector2(3.0f, 4.0f), 2.0f, 0.5f));
            Assert.AreEqual(0.0f, stats.Get(MoveSpeed));

            behaviour.DoUpdate(0.25f);
            Assert.That(knockback.Moved.magnitude, Is.GreaterThan(1.0f), "ease out なので半分の時間で半分より多く動く");
            Assert.IsTrue(knockback.Enable);

            behaviour.DoUpdate(0.3f);
            Assert.AreEqual(1.2f, knockback.Moved.x, 0.0001f);
            Assert.AreEqual(1.6f, knockback.Moved.y, 0.0001f);
            Assert.IsFalse(knockback.Enable);
            Assert.AreEqual(4.0f, stats.Get(MoveSpeed), 0.0001f);
        }

        [Test]
        public void ノックバックの間はほかの効果を待たせ_終わってからかける()
        {
            var knockback = new TestKnockback();
            var other = new TestOther();
            controller.SetStatusEffect(knockback);
            controller.SetStatusEffect(other);

            controller.Request(ALKnockbackUtility.CreateApplyParams(Vector2.right, 1.0f, 0.2f));
            controller.Request(Params(OtherId));
            Assert.IsFalse(other.Enable);

            behaviour.DoUpdate(0.1f);
            Assert.IsFalse(other.Enable);

            // 押し返し終わったフレームの次の更新でかかる
            behaviour.DoUpdate(0.2f);
            behaviour.DoUpdate(0.1f);
            Assert.IsTrue(other.Enable);
        }

        [Test]
        public void 耐性を引いた段階()
        {
            Assert.AreEqual(2, ALKnockbackUtility.ApplyResist(3, 1.0f, 3));
            Assert.AreEqual(0, ALKnockbackUtility.ApplyResist(1, 5.0f, 3));
            Assert.AreEqual(3, ALKnockbackUtility.ApplyResist(5, -1.0f, 3));
        }

        [Test]
        public void ノックバックのパラメータは向きを長さ1にして必ずかかる()
        {
            var p = ALKnockbackUtility.CreateApplyParams(new Vector2(3.0f, 4.0f), 1.5f, 0.2f);
            Assert.AreEqual((int)CoreStatusEffectId.Knockback, p.Id);
            Assert.That(p.ApplyChance, Is.GreaterThanOrEqualTo(1.0f));
            Assert.AreEqual(0.6f, p.Param1.Float, 0.0001f);
            Assert.AreEqual(0.8f, p.Param2.Float, 0.0001f);
            Assert.AreEqual(1.5f, p.Param3.Float);
            Assert.AreEqual(0.2f, p.DurationTime);
        }
        #endregion
    }
}
#nullable restore
