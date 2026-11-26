#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl.Service.Debugger;

namespace ActorWorkspace.ArcaneLedger
{
    // ArcaneLedger の当たり判定(HurtBox・HitBox・エフェクトの HitBox)を色分けで画面に描く(デバッグ表示)
    // 描く仕組みは afl の CollisionBoxVisualizer。ここは描く対象を集めて種類に分け、色を決める
    // 色・描く種類は外(ゲーム側)から入れる(このクラスは設定ファイルを読まない)。作る・消すは CollisionBoxVisualizer.Show<T>()・Hide<T>()
    //
    // 描く対象(RefreshInterval 秒ごとに集め直す。描くのは activeInHierarchy で enabled のものだけ)
    // - ALCollisionExtraData: BoxKind が HurtBox・HitBox のもの(ALHurtBoxBehaviour・ALHitBoxBehaviour が付けたもの)。
    //   BoxKind が分からないもの(Unknown)は ClassifyUnknown で決める(既定は描かない)
    // - EMCollisionExtraData: 同じ GameObject の Collider2D をエフェクトの HitBox として描く
    // - EMHitBoxDetector: 粒ごとの当たりの形(GetParticleShapes)をエフェクトの HitBox として描く
    // HurtBox は枠線だけ、HitBox(エフェクトも)は半透明の塗り(HitBoxFillAlpha)と枠線
    //
    // See also: afl の CollisionBoxVisualizer.cs・CollisionShapeMeshBuilder.cs, ALCollisionExtraData.cs(BoxKind), EMHitBoxDetector.cs
    public class ALCollisionBoxVisualizer : CollisionBoxVisualizer
    {
        // 描く種類
        [System.Flags]
        public enum Kinds
        {
            None = 0,
            // やられ判定(ALHurtBoxBehaviour の HurtBox)
            HurtBox = 1 << 0,
            // 攻撃判定(ALHitBoxBehaviour の HitBox)
            HitBox = 1 << 1,
            // エフェクトの当たり(EMHitBoxDetector の粒、EMCollisionExtraData の付いたエフェクトのコライダーなど)
            EffectHitBox = 1 << 2,
            All = HurtBox | HitBox | EffectHitBox,
        }

        // 1 つの箱の種類
        public enum Kind
        {
            HurtBox,
            HitBox,
            EffectHitBox,
        }

        Kinds visibleKinds = Kinds.All;
        // 描く種類(変えるとすぐに描き直す)
        public Kinds VisibleKinds
        {
            get => visibleKinds;
            set
            {
                visibleKinds = value;
                Redraw();
            }
        }

        // 色(コライダーの GameObject のレイヤーから決める)。null なら既定の色(DefaultColor)
        public System.Func<int, Color>? HurtBoxColor { get; set; }
        public System.Func<int, Color>? HitBoxColor { get; set; }
        public Color EffectHitBoxColor { get; set; } = new(1.0f, 0.2f, 1.0f, 1.0f);
        public Color DefaultColor { get; set; } = new(0.3f, 1.0f, 0.3f, 1.0f);
        // HitBox の塗りの不透明度(色のアルファに掛ける)
        public float HitBoxFillAlpha { get; set; } = 0.3f;

        readonly struct ColliderEntry
        {
            public readonly Collider2D Collider;
            public readonly Kind Kind;

            public ColliderEntry(Collider2D collider, Kind kind)
            {
                Collider = collider;
                Kind = kind;
            }
        }

        readonly List<ColliderEntry> colliders = new();
        // 同じコライダーを 2 回足さない(塗りが濃くなる)
        readonly HashSet<Collider2D> addedColliders = new();
        readonly List<EMHitBoxDetector> detectors = new();
        readonly List<Collider2D> componentBuffer = new();
        readonly List<EMHitBoxDetector.ShapeInstance> shapeBuffer = new();

        // BoxKind が分からない ALCollisionExtraData をどの種類で描くか。描かないなら false(既定)
        // 例: ゲーム側の弾(ALCollisionExtraData を継承した GameObject の弾)をエフェクトの HitBox として描く
        protected virtual bool ClassifyUnknown(ALCollisionExtraData data, out Kind kind)
        {
            kind = Kind.HurtBox;
            return false;
        }

        // 箱の色
        protected virtual Color GetColor(Kind kind, int layer)
        {
            return kind switch
            {
                Kind.HurtBox => HurtBoxColor?.Invoke(layer) ?? DefaultColor,
                Kind.HitBox => HitBoxColor?.Invoke(layer) ?? DefaultColor,
                _ => EffectHitBoxColor,
            };
        }

        protected override void Collect()
        {
            colliders.Clear();
            addedColliders.Clear();
            detectors.Clear();

            // MEMO: 非アクティブのものも集める(アタッチメントが出たり消えたりする HitBox、プールに戻したエフェクト)
            foreach (var data in FindObjectsByType<ALCollisionExtraData>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                switch (data.BoxKind)
                {
                    case ALCollisionBoxKind.HurtBox:
                        AddColliders(data.gameObject, Kind.HurtBox);
                        break;
                    case ALCollisionBoxKind.HitBox:
                        AddColliders(data.gameObject, Kind.HitBox);
                        break;
                    default:
                        if (ClassifyUnknown(data, out var kind)) AddColliders(data.gameObject, kind);
                        break;
                }
            }

            // エフェクトにコライダーで当たりを付けたもの
            foreach (var data in FindObjectsByType<EMCollisionExtraData>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                AddColliders(data.gameObject, Kind.EffectHitBox);
            }

            detectors.AddRange(FindObjectsByType<EMHitBoxDetector>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        }

        void AddColliders(GameObject target, Kind kind)
        {
            target.GetComponents(componentBuffer);
            foreach (var collider in componentBuffer)
            {
                if (addedColliders.Add(collider)) colliders.Add(new ColliderEntry(collider, kind));
            }
            componentBuffer.Clear();
        }

        protected override void Draw(CollisionShapeMeshBuilder builder)
        {
            foreach (var entry in colliders)
            {
                var collider = entry.Collider;
                if (IsDrawable(collider) == false || IsVisible(entry.Kind) == false) continue;

                var color = GetColor(entry.Kind, collider.gameObject.layer);
                builder.AddCollider(collider, color, GetFillColor(entry.Kind, color));
            }

            if (IsVisible(Kind.EffectHitBox))
            {
                foreach (var detector in detectors)
                {
                    if (detector == null || detector.isActiveAndEnabled == false) continue;

                    var color = GetColor(Kind.EffectHitBox, detector.gameObject.layer);
                    var fill = GetFillColor(Kind.EffectHitBox, color);
                    shapeBuffer.Clear();
                    detector.GetParticleShapes(shapeBuffer);
                    foreach (var shape in shapeBuffer)
                    {
                        if (shape.IsCircle) builder.AddCircle(shape.Center, shape.Radius, color, fill);
                        else builder.AddCapsule(shape.Center, shape.Direction, shape.Length * 0.5f - shape.Radius, shape.Radius, color, fill);
                    }
                }
            }
        }

        bool IsVisible(Kind kind)
        {
            var flag = kind switch
            {
                Kind.HurtBox => Kinds.HurtBox,
                Kind.HitBox => Kinds.HitBox,
                _ => Kinds.EffectHitBox,
            };
            return (visibleKinds & flag) != 0;
        }

        // HurtBox は塗らない。HitBox は色のアルファに HitBoxFillAlpha を掛けて塗る
        Color GetFillColor(Kind kind, Color color)
        {
            if (kind == Kind.HurtBox) return new Color(color.r, color.g, color.b, 0.0f);
            return new Color(color.r, color.g, color.b, color.a * HitBoxFillAlpha);
        }
    }
}
#nullable restore
