#nullable enable
using System.Collections.Generic;
using UnityEngine;
using afl;
using afl.Service.Effects;
using ActorWorkspace.ArcaneLedger.ActorBehaviour;

namespace ActorWorkspace.ArcaneLedger
{
    // パーティクルの弾の当たりを調べる EffectModifier(2D)。弾の ParticleSystem に付ける
    // 当たりの情報(IALCollisionExtraData。EMCollisionExtraData の派生など)も、同じ GameObject に付けておく
    //
    // 毎フレーム、粒がそのフレームに進んだ線を当たりの形(Shape。円か、進む向きに伸びたカプセル)で調べ(Physics2D.CircleCast・CapsuleCast)、近い順に次のようにする
    // - 当たる相手(TargetLayers)の HurtBox: 相手のルート(Rigidbody2D のある GameObject)の ICollisionDetector に ShurikenAtPosition で知らせる
    //   (ShurikenCollisionEventDispatcher と同じ形。ALHurtBoxBehaviour の DetectMode=Main が受け取る)。貫通しない弾は粒を消す
    // - 地形(TerrainLayers): 粒を消す。PassThroughEffectors なら PlatformEffector2D の付いたもの(すり抜け床)は抜ける
    // 粒を消したところ・相手に当たったところで、Sub Emitters の HitSubEmitterIndex 番(種類は Manual)を出す
    //
    // MEMO: ParticleSystem の Collision モジュールは、トリガーのコライダー(HurtBox)に当たらない。そのため自前で調べる
    //       点ではなく線で調べるので、速い弾でも HurtBox を飛び越えない
    // MEMO: 同じ粒が同じ相手に 2 回知らせないよう、粒(randomSeed)と相手のルートの組で覚える(貫通弾・1 回の再生で何発も出す弾のため)
    //       ALHitResolver のヒット履歴とは別。当たりの情報の HitHistoryMode を None にすると、粒ごとの当たりがそのまま 1 回ずつ当たる
    // MEMO: 粒の位置・速さをワールドの値として扱うので、ParticleSystem の Simulation Space は World にする
    //
    // See also: EMCollisionExtraData.cs, afl の ShurikenCollisionEventDispatcher.cs
    [RequireComponent(typeof(ParticleSystem))]
    public class EMHitBoxDetector : EffectModifierComponent
    {
        [Header("当たり")]
        [Tooltip("当たる相手の HurtBox のレイヤー")]
        [SerializeField] LayerMask targetLayers;
        [Tooltip("当たると消える地形のレイヤー")]
        [SerializeField] LayerMask terrainLayers;
        [Tooltip("PlatformEffector2D の付いた地形(すり抜け床)を抜ける")]
        [SerializeField] bool passThroughEffectors = true;
        [Tooltip("相手に当たっても消えない(貫通弾)。同じ相手には 1 回だけ当たる")]
        [SerializeField] bool pierce;
        [Tooltip("当たりの形。Circle: 粒の中心から半径 Radius の円。Capsule: 粒の進む向きに長さ CapsuleLength、太さ Radius × 2 のカプセル")]
        [SerializeField] HitShape shape = HitShape.Circle;
        [Tooltip("当たりの半径(粒の中心から)。Capsule のときは太さの半分")]
        [SerializeField] float radius = 0.15f;
        [Tooltip("Capsule の長さ(粒の進む向き。両端の丸みを含む全体の長さ)。Radius × 2 より短いときは Radius × 2(円と同じ)")]
        [SerializeField] float capsuleLength = 0.6f;

        [Header("演出")]
        [Tooltip("当たったところで出すサブエミッターの番号(種類は Manual にする)。-1 なら出さない")]
        [SerializeField] int hitSubEmitterIndex = 0;

        public override UpdateFlags UpdateFlags => UpdateFlags.LateUpdate;

        public LayerMask TargetLayers { get => targetLayers; set => targetLayers = value; }
        public LayerMask TerrainLayers { get => terrainLayers; set => terrainLayers = value; }
        public bool PassThroughEffectors { get => passThroughEffectors; set => passThroughEffectors = value; }
        public bool Pierce { get => pierce; set => pierce = value; }
        public float Radius { get => radius; set => radius = value; }
        public HitShape Shape { get => shape; set => shape = value; }
        public float CapsuleLength { get => capsuleLength; set => capsuleLength = value; }
        public bool DrawGizmos { get => drawGizmos; set => drawGizmos = value; }

        // 当たりの形
        public enum HitShape
        {
            // 円(半径 Radius)
            Circle,
            // 粒の進む向きに伸びたカプセル(長さ CapsuleLength、太さ Radius × 2)
            Capsule,
        }

        protected ParticleSystem Shuriken { get; private set; } = null!;
        // 同じ GameObject の当たりの情報(陣営を見る。無ければ陣営は見ない)
        protected IALCollisionExtraData? Source { get; private set; }

        ParticleSystem.Particle[] particles = System.Array.Empty<ParticleSystem.Particle>();
        readonly RaycastHit2D[] hits = new RaycastHit2D[32];
        static readonly IComparer<RaycastHit2D> DistanceComparer = Comparer<RaycastHit2D>.Create((a, b) => a.distance.CompareTo(b.distance));

        // 粒(randomSeed)ごとに、当たった相手のルート(InstanceID)
        readonly Dictionary<uint, List<int>> hitHistory = new();
        readonly HashSet<uint> aliveSeeds = new();
        readonly List<uint> removeSeeds = new();
        readonly Stack<List<int>> listPool = new();

        protected override bool OnSetup()
        {
            Shuriken = GetComponent<ParticleSystem>();
            Source = GetComponent<IALCollisionExtraData>();
#if UNITY_EDITOR
            if (Shuriken.main.simulationSpace != ParticleSystemSimulationSpace.World)
            {
                Debug.LogWarning($"EMHitBoxDetector: 弾の ParticleSystem の Simulation Space を World にしてください。\"{gameObject.GetHierarchyPath()}\"");
            }
#endif
            return true;
        }

        protected override void OnRestore()
        {
            ClearHistory();
        }

        protected override void Evaluate(float deltaTime)
        {
            int count = Shuriken.particleCount;
            if (count == 0)
            {
                if (hitHistory.Count > 0) ClearHistory();
                return;
            }
            if (particles.Length < count) particles = new ParticleSystem.Particle[Mathf.Max(count, Shuriken.main.maxParticles)];
            count = Shuriken.GetParticles(particles, count);

            int mask = targetLayers.value | terrainLayers.value;
            var filter = new ContactFilter2D { useTriggers = true, useLayerMask = true, layerMask = mask };
            bool checkHits = deltaTime > 0.0f && mask != 0;
            bool changed = false;
            for (int i = 0; i < count; i++)
            {
                if (particles[i].remainingLifetime <= 0.0f) continue;
                if (checkHits && Check(ref particles[i], deltaTime, filter)) changed = true;
            }
            if (changed) Shuriken.SetParticles(particles, count);

            // 消えた粒の履歴を捨てる(履歴があるときだけ、生きている粒を数える)
            if (hitHistory.Count > 0)
            {
                aliveSeeds.Clear();
                for (int i = 0; i < count; i++)
                {
                    if (particles[i].remainingLifetime > 0.0f) aliveSeeds.Add(particles[i].randomSeed);
                }
                removeSeeds.Clear();
                foreach (var seed in hitHistory.Keys)
                {
                    if (aliveSeeds.Contains(seed) == false) removeSeeds.Add(seed);
                }
                foreach (var seed in removeSeeds)
                {
                    ReleaseList(hitHistory[seed]);
                    hitHistory.Remove(seed);
                }
            }
        }

        // 粒がそのフレームに進んだ線を調べる。粒を書き換えたら true
        bool Check(ref ParticleSystem.Particle particle, float deltaTime, ContactFilter2D filter)
        {
            Vector2 position = particle.position;
            // MEMO: Velocity over Lifetime・Force などの分も含めた速さ
            Vector2 velocity = particle.totalVelocity;
            float speed = velocity.magnitude;
            if (speed <= 0.0f) return false;

            // 出たばかりの粒は、出た位置より後ろを調べない
            float age = Mathf.Max(particle.startLifetime - particle.remainingLifetime, 0.0f);
            float distance = speed * Mathf.Min(deltaTime, age);
            Vector2 direction = velocity / speed;
            // 当たりの形は、前の端を粒の位置にそろえる(Capsule は粒の後ろへ伸びる)。調べ始める位置で、出た位置より後ろへ伸ばさない
            var size = GetShapeSize(speed * age - distance, out float backOffset);
            Vector2 center = position - direction * backOffset;
            Vector2 origin = center - direction * distance;

            int hitCount = Cast(origin, direction, distance, filter, size);
            if (hitCount == 0) return false;
            // MEMO: 結果が近い順に並んでいるかは、この形の CircleCast・CapsuleCast では保証されていないので並べ直す
            System.Array.Sort(hits, 0, hitCount, DistanceComparer);

            bool changed = false;
            for (int i = 0; i < hitCount; i++)
            {
                var hit = hits[i];
                var collider = hit.collider;
                int layerBit = 1 << collider.gameObject.layer;
                // 当たったときの粒の位置(形の前の端の丸みの中心)
                Vector2 hitPosition = hit.centroid + direction * backOffset;

                if ((layerBit & terrainLayers.value) != 0 && IsTerrain(collider))
                {
                    Kill(ref particle, hitPosition);
                    return true;
                }

                if ((layerBit & targetLayers.value) == 0) continue;
                if (TryGetTarget(collider, out var root, out var detector) == false) continue;

                // 同じ粒は、同じ相手に 1 回だけ
                if (AddHistory(particle.randomSeed, root.GetInstanceID()) == false) continue;

                // 相手に知らせるので、Me が相手、Other が弾(ShurikenCollisionEventDispatcher と同じ)
                var info = new CollisionContactInfo(CollisionEventType.ShurikenAtPosition, me: root, other: gameObject)
                {
                    Position = hitPosition,
                };
                detector.InvokeOnCollision(info);

                if (pierce == false)
                {
                    Kill(ref particle, hitPosition);
                    return true;
                }
                EmitHitEffect(ref particle, hitPosition);
                changed = true;
            }
            return changed;
        }

        // 当たりの形(中心が origin)で、direction へ distance だけ掃引する。当たった数を返す(hits に入る)
        int Cast(Vector2 origin, Vector2 direction, float distance, ContactFilter2D filter, Vector2 size)
        {
            if (shape == HitShape.Capsule && size.x > size.y)
            {
                // カプセルは進む向きに伸ばす(Horizontal のカプセルを、進む向きの角度に回す)
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                return Physics2D.CapsuleCast(origin, size, CapsuleDirection2D.Horizontal, angle, direction, filter, hits, distance);
            }
            return Physics2D.CircleCast(origin, radius, direction, filter, hits, distance);
        }

        // 当たりの形の大きさ(x: 進む向きの長さ、y: 太さ)と、形の中心が粒の位置からどれだけ後ろにあるか(backOffset)
        // traveled: 粒が出た位置から進んだ距離。Capsule は、出た位置より後ろへ伸ばさない(出たばかりの粒は短い)
        Vector2 GetShapeSize(float traveled, out float backOffset)
        {
            float width = radius * 2.0f;
            if (shape != HitShape.Capsule)
            {
                backOffset = 0.0f;
                return new Vector2(width, width);
            }
            float length = Mathf.Clamp(capsuleLength, width, width + Mathf.Max(traveled, 0.0f));
            // 前の端の丸みの中心を粒の位置にそろえる
            backOffset = length * 0.5f - radius;
            return new Vector2(length, width);
        }

        // 粒を消す地形か(既定: トリガーでない。PassThroughEffectors なら PlatformEffector2D の付いたものは抜ける)
        protected virtual bool IsTerrain(Collider2D collider)
        {
            if (collider.isTrigger) return false;
            if (passThroughEffectors && collider.usedByEffector) return false;
            return true;
        }

        // 当たる相手か。当たるなら、知らせる先(相手のルートとその ICollisionDetector)を返す
        // 既定: IALCollisionExtraData の付いたトリガー(HurtBox)で、相手のアクターに台帳(ArcaneLedgerBehaviour)があり、生きていて、陣営が当たる。
        //       撃ったアクターが分からない弾(当たりの情報の Actor が無い)は当たらない
        // MEMO: 知らせる先は相手のルート(ALHurtBoxBehaviour の DetectMode=Main が受け取る)。DetectMode=PerHurtBox の相手には届かない
        protected virtual bool TryGetTarget(Collider2D collider, out GameObject root, out ICollisionDetector detector)
        {
            root = null!;
            detector = null!;
            if (Source != null && Source.Actor == null) return false;
            if (collider.isTrigger == false || collider.TryGetComponent<IALCollisionExtraData>(out _) == false) return false;
            var body = collider.attachedRigidbody;
            if (body == null || body.TryGetComponent<ICollisionDetector>(out var found) == false) return false;
            if (body.TryGetComponent<IAWActor>(out var actor) == false) return false;
            var ledger = actor.GetLedger();
            if (ledger == null || ledger.Health == null || ledger.Health.IsAlive == false) return false;
            if (Source != null && ALFaction.CanHit(Source.Faction, ledger.HostileMask) == false) return false;

            root = body.gameObject;
            detector = found;
            return true;
        }

        void Kill(ref ParticleSystem.Particle particle, Vector2 position)
        {
            EmitHitEffect(ref particle, position);
            particle.remainingLifetime = 0.0f;
        }

        void EmitHitEffect(ref ParticleSystem.Particle particle, Vector2 position)
        {
            if (hitSubEmitterIndex < 0) return;
            var subEmitters = Shuriken.subEmitters;
            if (subEmitters.enabled == false || hitSubEmitterIndex >= subEmitters.subEmittersCount) return;

            // 当たったところから出す(粒の位置は元に戻す)
            var original = particle.position;
            particle.position = new Vector3(position.x, position.y, original.z);
            Shuriken.TriggerSubEmitter(hitSubEmitterIndex, ref particle);
            if (pierce) particle.position = original;
        }

        // 粒と相手の組を覚える。もう覚えていたら false
        bool AddHistory(uint seed, int targetId)
        {
            if (hitHistory.TryGetValue(seed, out var list) == false)
            {
                list = listPool.Count > 0 ? listPool.Pop() : new List<int>(4);
                hitHistory.Add(seed, list);
            }
            if (list.Contains(targetId)) return false;
            list.Add(targetId);
            return true;
        }

        void ClearHistory()
        {
            foreach (var list in hitHistory.Values) ReleaseList(list);
            hitHistory.Clear();
        }

        void ReleaseList(List<int> list)
        {
            list.Clear();
            listPool.Push(list);
        }

        #region Gizmo
        // Scene ビューに当たりの形と大きさを描く(当たるかどうかは描かない)
        // MEMO: OnDrawGizmos は編集中も呼ばれ、ParticleSystem のプレビュー再生中(Prefab モードなど)も粒の位置・速さが取れるので、粒と一緒に動いて見える
        //       粒が無いときは、発射位置に形と撃つ向き(Transform の +Z)を描く
        // MEMO: drawGizmos は #if の外に置く(ビルドと Editor でシリアライズする項目を変えないため)
        [Header("Gizmo")]
        [Tooltip("Scene ビューに当たりの形を描く(粒ごと。粒が無いときは発射位置に、撃つ向きと一緒に描く)")]
        [SerializeField] bool drawGizmos = true;

#if UNITY_EDITOR
        ParticleSystem.Particle[] gizmoParticles = System.Array.Empty<ParticleSystem.Particle>();
        const int GizmoCircleSegments = 24;

        void OnDrawGizmos()
        {
            if (drawGizmos == false) return;
            // ゲーム中は、選んでいる GameObject(とその下)のものだけ描く(弾がたくさん出ると見づらく、重くなるため)
            if (Application.isPlaying)
            {
                var selected = UnityEditor.Selection.activeTransform;
                if (selected == null || transform.IsChildOf(selected) == false) return;
            }
            var ps = GetComponent<ParticleSystem>();
            if (ps == null) return;

            var oldColor = Gizmos.color;
            Gizmos.color = pierce ? new Color(1.0f, 0.4f, 1.0f, 0.9f) : new Color(0.3f, 1.0f, 0.4f, 0.9f);

            int count = ps.particleCount;
            if (count > 0)
            {
                if (gizmoParticles.Length < count) gizmoParticles = new ParticleSystem.Particle[Mathf.Max(count, ps.main.maxParticles)];
                count = ps.GetParticles(gizmoParticles, count);
                // 粒の位置・速さの空間(World 以外は、その Transform の空間)
                var main = ps.main;
                Transform? space = main.simulationSpace switch
                {
                    ParticleSystemSimulationSpace.World => null,
                    ParticleSystemSimulationSpace.Custom => main.customSimulationSpace != null ? main.customSimulationSpace : transform,
                    _ => transform,
                };
                for (int i = 0; i < count; i++)
                {
                    var particle = gizmoParticles[i];
                    if (particle.remainingLifetime <= 0.0f) continue;
                    Vector3 position = space == null ? particle.position : space.TransformPoint(particle.position);
                    Vector3 velocity = space == null ? particle.totalVelocity : space.TransformVector(particle.totalVelocity);
                    Vector2 velocity2D = new Vector2(velocity.x, velocity.y);
                    float speed = velocity2D.magnitude;
                    Vector2 direction = speed > 1e-3f ? velocity2D / speed : GetFireDirection();
                    float age = Mathf.Max(particle.startLifetime - particle.remainingLifetime, 0.0f);
                    var size = GetShapeSize(speed * age, out float backOffset);
                    DrawShapeGizmo(position - (Vector3)(direction * backOffset), direction, size);
                }
            }
            else
            {
                // 粒が無いとき: 発射位置に形(前の端を発射位置にそろえる)と、撃つ向きの矢印
                Vector2 direction = GetFireDirection();
                Vector3 position = transform.position;
                var size = GetShapeSize(float.MaxValue, out float backOffset);
                DrawShapeGizmo(position - (Vector3)(direction * backOffset), direction, size);
                float length = size.x * 2.0f;
                Vector3 tip = position + (Vector3)(direction * length);
                Gizmos.DrawLine(position, tip);
                Vector3 side = new Vector3(-direction.y, direction.x, 0.0f) * (length * 0.15f);
                Vector3 back = (Vector3)(direction * (length * 0.25f));
                Gizmos.DrawLine(tip, tip - back + side);
                Gizmos.DrawLine(tip, tip - back - side);
            }

            Gizmos.color = oldColor;
        }

        // 撃つ向き(Shape を使わない ParticleSystem は、Transform の +Z へ粒を出す)
        Vector2 GetFireDirection()
        {
            var forward = transform.forward;
            var direction = new Vector2(forward.x, forward.y);
            return direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector2.right;
        }

        // 当たりの形を描く(size は GetShapeSize の値。Capsule は direction の向きに伸ばす)
        void DrawShapeGizmo(Vector3 center, Vector2 direction, Vector2 size)
        {
            float halfLength = size.x * 0.5f - radius;
            if (shape == HitShape.Circle || halfLength <= 0.0f)
            {
                DrawArcGizmo(center, radius, 0.0f, 360.0f);
                return;
            }

            Vector3 axis = new Vector3(direction.x, direction.y, 0.0f);
            Vector3 normal = new Vector3(-direction.y, direction.x, 0.0f);
            Vector3 front = center + axis * halfLength;
            Vector3 back = center - axis * halfLength;
            Gizmos.DrawLine(back + normal * radius, front + normal * radius);
            Gizmos.DrawLine(back - normal * radius, front - normal * radius);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            DrawArcGizmo(front, radius, angle - 90.0f, angle + 90.0f);
            DrawArcGizmo(back, radius, angle + 90.0f, angle + 270.0f);
        }

        // XY 平面の円弧を描く(角度は度。+X が 0 度で反時計回り)
        static void DrawArcGizmo(Vector3 center, float arcRadius, float fromDegree, float toDegree)
        {
            int segments = Mathf.Max(2, Mathf.CeilToInt(GizmoCircleSegments * Mathf.Abs(toDegree - fromDegree) / 360.0f));
            float step = (toDegree - fromDegree) / segments;
            Vector3 previous = center + ArcPoint(fromDegree, arcRadius);
            for (int i = 1; i <= segments; i++)
            {
                Vector3 next = center + ArcPoint(fromDegree + step * i, arcRadius);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }

        static Vector3 ArcPoint(float degree, float arcRadius)
        {
            float rad = degree * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0.0f) * arcRadius;
        }
#endif
        #endregion
    }
}
#nullable restore
