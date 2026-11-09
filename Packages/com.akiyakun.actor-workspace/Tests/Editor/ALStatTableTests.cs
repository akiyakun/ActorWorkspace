#nullable enable
using NUnit.Framework;

namespace ActorWorkspace.ArcaneLedger.Tests
{
    // ALStatTable・ALHealth の EditMode テスト
    public class ALStatTableTests
    {
        const int A = 0;
        const int B = 1;

        [Test]
        public void 補正が無ければ基礎値()
        {
            var stats = new ALStatTable(2);
            stats.SetBase(A, 10.0f);
            Assert.AreEqual(10.0f, stats.Get(A));
            Assert.AreEqual(0.0f, stats.Get(B));
        }

        [Test]
        public void 足して割合を足して掛ける順番で計算する()
        {
            var stats = new ALStatTable(1);
            stats.SetBase(A, 10.0f);
            stats.AddModifier(A, ALModifierOp.Multiply, 2.0f);
            stats.AddModifier(A, ALModifierOp.PercentAdd, 0.25f);
            stats.AddModifier(A, ALModifierOp.Add, 2.0f);
            stats.AddModifier(A, ALModifierOp.PercentAdd, 0.25f);
            stats.AddModifier(A, ALModifierOp.Multiply, 1.5f);

            // (10 + 2) × (1 + 0.5) × 2 × 1.5
            Assert.AreEqual(54.0f, stats.Get(A), 0.0001f);
        }

        [Test]
        public void MultiplyMinはいちばん小さいものだけを掛ける()
        {
            var stats = new ALStatTable(1);
            stats.SetBase(A, 10.0f);
            stats.AddModifier(A, ALModifierOp.MultiplyMin, 0.5f);
            stats.AddModifier(A, ALModifierOp.MultiplyMin, 0.2f);
            stats.AddModifier(A, ALModifierOp.MultiplyMin, 0.8f);
            stats.AddModifier(A, ALModifierOp.Multiply, 2.0f);

            Assert.AreEqual(4.0f, stats.Get(A), 0.0001f);
        }

        [Test]
        public void Overrideは最後に足したものの値になる()
        {
            var stats = new ALStatTable(1);
            stats.SetBase(A, 10.0f);
            stats.AddModifier(A, ALModifierOp.Add, 5.0f);
            var first = stats.AddModifier(A, ALModifierOp.Override, 1.0f);
            var second = stats.AddModifier(A, ALModifierOp.Override, 3.0f);
            Assert.AreEqual(3.0f, stats.Get(A));

            second.Dispose();
            Assert.AreEqual(1.0f, stats.Get(A));

            first.Dispose();
            Assert.AreEqual(15.0f, stats.Get(A));
        }

        [Test]
        public void ハンドルで外せる_二度外しても何も起きない()
        {
            var stats = new ALStatTable(1);
            stats.SetBase(A, 10.0f);
            var handle = stats.AddModifier(A, ALModifierOp.Add, 5.0f);
            Assert.IsTrue(handle.IsValid);
            Assert.AreEqual(15.0f, stats.Get(A));

            handle.Dispose();
            Assert.IsFalse(handle.IsValid);
            Assert.AreEqual(10.0f, stats.Get(A));

            handle.Dispose();
            Assert.IsFalse(stats.RemoveModifier(handle));
            Assert.AreEqual(10.0f, stats.Get(A));
        }

        [Test]
        public void 出どころが同じ補正をまとめて外す()
        {
            var stats = new ALStatTable(2);
            stats.SetBase(A, 10.0f);
            stats.SetBase(B, 1.0f);
            var equip = new object();
            var skill = new object();
            stats.AddModifier(A, ALModifierOp.Add, 5.0f, equip);
            stats.AddModifier(B, ALModifierOp.Multiply, 3.0f, equip);
            stats.AddModifier(A, ALModifierOp.Add, 1.0f, skill);

            Assert.AreEqual(2, stats.RemoveModifiers(equip));

            Assert.AreEqual(11.0f, stats.Get(A));
            Assert.AreEqual(1.0f, stats.Get(B));
        }

        [Test]
        public void 範囲に収める()
        {
            var stats = new ALStatTable(1);
            stats.SetBase(A, 10.0f);
            stats.SetRange(A, 0.0f, 12.0f);
            stats.AddModifier(A, ALModifierOp.Add, 5.0f);
            Assert.AreEqual(12.0f, stats.Get(A));

            stats.AddModifier(A, ALModifierOp.Multiply, -1.0f);
            Assert.AreEqual(0.0f, stats.Get(A));
        }

        [Test]
        public void Restoreで補正が外れて基礎値は残る()
        {
            var stats = new ALStatTable(1);
            stats.SetBase(A, 10.0f);
            stats.AddModifier(A, ALModifierOp.Add, 5.0f);

            stats.Restore();

            Assert.AreEqual(10.0f, stats.Get(A));
        }

        [Test]
        public void 値が変わると知らせる()
        {
            var stats = new ALStatTable(2);
            int changedId = -1;
            int count = 0;
            stats.OnChanged += id => { changedId = id; count++; };

            stats.SetBase(B, 3.0f);
            Assert.AreEqual(B, changedId);

            stats.SetBase(B, 3.0f);
            Assert.AreEqual(1, count, "同じ値を入れたときは知らせない");
        }

        [Test]
        public void 最大HPが下がると今のHPも下がる_上がっても今のHPはそのまま()
        {
            var stats = new ALStatTable(1);
            stats.SetBase(A, 10.0f);
            var health = new ALHealth(stats, A);
            health.Reset();
            Assert.AreEqual(10.0f, health.Current);

            var handle = stats.AddModifier(A, ALModifierOp.Add, -4.0f);
            Assert.AreEqual(6.0f, health.Current);

            handle.Dispose();
            Assert.AreEqual(10.0f, health.Max);
            Assert.AreEqual(6.0f, health.Current);
        }

        [Test]
        public void HPを持たないときは最大HPが0()
        {
            var stats = new ALStatTable(0);
            var health = new ALHealth(stats, -1);
            health.Reset();
            Assert.IsFalse(health.HasLife);
            Assert.AreEqual(0.0f, health.Max);
            Assert.IsTrue(health.IsAlive);
        }
    }
}
#nullable restore
