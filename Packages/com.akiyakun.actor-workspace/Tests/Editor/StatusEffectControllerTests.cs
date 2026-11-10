#nullable enable
using NUnit.Framework;
using ActorWorkspace.ArcaneLedger.ActorBehaviour;
using ActorWorkspace.Tests;

namespace ActorWorkspace.ArcaneLedger.Tests
{
    // StatusEffect・StatusEffectController の EditMode テスト
    // MEMO: ArcaneLedgerBehaviour をスタブのアクター(StubAWActor)に付けて、Update を直接呼ぶ
    public class StatusEffectControllerTests
    {
        // テスト用のプロセッサー(コントローラーを外から触れるようにする)
        class TestProcessor : DefaultALProcessor
        {
            public StatusEffectController Controller => statusEffectController;
            public int FixedUpdateCount;
            public override void FixedUpdate(float deltaTime) => FixedUpdateCount++;
        }

        // テスト用の効果。Id は作るときに決める
        class TestEffect : StatusEffect
        {
            readonly int id;
            public override int Id => id;

            public bool KeepTick = true;
            protected override bool KeepTickOnReapply => KeepTick;

            public int TickCount;
            public int FixedUpdateCount;
            public ApplyStatusEffectParams LastAppliedParams => AppliedParams;

            public TestEffect(int id) { this.id = id; }
            protected override void OnTick() => TickCount++;
            protected override void OnFixedUpdate(float deltaTime) => FixedUpdateCount++;
        }

        StubAWActor actor = null!;
        ArcaneLedgerBehaviour behaviour = null!;
        TestProcessor processor = null!;
        StatusEffectController controller = null!;

        [SetUp]
        public void SetUp()
        {
            actor = new StubAWActor(1, 0);
            behaviour = actor.ActorBehaviourController.Add<ArcaneLedgerBehaviour<TestProcessor>>()!;
            processor = (TestProcessor)behaviour.Processor;
            controller = processor.Controller;
        }

        [TearDown]
        public void TearDown()
        {
            actor.Dispose();
        }

        TestEffect AddEffect(int id)
        {
            var effect = new TestEffect(id);
            controller.SetStatusEffect(effect);
            return effect;
        }

        static ApplyStatusEffectParams Params(int id, float duration = -1.0f, float tickInterval = 0.0f, float chance = 1.0f)
        {
            return new ApplyStatusEffectParams() { Id = id, ApplyChance = chance, DurationTime = duration, TickIntervalTime = tickInterval };
        }

        bool IsEnabled(int id) => (controller.EnableFlags & (1u << id)) != 0;
        bool IsRequested(int id) => (controller.RequestFlags & (1u << id)) != 0;

        #region 成功判定
        [Test]
        public void 成功率1以上は必ずかかる()
        {
            var effect = AddEffect(1);
            for (int i = 0; i < 1000; i++)
            {
                controller.Revoke(1);
                Assert.IsTrue(controller.Request(Params(1, chance: 1.0f)));
                Assert.IsTrue(effect.Enable);
            }
        }

        [Test]
        public void 成功率0はかからない()
        {
            var effect = AddEffect(1);
            Assert.IsFalse(controller.Request(Params(1, chance: 0.0f)));
            Assert.IsFalse(effect.Enable);
        }

        [Test]
        public void 登録していない効果はかからず_待たせもしない()
        {
            Assert.IsFalse(controller.Request(Params(5)));
            Assert.IsFalse(IsEnabled(5));
            Assert.IsFalse(IsRequested(5));
        }
        #endregion

        #region パラメータ
        [Test]
        public void かけたときのパラメータをAppliedParamsで読める()
        {
            var effect = AddEffect(1);
            var param = Params(1, duration: 3.0f);
            param.Param1.Float = 0.25f;
            param.Param2.Int = 7;
            param.Param3.Float = 1.5f;

            controller.Request(param);

            Assert.AreEqual(0.25f, effect.LastAppliedParams.Param1.Float);
            Assert.AreEqual(7, effect.LastAppliedParams.Param2.Int);
            Assert.AreEqual(1.5f, effect.LastAppliedParams.Param3.Float);
            Assert.AreEqual(3.0f, effect.LastAppliedParams.DurationTime);
        }
        #endregion

        #region Tick
        [Test]
        public void かけ直しても次のTickまでの時間を引き継ぐ()
        {
            var effect = AddEffect(1);
            controller.Request(Params(1, tickInterval: 1.0f));

            behaviour.DoUpdate(0.6f);
            controller.Request(Params(1, tickInterval: 1.0f));
            behaviour.DoUpdate(0.5f);

            // 最初にかけてから 1.1 秒たったので 1 回
            Assert.AreEqual(1, effect.TickCount);
        }

        [Test]
        public void かけ直したとき新しい間隔のほうが短ければ縮める()
        {
            var effect = AddEffect(1);
            controller.Request(Params(1, tickInterval: 2.0f));

            behaviour.DoUpdate(0.1f);
            controller.Request(Params(1, tickInterval: 0.5f));
            behaviour.DoUpdate(0.5f);

            Assert.AreEqual(1, effect.TickCount);
        }

        [Test]
        public void KeepTickOnReapplyがfalseならかけ直すたびに数え直す()
        {
            var effect = AddEffect(1);
            effect.KeepTick = false;
            controller.Request(Params(1, tickInterval: 1.0f));

            behaviour.DoUpdate(0.6f);
            controller.Request(Params(1, tickInterval: 1.0f));
            behaviour.DoUpdate(0.5f);

            Assert.AreEqual(0, effect.TickCount);
        }

        [Test]
        public void 終わってからかけ直したときは最初から数える()
        {
            var effect = AddEffect(1);
            controller.Request(Params(1, duration: 0.5f, tickInterval: 1.0f));
            behaviour.DoUpdate(0.6f);
            Assert.IsFalse(effect.Enable);

            controller.Request(Params(1, tickInterval: 1.0f));
            behaviour.DoUpdate(0.6f);

            Assert.AreEqual(0, effect.TickCount);
        }
        #endregion

        #region FixedUpdate
        [Test]
        public void FixedUpdateはかかっている効果とプロセッサーに届く()
        {
            var enabled = AddEffect(1);
            var disabled = AddEffect(2);
            controller.Request(Params(1));

            behaviour.DoFixedUpdate(0.02f);

            Assert.AreEqual(1, enabled.FixedUpdateCount);
            Assert.AreEqual(0, disabled.FixedUpdateCount);
            Assert.AreEqual(1, processor.FixedUpdateCount);
        }
        #endregion

        #region 重ね掛けのルール
        void SetupExclusive(ALStatusEffectStackRule rule)
        {
            AddEffect(1);
            AddEffect(2);
            AddEffect(3);
            controller.ExclusiveMask = (1u << 1) | (1u << 2);
            controller.StackRule = rule;
        }

        [Test]
        public void 前維持はほかの種類がかかっているとかからない()
        {
            SetupExclusive(ALStatusEffectStackRule.KeepCurrent);
            controller.Request(Params(1));

            Assert.IsFalse(controller.Request(Params(2)));
            Assert.IsTrue(IsEnabled(1));
            Assert.IsFalse(IsEnabled(2));
        }

        [Test]
        public void 前維持でも同じ種類はかけ直せる()
        {
            SetupExclusive(ALStatusEffectStackRule.KeepCurrent);
            controller.Request(Params(1, duration: 1.0f));

            Assert.IsTrue(controller.Request(Params(1, duration: 5.0f)));
            Assert.AreEqual(5.0f, controller.GetStatusEffect(1)!.RemainingTime);
        }

        [Test]
        public void 上書きはほかの種類を終わらせてかける()
        {
            SetupExclusive(ALStatusEffectStackRule.Overwrite);
            controller.Request(Params(1));

            Assert.IsTrue(controller.Request(Params(2)));
            Assert.IsFalse(IsEnabled(1));
            Assert.IsTrue(IsEnabled(2));
        }

        [Test]
        public void 上書きは待たせているほかの種類も捨てる()
        {
            SetupExclusive(ALStatusEffectStackRule.Overwrite);
            controller.RequestMask = 1u << 1;
            controller.Request(Params(1));
            Assert.IsTrue(IsRequested(1));

            controller.Request(Params(2));

            Assert.IsFalse(IsRequested(1));
            Assert.IsTrue(IsEnabled(2));
        }

        [Test]
        public void 重複はほかの種類と重なる()
        {
            SetupExclusive(ALStatusEffectStackRule.Stack);
            controller.Request(Params(1));

            Assert.IsTrue(controller.Request(Params(2)));
            Assert.IsTrue(IsEnabled(1));
            Assert.IsTrue(IsEnabled(2));
        }

        [Test]
        public void ルールの対象外の種類はルールに関係なくかかる()
        {
            SetupExclusive(ALStatusEffectStackRule.KeepCurrent);
            controller.Request(Params(1));

            Assert.IsTrue(controller.Request(Params(3)));
            Assert.IsTrue(IsEnabled(1));
            Assert.IsTrue(IsEnabled(3));
        }

        [Test]
        public void 前維持は待たせているほかの種類があるときもかからない()
        {
            SetupExclusive(ALStatusEffectStackRule.KeepCurrent);
            controller.RequestMask = 1u << 1;
            controller.Request(Params(1));

            Assert.IsFalse(controller.Request(Params(2)));
        }
        #endregion

        #region Revoke・CancelRequest・Restore
        [Test]
        public void Revokeは待たせているリクエストも捨てる()
        {
            AddEffect(1);
            controller.RequestMask = 1u << 1;
            controller.Request(Params(1));
            Assert.IsTrue(IsRequested(1));

            controller.Revoke(1);
            behaviour.DoUpdate(0.1f);

            Assert.IsFalse(IsRequested(1));
            Assert.IsFalse(IsEnabled(1));
        }

        [Test]
        public void CancelRequestはかかっている効果を残す()
        {
            AddEffect(1);
            controller.Request(Params(1));
            controller.RequestMask = 1u << 1;
            controller.Request(Params(1));

            controller.CancelRequest(1);

            Assert.IsFalse(IsRequested(1));
            Assert.IsTrue(IsEnabled(1));
        }

        [Test]
        public void 待たせたリクエストはマスクが外れた更新でかかる()
        {
            AddEffect(1);
            controller.RequestMask = 1u << 1;
            controller.Request(Params(1));
            Assert.IsFalse(IsEnabled(1));

            // Prepare() でマスクが戻るので、次の更新でかかる
            behaviour.DoUpdate(0.1f);

            Assert.IsTrue(IsEnabled(1));
            Assert.IsFalse(IsRequested(1));
        }

        [Test]
        public void Restoreで効果と待たせているリクエストがなくなる()
        {
            AddEffect(1);
            AddEffect(2);
            controller.Request(Params(1));
            controller.RequestMask = 1u << 2;
            controller.Request(Params(2));

            behaviour.Restore();

            Assert.AreEqual(0u, controller.EnableFlags);
            Assert.AreEqual(0u, controller.RequestFlags);
        }
        #endregion

        #region 優先度
        [Test]
        public void いちばん優先度の高い効果のIdを返す()
        {
            AddEffect(3);
            AddEffect(5);
            Assert.AreEqual(-1, controller.GetTopPriorityEnabledId());

            controller.Request(Params(5));
            Assert.AreEqual(5, controller.GetTopPriorityEnabledId());

            controller.Request(Params(3));
            Assert.AreEqual(3, controller.GetTopPriorityEnabledId());
        }
        #endregion
    }
}
#nullable restore
