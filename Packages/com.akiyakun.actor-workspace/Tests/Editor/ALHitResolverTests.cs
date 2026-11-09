#nullable enable
using NUnit.Framework;
using afl;
using ActorWorkspace.ArcaneLedger.ActorBehaviour;

namespace ActorWorkspace.ArcaneLedger.Tests
{
    // ALHitResolver・ALHitHistory・ALFaction の EditMode テスト
    public class ALHitResolverTests
    {
        // テスト用の当たり判定の情報(GameObject を使わない)
        class FakeExtraData : IALCollisionExtraData
        {
            public IAWActor? Actor { get; set; }
            public IALProcessor? ALProcessor { get; set; }
            public uint SerialNumber { get; set; }
            public uint Flags { get; set; }
            public int IntValue { get; set; }
            public float FloatValue { get; set; }
            public string StringValue { get; set; } = "";
            public object? UserData { get; set; }
            public int Faction { get; set; }
            public int AttackId { get; set; }
            public ALHitHistoryMode HitHistoryMode { get; set; }
            public float RepeatHitInterval { get; set; }
            public int Region { get; set; }
            public float DamageMultiplier { get; set; } = 1.0f;
            public int Priority { get; set; }
#if UNITY_EDITOR
            public bool DebugBreakPause { get; set; }
#endif
        }

        static CollisionContactInfo Raw() => default;

        [Test]
        public void Noneは同じ攻撃でも候補ごとに当たる()
        {
            var resolver = new ALHitResolver();
            var attacker = new FakeExtraData() { HitHistoryMode = ALHitHistoryMode.None };
            resolver.Add(Raw(), attacker, null);
            resolver.Add(Raw(), attacker, null);

            Assert.AreEqual(2, resolver.Resolve(0.0f, ALFaction.All).Count);
            // 次のフレームも当たる
            resolver.Add(Raw(), attacker, null);
            Assert.AreEqual(1, resolver.Resolve(0.1f, ALFaction.All).Count);
        }

        [Test]
        public void 陣営がHostileMaskに入っていなければ当たらない()
        {
            var resolver = new ALHitResolver();
            var attacker = new FakeExtraData() { Faction = 1 };
            resolver.Add(Raw(), attacker, null);
            Assert.AreEqual(0, resolver.Resolve(0.0f, ALFaction.ToBit(0)).Count);

            resolver.Add(Raw(), attacker, null);
            Assert.AreEqual(1, resolver.Resolve(0.0f, ALFaction.ToBit(0) | ALFaction.ToBit(1)).Count);
        }

        [Test]
        public void 同じフレームに複数の部位に当たると優先度の大きい部位を1つ使う()
        {
            var resolver = new ALHitResolver();
            var attacker = new FakeExtraData() { HitHistoryMode = ALHitHistoryMode.PerActivation, SerialNumber = 10 };
            var body = new FakeExtraData() { Region = 1, Priority = 1 };
            var weak = new FakeExtraData() { Region = 3, Priority = 5, DamageMultiplier = 2.0f };
            resolver.Add(Raw(), attacker, body);
            resolver.Add(Raw(), attacker, weak);

            var contacts = resolver.Resolve(0.0f, ALFaction.All);

            Assert.AreEqual(1, contacts.Count);
            Assert.AreEqual(3, contacts[0].Region);
            Assert.AreEqual(2.0f, contacts[0].RegionMultiplier);
        }

        [Test]
        public void PerActivationは同じ番号では一度だけ_番号が変わるとまた当たる()
        {
            var resolver = new ALHitResolver();
            var attacker = new FakeExtraData() { HitHistoryMode = ALHitHistoryMode.PerActivation, SerialNumber = 10 };

            resolver.Add(Raw(), attacker, null);
            Assert.AreEqual(1, resolver.Resolve(0.0f, ALFaction.All).Count);

            resolver.Add(Raw(), attacker, null);
            Assert.AreEqual(0, resolver.Resolve(1.0f, ALFaction.All).Count);

            // HitBox がまた出た(番号が変わった)
            attacker.SerialNumber = 11;
            resolver.Add(Raw(), attacker, null);
            Assert.AreEqual(1, resolver.Resolve(2.0f, ALFaction.All).Count);
        }

        [Test]
        public void PerSourceは間隔をあけて当たり直す()
        {
            var resolver = new ALHitResolver();
            var attacker = new FakeExtraData() { HitHistoryMode = ALHitHistoryMode.PerSource, RepeatHitInterval = 0.5f };

            resolver.Add(Raw(), attacker, null);
            Assert.AreEqual(1, resolver.Resolve(0.0f, ALFaction.All).Count);

            // 粒が何度当たっても、間隔の間は当たらない
            attacker.SerialNumber = 99;
            resolver.Add(Raw(), attacker, null);
            resolver.Add(Raw(), attacker, null);
            Assert.AreEqual(0, resolver.Resolve(0.3f, ALFaction.All).Count);

            resolver.Add(Raw(), attacker, null);
            Assert.AreEqual(1, resolver.Resolve(0.6f, ALFaction.All).Count);
        }

        [Test]
        public void 別の攻撃は別々に当たる()
        {
            var resolver = new ALHitResolver();
            var a = new FakeExtraData() { HitHistoryMode = ALHitHistoryMode.PerSource };
            var b = new FakeExtraData() { HitHistoryMode = ALHitHistoryMode.PerSource };
            resolver.Add(Raw(), a, null);
            resolver.Add(Raw(), b, null);

            Assert.AreEqual(2, resolver.Resolve(0.0f, ALFaction.All).Count);
        }

        [Test]
        public void 部位の倍率が0以下なら1とみなす()
        {
            var contact = new ALHitContact() { HurtBox = new FakeExtraData() { DamageMultiplier = 0.0f } };
            Assert.AreEqual(1.0f, contact.RegionMultiplier);
            Assert.AreEqual(1.0f, new ALHitContact().RegionMultiplier);
        }

        [Test]
        public void ヒット履歴の古い記録を捨てる()
        {
            var history = new ALHitHistory();
            history.Mark(1, 0.0f);
            history.Mark(2, 10.0f);

            history.Prune(now: 15.0f, keepTime: 10.0f);

            Assert.AreEqual(1, history.Count);
            Assert.IsTrue(history.CanHit(1, 0.0f, 15.0f));
            Assert.IsFalse(history.CanHit(2, 0.0f, 15.0f));
        }

        [Test]
        public void 当たりのダメージ情報に攻撃と部位が入る()
        {
            var attacker = new FakeExtraData() { AttackId = 2001 };
            var contact = new ALHitContact() { Attacker = attacker, HurtBox = new FakeExtraData() { Region = 2, DamageMultiplier = 1.5f } };

            var info = ALDamageInfo.FromHit(contact, 3.0f);

            Assert.AreEqual(ALDamageKind.Hit, info.Kind);
            Assert.AreEqual(3.0f, info.Value);
            Assert.AreEqual(2001, info.AttackId);
            Assert.AreSame(attacker, info.Source);
            Assert.AreEqual(2, info.Region);
            Assert.AreEqual(1.5f, info.RegionMultiplier);
        }
    }
}
#nullable restore
