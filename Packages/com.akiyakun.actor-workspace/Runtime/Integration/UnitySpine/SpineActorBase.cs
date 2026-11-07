#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Spine.Unity;
using afl;

namespace ActorWorkspace.UnitySpine
{
    public abstract class SpineActorBase<TActorParam>
        : AWActorBase<TActorParam, IAWAnimationController, SpineSkin>
        // where TActorContextProvider : AWActorContextProvider
        where TActorParam : class, IAWActorParam
        // where TActorDisplay : SpineActorDisplay, new()
    {
        // public override TActorContextProvider ActorContextProvider { get; protected set; } = null!;

        // public override TActorParam ActorParam { get; protected set; }
        // public override TActorDisplay ActorDisplay { get; protected set; } = null!;
        // public override IAWAnimationController AnimationController { get; protected set; } = null!;
        public override IReadOnlyList<SpineSkin> SkinList => skinList;
        public override IReadOnlyList<int> SkinIndices => skinIndices;

        protected Spine.Skeleton Skeleton => skeletonAnimationInterface.Skeleton;

        ISkeletonAnimation skeletonAnimationInterface = null!;
        // SpineSkeletonAnimationController spineSkeletonAnimationController = null!;
        List<SpineSkin> skinList = new();
        List<int> skinIndices = new();

        // 初期化したときのスキン(Restore で戻す)
        Spine.Skin? initialSkin;
        readonly List<int> initialSkinIndices = new();

        // スキンを変えたあと、次にアニメーションを当てる前にスロットをセットアップポーズに戻すか
        bool setSlotsToSetupPoseBeforeApply;
        // スキンを変える前のシーケンスのコマ(スロットの番号ごと)
        int[] sequenceIndices = System.Array.Empty<int>();
        // SetSkin(int) で SetSkins に渡す番号(SetSkins は中身を写すので使い回す)
        readonly int[] singleSkinIndex = new int[1];

        protected override async UniTask<int> CreateAnimationController(CancellationToken cancellationToken)
        {
            // SkeletonMecanim か SkeletonAnimation のどちらかを取得
            if (GetComponentInChildren<SkeletonMecanim>(includeInactive: false) is SkeletonMecanim skeletonMecanim)
            {
                skeletonAnimationInterface = skeletonMecanim;
                Debug.Assert(skeletonAnimationInterface != null);
                skeletonMecanim.BeforeApply += OnBeforeApply;

                var spineMecanimAnimationController = new SpineMecanimAnimationController(skeletonMecanim);
                AnimationController = spineMecanimAnimationController as IAWAnimationController;
            }
            else if (GetComponentInChildren<SkeletonAnimation>(includeInactive: false) is SkeletonAnimation skeletonAnimation)
            {
                skeletonAnimationInterface = skeletonAnimation;
                Debug.Assert(skeletonAnimationInterface != null);
                skeletonAnimation.BeforeApply += OnBeforeApply;

                var spineSkeletonAnimationController = new SpineSkeletonAnimationController(skeletonAnimation);
                AnimationController = spineSkeletonAnimationController as IAWAnimationController;
            }
            else
            {
                Debug.Assert(false, "SkeletonMecanim or SkeletonAnimation not found");
                return GeneralReturnCode.Failed;
            }

            if (AnimationController == null)
            {
                Debug.Assert(false, "Failed to create AnimationController");
                return GeneralReturnCode.Failed;
            }

            return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        }

        protected override async UniTask<int> InnerInitializeAsync(CancellationToken cancellationToken)
        {
            // ActorContextProvider = (TActorContextProvider)awActorContextProvider;
            // if (ActorContextProvider == null) return GeneralReturnCode.Failed;

            // AnimationControllerの初期化
            {
                if (AnimationController is SpineMecanimAnimationController mecanim)
                {
                    if (await mecanim.InitializeAsync(cancellationToken) is int ret && ret < 0) return ret;
                }
                else if (AnimationController is SpineSkeletonAnimationController skeleton)
                {
                    if (await skeleton.InitializeAsync(cancellationToken) is int ret && ret < 0) return ret;
                }
                else
                {
                    Debug.Assert(false, "Invalid AnimationController type");
                    return GeneralReturnCode.Failed;
                }
            }

            // スキンの初期化
            // MEMO: Skins.Items は配列の大きさが要素数より大きいことがあるので、Count まで回す
            var skins = Skeleton.Data.Skins;
            skinList.Clear();
            for (int i = 0; i < skins.Count; i++)
            {
                var skin = new SpineSkin(skins.Items[i]);
                skinList.Add(skin);
            }
            initialSkin = Skeleton.Skin;
            initialSkinIndices.Clear();
            if (FindSkinIndex(initialSkin) is int initialSkinIndex && initialSkinIndex >= 0) initialSkinIndices.Add(initialSkinIndex);
            skinIndices.Clear();
            skinIndices.AddRange(initialSkinIndices);

            // ActorDisplayの初期化
            // {
            //     ActorDisplay = new TActorDisplay();
            //     int ret = await ActorDisplay.InitializeAsync(this, AnimationController, cancellationToken: cancellationToken);
            //     if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Canceled;
            //     if (ret < 0) return ret;
            // }

            return await UniTask.FromResult(GeneralReturnCode.Succeeded);
        }

        // public void Restore()
        // {
        //     ActorParam?.Restore();
        // }

        public override void Dispose()
        {
            if (skeletonAnimationInterface is SkeletonMecanim skeletonMecanim) skeletonMecanim.BeforeApply -= OnBeforeApply;
            else if (skeletonAnimationInterface is SkeletonAnimation skeletonAnimation) skeletonAnimation.BeforeApply -= OnBeforeApply;

            base.Dispose();
        }

        public override void Restore()
        {
            base.Restore();

            // 初期化したときのスキンに戻す
            ApplySkin(initialSkin);
            skinIndices.Clear();
            skinIndices.AddRange(initialSkinIndices);
        }

        public override void SetSkin(int skinIndex)
        {
            singleSkinIndex[0] = skinIndex;
            SetSkins(singleSkinIndex);
        }

        public override void SetSkins(IReadOnlyList<int> skinIndices)
        {
            for (int i = 0; i < skinIndices.Count; i++)
            {
                if (skinIndices[i] < 0 || skinList.Count <= skinIndices[i])
                {
                    Debug.Assert(false, $"Invalid skin index: {skinIndices[i]}");
                    return;
                }
            }

            Spine.Skin? skin;
            if (skinIndices.Count == 0)
            {
                // スキンなし(デフォルトのスキンのアタッチメントだけ)
                skin = null;
            }
            else if (skinIndices.Count == 1)
            {
                skin = skinList[skinIndices[0]].Skin;
            }
            else
            {
                // MEMO: SkeletonData のスキンはほかのアクターと共有しているので、書き換えずに新しいスキンを作る
                var names = new string[skinIndices.Count];
                for (int i = 0; i < skinIndices.Count; i++) names[i] = skinList[skinIndices[i]].Name;
                skin = new Spine.Skin(string.Join("+", names));
                for (int i = 0; i < skinIndices.Count; i++) skin.AddSkin(skinList[skinIndices[i]].Skin);
            }

            ApplySkin(skin);
            // MEMO: SkinIndices をそのまま渡されたとき(付け直し)は、消すと中身が無くなるので写さない
            if (ReferenceEquals(skinIndices, this.skinIndices)) return;
            this.skinIndices.Clear();
            for (int i = 0; i < skinIndices.Count; i++) this.skinIndices.Add(skinIndices[i]);
        }

        // SkinList に無いスキン(実行時に作ったスキンなど)を付ける。null はスキンなし
        // SkinIndices は SkinList にあるスキンならその番号、無ければ空になる
        // MEMO: 同じ Skin のインスタンスを付け直しても Spine は何もしないので、中身を変えたスキンは新しく作って渡す
        public void SetSkin(Spine.Skin? skin)
        {
            ApplySkin(skin);
            skinIndices.Clear();
            if (FindSkinIndex(skin) is int index && index >= 0) skinIndices.Add(index);
        }

        // MEMO: スロットをセットアップポーズに戻すのは次にアニメーションを当てる直前なので、
        // スキンを変えたあと、それまでに Skeleton を直接変えた内容(アタッチメント・色など)はセットアップポーズに戻る
        void ApplySkin(Spine.Skin? skin)
        {
            // 同じスキンなら何もしない(セットアップポーズにも戻さない)
            if (Skeleton.Skin == skin) return;

            // 今付いているアタッチメントは、新しいスキンの同じ名前のアタッチメントにすぐ置き換わる
            // MEMO: アタッチメントを置き換えると、Spine はシーケンスのコマを -1(最初のコマ)に戻すので、
            // 次の更新まで最初のコマが出ないように、置き換えたシーケンスのコマを引き継ぐ
            var slots = Skeleton.Slots;
            if (sequenceIndices.Length < slots.Count) sequenceIndices = new int[slots.Count];
            for (int i = 0; i < slots.Count; i++) sequenceIndices[i] = slots.Items[i].SequenceIndex;

            Skeleton.SetSkin(skin);

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots.Items[i];
                if (slot.SequenceIndex >= 0 || sequenceIndices[i] < 0) continue;
                if (slot.Attachment is Spine.IHasTextureRegion region && region.Sequence is Spine.Sequence sequence
                    && sequenceIndices[i] < sequence.Regions.Length)
                {
                    slot.SequenceIndex = sequenceIndices[i];
                }
            }

            // MEMO: スロットをセットアップポーズに戻すのは、次にアニメーションを当てる直前にする。
            // ここで戻すと、次の更新まで、アニメーションが付けたアタッチメント・色・描画順がセットアップポーズのものになる
            // (更新のあと・描画の前に呼ぶと、1 フレームだけ待機の絵が出る)。
            // ここでアニメーションを当て直すと、同じフレームでイベント(SE など)やルートモーションが二重になる
            setSlotsToSetupPoseBeforeApply = true;
        }

        void OnBeforeApply(ISkeletonAnimation animated)
        {
            if (setSlotsToSetupPoseBeforeApply == false) return;
            setSlotsToSetupPoseBeforeApply = false;
            Skeleton.SetSlotsToSetupPose();
        }

        int FindSkinIndex(Spine.Skin? skin)
        {
            if (skin == null) return -1;
            for (int i = 0; i < skinList.Count; i++)
            {
                if (skinList[i].Skin == skin) return i;
            }
            return -1;
        }
    }
}
#nullable restore