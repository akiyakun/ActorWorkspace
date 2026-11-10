#nullable enable
using NUnit.Framework;
using ActorWorkspace.ArcaneLedger.ActorBehaviour;
using ActorWorkspace.Tests;

namespace ActorWorkspace.ArcaneLedger.Tests
{
    // DefaultALProcessor.ApplyDamage()・ApplyHeal() の EditMode テスト
    public class ApplyDamageTests
    {
        const int MaxLife = 0;

        class TestProcessor : DefaultALProcessor
        {
            public override int StatCapacity => 1;
            public override int MaxLifeStatId => MaxLife;

            public float InvincibleTimeOnHit;
            public bool Filter = true;
            public int DamagedCount;
            public int DefeatedCount;
            public ALDamageRejectReason LastDamagedReason;

            protected override bool FilterDamage(ref ALDamageInfo info) => Filter;
            protected override float CalcDamage(in ALDamageInfo info) => info.Value * 2.0f;
            protected override float GetInvincibleTimeOnHit(in ALDamageInfo info) => InvincibleTimeOnHit;
            protected override void OnDamaged(in ALDamagedArgs args)
            {
                DamagedCount++;
                LastDamagedReason = args.Result.Reason;
            }
            protected override void OnDefeated(in ALDamagedArgs args) => DefeatedCount++;
        }

        StubAWActor actor = null!;
        StubAWActor attacker = null!;
        ArcaneLedgerBehaviour behaviour = null!;
        TestProcessor processor = null!;
        ALHealth health = null!;

        int damagedEventCount;
        int defeatedEventCount;
        int dealtEventCount;
        int killedEventCount;
        int healedEventCount;

        [SetUp]
        public void SetUp()
        {
            actor = new StubAWActor(1, 0);
            attacker = new StubAWActor(2, 0);
            behaviour = actor.ActorBehaviourController.Add<ArcaneLedgerBehaviour<TestProcessor>>()!;
            processor = (TestProcessor)behaviour.Processor;
            health = behaviour.Health;

            behaviour.Stats.SetBase(MaxLife, 10.0f);
            behaviour.Restore();

            damagedEventCount = defeatedEventCount = dealtEventCount = killedEventCount = healedEventCount = 0;
            actor.EventBus.Subscribe(ALActorEvents.OnDamaged, (ALDamagedArgs _) => damagedEventCount++);
            actor.EventBus.Subscribe(ALActorEvents.OnDefeated, (ALDamagedArgs _) => defeatedEventCount++);
            actor.EventBus.Subscribe(ALActorEvents.OnHealed, (ALHealResult _) => healedEventCount++);
            attacker.EventBus.Subscribe(ALActorEvents.OnDealtDamage, (ALDamagedArgs _) => dealtEventCount++);
            attacker.EventBus.Subscribe(ALActorEvents.OnKilled, (ALDamagedArgs _) => killedEventCount++);
        }

        [TearDown]
        public void TearDown()
        {
            actor.Dispose();
            attacker.Dispose();
        }

        // HP を持たない(MaxLifeStatId が -1)
        class NoLifeProcessor : DefaultALProcessor
        {
            public override int StatCapacity => 1;
            public int DefeatedCount;
            protected override void OnDefeated(in ALDamagedArgs args) => DefeatedCount++;
        }

        [Test]
        public void HPを持たなければダメージを受けない()
        {
            var noLifeActor = new StubAWActor(3, 0);
            try
            {
                var noLife = noLifeActor.ActorBehaviourController.Add<ArcaneLedgerBehaviour<NoLifeProcessor>>()!;
                noLife.Restore();
                var noLifeProcessor = (NoLifeProcessor)noLife.Processor;

                var result = noLifeProcessor.ApplyDamage(ALDamageInfo.Hit(5.0f, attacker));

                Assert.IsFalse(result.Applied);
                Assert.AreEqual(ALDamageRejectReason.NoLife, result.Reason);
                Assert.IsFalse(result.Killed);
                Assert.IsTrue(noLife.Health.IsAlive);
                Assert.AreEqual(0, noLifeProcessor.DefeatedCount);
            }
            finally
            {
                noLifeActor.Dispose();
            }
        }

        [Test]
        public void Restoreで最大HPになる()
        {
            Assert.AreEqual(10.0f, health.Max);
            Assert.AreEqual(10.0f, health.Current);
            Assert.IsTrue(health.IsAlive);
        }

        [Test]
        public void 計算したダメージだけHPが減り知らせる()
        {
            var result = processor.ApplyDamage(ALDamageInfo.Hit(1.5f, attacker));

            Assert.IsTrue(result.Applied);
            Assert.AreEqual(3.0f, result.Damage);
            Assert.AreEqual(7.0f, health.Current);
            Assert.AreEqual(1, processor.DamagedCount);
            Assert.AreEqual(1, damagedEventCount);
            Assert.AreEqual(1, dealtEventCount);
            Assert.AreEqual(0, defeatedEventCount);
        }

        [Test]
        public void RequestDamageのイベントでも受ける()
        {
            actor.EventBus.Publish(ALActorEvents.RequestDamage, ALDamageInfo.Hit(1.0f));
            Assert.AreEqual(8.0f, health.Current);
        }

        [Test]
        public void HPが0になると倒れて一度だけ知らせる()
        {
            var result = processor.ApplyDamage(ALDamageInfo.Hit(100.0f, attacker));

            Assert.IsTrue(result.Killed);
            Assert.AreEqual(200.0f, result.Damage, "HP より大きくてもそのままの値");
            Assert.AreEqual(0.0f, health.Current);
            Assert.IsFalse(health.IsAlive);
            Assert.AreEqual(1, processor.DefeatedCount);
            Assert.AreEqual(1, defeatedEventCount);
            Assert.AreEqual(1, killedEventCount);

            var second = processor.ApplyDamage(ALDamageInfo.Hit(1.0f, attacker));
            Assert.AreEqual(ALDamageRejectReason.NotAlive, second.Reason);
            Assert.AreEqual(1, processor.DefeatedCount);
            Assert.AreEqual(1, defeatedEventCount);
        }

        [Test]
        public void 攻撃を受けると無敵になり無敵の間は攻撃を受けない()
        {
            processor.InvincibleTimeOnHit = 1.0f;
            processor.ApplyDamage(ALDamageInfo.Hit(1.0f));
            Assert.AreEqual(1.0f, health.InvincibleTime);

            var result = processor.ApplyDamage(ALDamageInfo.Hit(1.0f));
            Assert.AreEqual(ALDamageRejectReason.Invincible, result.Reason);
            Assert.AreEqual(8.0f, health.Current);

            // 無敵時間が終わると、また受ける
            behaviour.DoUpdate(1.1f);
            processor.ApplyDamage(ALDamageInfo.Hit(1.0f));
            Assert.AreEqual(6.0f, health.Current);
        }

        [Test]
        public void 続くダメージは無敵の間も受け_受けても無敵にならない()
        {
            processor.InvincibleTimeOnHit = 1.0f;
            health.InvincibleTime = 1.0f;

            var result = processor.ApplyDamage(ALDamageInfo.Continuous(1.0f));

            Assert.IsTrue(result.Applied);
            Assert.AreEqual(8.0f, health.Current);
            Assert.AreEqual(1.0f, health.InvincibleTime);

            health.InvincibleTime = 0.0f;
            processor.ApplyDamage(ALDamageInfo.Continuous(1.0f));
            Assert.AreEqual(0.0f, health.InvincibleTime);
        }

        [Test]
        public void ダメージが0なら_HPは減らず無敵にはなり_OnDamagedは出る()
        {
            processor.InvincibleTimeOnHit = 1.0f;

            var result = processor.ApplyDamage(ALDamageInfo.Hit(0.0f, attacker));

            Assert.IsFalse(result.Applied);
            Assert.AreEqual(ALDamageRejectReason.Zero, result.Reason);
            Assert.AreEqual(0.0f, result.Damage);
            Assert.AreEqual(10.0f, health.Current);
            Assert.AreEqual(1.0f, health.InvincibleTime);
            Assert.AreEqual(1, processor.DamagedCount);
            Assert.AreEqual(ALDamageRejectReason.Zero, processor.LastDamagedReason);
            Assert.AreEqual(1, damagedEventCount);
        }

        [Test]
        public void FilterDamageがfalseなら受けず知らせない()
        {
            processor.Filter = false;

            var result = processor.ApplyDamage(ALDamageInfo.Hit(1.0f));

            Assert.AreEqual(ALDamageRejectReason.Filtered, result.Reason);
            Assert.AreEqual(10.0f, health.Current);
            Assert.AreEqual(0, processor.DamagedCount);
            Assert.AreEqual(0, damagedEventCount);
        }

        [Test]
        public void 回復は最大HPまで_倒れたあとは回復しない()
        {
            processor.ApplyDamage(ALDamageInfo.Hit(2.0f));
            Assert.AreEqual(6.0f, health.Current);

            var heal = processor.ApplyHeal(100.0f);
            Assert.IsTrue(heal.Applied);
            Assert.AreEqual(4.0f, heal.Amount);
            Assert.AreEqual(10.0f, health.Current);
            Assert.AreEqual(1, healedEventCount);

            processor.ApplyDamage(ALDamageInfo.Hit(100.0f));
            Assert.IsFalse(processor.ApplyHeal(5.0f).Applied);
            Assert.AreEqual(0.0f, health.Current);
        }

        [Test]
        public void 最大HPのときは回復しない()
        {
            var heal = processor.ApplyHeal(5.0f);
            Assert.IsFalse(heal.Applied);
            Assert.AreEqual(0.0f, heal.Amount);
            Assert.AreEqual(0, healedEventCount);
        }

        [Test]
        public void RequestHealのイベントでも回復する()
        {
            processor.ApplyDamage(ALDamageInfo.Hit(2.0f));
            actor.EventBus.Publish(ALActorEvents.RequestHeal, 1.0f);
            Assert.AreEqual(7.0f, health.Current);
        }
    }
}
#nullable restore
