#nullable enable
using NUnit.Framework;

namespace ActorWorkspace.ArcaneLedger.Tests
{
    // ArcaneLedger の EditMode テスト
    // MEMO: ActorWorkspace の Unity プロジェクトは保守していないので、ゲームのプロジェクトで動かす
    // (ゲームの Packages/manifest.json の "testables" に "com.akiyakun.actor-workspace" を足すと Test Runner に出る)
    public class ALGeneralParamTests
    {
        [Test]
        public void 作った直後は値0_係数1_増減0()
        {
            var param = new ALGeneralParam();
            for (int i = 0; i < ALGeneralParam.MaxParamCount; i++)
            {
                Assert.AreEqual(0.0f, param[i].Value);
                Assert.AreEqual(1.0f, param[i].Factor);
                Assert.AreEqual(0.0f, param[i].Delta);
            }
        }

        [Test]
        public void Restoreで初期値に戻る()
        {
            var param = new ALGeneralParam();
            param[3].Value = 5.0f;
            param[3].Factor = 0.5f;
            param[3].Delta = 2.0f;
            param.EventSetMask = 1u << 3;
            Assert.IsTrue(param.IsSetByEvent(3));

            param.Restore();

            Assert.AreEqual(0.0f, param[3].Value);
            Assert.AreEqual(1.0f, param[3].Factor);
            Assert.AreEqual(0.0f, param[3].Delta);
            Assert.IsFalse(param.IsSetByEvent(3));
        }
    }
}
#nullable restore
