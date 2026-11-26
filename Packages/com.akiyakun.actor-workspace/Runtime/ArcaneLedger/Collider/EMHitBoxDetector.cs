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
    // 粒を消したところ・相手に当たったところで、HitEffects の ParticleSystem を出す(Sub Emitters に Manual で入っていればサブエミッターとして、無ければ直接)。
    // HitEffects が空なら Sub Emitters の最初の Manual を出す
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
        [Tooltip("当たったところで出す ParticleSystem(火花・煙など。いくつでも入れられる)。弾の子のパーティクルをドラッグして入れる。\n" +
                 "弾の Sub Emitters に種類 Manual で入っていればサブエミッターとして出し、入っていなければ当たったところに直接出す。\n" +
                 "空なら、弾の Sub Emitters の最初の Manual を出す(無ければ何も出さない)")]
        [SerializeField] ParticleSystem[] hitEffects = System.Array.Empty<ParticleSystem>();

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

        // 粒 1 つ分の当たりの形(ワールドの XY)。GetParticleShapes・CalculateShape が返す
        // IsCircle なら Center が中心の円。そうでなければ、Direction の向きに伸びたカプセル(Center が真ん中。Length は丸みを含む全体の長さ)
        public readonly struct ShapeInstance
        {
            public Vector2 Center { get; }
            // 進む向き(長さ 1)
            public Vector2 Direction { get; }
            // 進む向きの長さ(丸みを含む全体)。円のときは Radius × 2
            public float Length { get; }
            public float Radius { get; }

            public bool IsCircle => Length <= Radius * 2.0f + 1e-5f;

            public ShapeInstance(Vector2 center, Vector2 direction, float length, float radius)
            {
                Center = center;
                Direction = direction;
                Length = length;
                Radius = radius;
            }
        }

        protected ParticleSystem Shuriken { get; private set; } = null!;
        // 同じ GameObject の当たりの情報(陣営を見る。無ければ陣営は見ない)
        protected IALCollisionExtraData? Source { get; private set; }

        ParticleSystem.Particle[] particles = System.Array.Empty<ParticleSystem.Particle>();
        // GetParticleShapes で使う(当たりを調べる particles とは分ける)
        ParticleSystem.Particle[] shapeParticles = System.Array.Empty<ParticleSystem.Particle>();
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
            ResolveHitEffects();
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

        Vector2 GetShapeSize(float traveled, out float backOffset) => GetShapeSize(shape, radius, capsuleLength, traveled, out backOffset);

        // 当たりの形の大きさ(x: 進む向きの長さ、y: 太さ)と、形の中心が粒の位置からどれだけ後ろにあるか(backOffset)
        // traveled: 粒が出た位置から進んだ距離。Capsule は、出た位置より後ろへ伸ばさない(出たばかりの粒は短い)
        static Vector2 GetShapeSize(HitShape shape, float radius, float capsuleLength, float traveled, out float backOffset)
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

        // 粒の位置での当たりの形(Gizmo・当たり判定の可視化で描く形。当たりを調べる形は、そのフレームに進んだ分だけ短いことがある)
        // position: 粒の位置、direction: 進む向き(長さ 1)、traveled: 出た位置から進んだ距離
        public static ShapeInstance CalculateShape(HitShape shape, float radius, float capsuleLength, Vector2 position, Vector2 direction, float traveled)
        {
            var size = GetShapeSize(shape, radius, capsuleLength, traveled, out float backOffset);
            return new ShapeInstance(position - direction * backOffset, direction, size.x, radius);
        }

        // 今の粒ごとの当たりの形を results に足す(results は消さない)。足した数を返す
        // MEMO: 編集中(Setup の前)も呼べる。Simulation Space が World でないときは、その空間からワールドに直す
        // MEMO: 進む向きは粒の速さの向き。止まっている粒は撃つ向き(Transform の +Z)
        public int GetParticleShapes(List<ShapeInstance> results)
        {
            var ps = Shuriken != null ? Shuriken : GetComponent<ParticleSystem>();
            if (ps == null) return 0;
            int count = ps.particleCount;
            if (count == 0) return 0;
            if (shapeParticles.Length < count) shapeParticles = new ParticleSystem.Particle[Mathf.Max(count, ps.main.maxParticles)];
            count = ps.GetParticles(shapeParticles, count);

            // 粒の位置・速さの空間(World 以外は、その Transform の空間)
            var main = ps.main;
            Transform? space = main.simulationSpace switch
            {
                ParticleSystemSimulationSpace.World => null,
                ParticleSystemSimulationSpace.Custom => main.customSimulationSpace != null ? main.customSimulationSpace : transform,
                _ => transform,
            };
            int added = 0;
            for (int i = 0; i < count; i++)
            {
                var particle = shapeParticles[i];
                if (particle.remainingLifetime <= 0.0f) continue;
                Vector3 position = space == null ? particle.position : space.TransformPoint(particle.position);
                Vector3 velocity = space == null ? particle.totalVelocity : space.TransformVector(particle.totalVelocity);
                Vector2 velocity2D = new Vector2(velocity.x, velocity.y);
                float speed = velocity2D.magnitude;
                Vector2 direction = speed > 1e-3f ? velocity2D / speed : GetFireDirection();
                float age = Mathf.Max(particle.startLifetime - particle.remainingLifetime, 0.0f);
                results.Add(CalculateShape(shape, radius, capsuleLength, position, direction, speed * age));
                added++;
            }
            return added;
        }

        // 撃つ向き(Shape を使わない ParticleSystem は、Transform の +Z へ粒を出す)
        Vector2 GetFireDirection()
        {
            var forward = transform.forward;
            var direction = new Vector2(forward.x, forward.y);
            return direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector2.right;
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

        // 当たったところで HitEffects(空なら Sub Emitters の最初の Manual)を出す
        void EmitHitEffect(ref ParticleSystem.Particle particle, Vector2 position)
        {
            if (hitSubEmitterIndices.Count == 0 && directHitEffects.Count == 0) return;

            // サブエミッター: 当たったところから出す(粒の位置は元に戻す)
            if (hitSubEmitterIndices.Count > 0)
            {
                var original = particle.position;
                particle.position = new Vector3(position.x, position.y, original.z);
                foreach (int index in hitSubEmitterIndices)
                {
                    Shuriken.TriggerSubEmitter(index, ref particle);
                }
                if (pierce) particle.position = original;
            }

            // サブエミッターでないもの: 当たったところに直接出す(向き・形は、その ParticleSystem の Transform と Shape のまま)
            if (directHitEffects.Count > 0)
            {
                var emitParams = new ParticleSystem.EmitParams
                {
                    position = new Vector3(position.x, position.y, particle.position.z),
                    applyShapeToPosition = true,
                };
                foreach (var effect in directHitEffects)
                {
                    if (effect == null) continue;
                    effect.Emit(emitParams, GetBurstCount(effect));
                }
            }
        }

        // HitEffects を、サブエミッターとして出すもの(番号)と、直接出すものに分ける(初期化のときに 1 回)
        readonly List<int> hitSubEmitterIndices = new();
        readonly List<ParticleSystem> directHitEffects = new();

        void ResolveHitEffects()
        {
            hitSubEmitterIndices.Clear();
            directHitEffects.Clear();
            var subEmitters = Shuriken.subEmitters;
            int subCount = subEmitters.enabled ? subEmitters.subEmittersCount : 0;
            var effectRoot = Effect != null ? Effect.GameObject.transform : transform.root;

            bool hasAny = false;
            foreach (var effect in hitEffects)
            {
                if (effect == null) continue;
                hasAny = true;
                var kind = ClassifyHitEffect(effect, Shuriken, effectRoot, subEmitters, subCount, out int index);
                switch (kind)
                {
                    case HitEffectKind.SubEmitter:
                        if (hitSubEmitterIndices.Contains(index) == false) hitSubEmitterIndices.Add(index);
                        break;
                    case HitEffectKind.Direct:
                        if (directHitEffects.Contains(effect)) break;
                        directHitEffects.Add(effect);
                        DisableEmission(effect);
                        break;
                    default:
#if UNITY_EDITOR
                        // MEMO: 編集中は何度も初期化されるので、警告はゲーム中だけ(編集中は Inspector の案内で知らせる)
                        if (Application.isPlaying) Debug.LogWarning($"EMHitBoxDetector: Hit Effects の {effect.name} は{GetSkipReason(kind)}ので、出しません。\"{gameObject.GetHierarchyPath()}\"");
#endif
                        break;
                }
            }
            if (hasAny) return;

            // 空なら、Sub Emitters の最初の Manual
            for (int i = 0; i < subCount; i++)
            {
                if (subEmitters.GetSubEmitterType(i) == ParticleSystemSubEmitterType.Manual)
                {
                    hitSubEmitterIndices.Add(i);
                    return;
                }
            }
        }

        // 直接出す ParticleSystem が、エフェクトを再生したときに自分の Emission で(撃った位置に)出ないようにする。Emit() は Emission がオフでも出せる
        // MEMO: ゲーム中のシーンにあるものだけ(Prefab のアセットや Prefab モードの値を書き換えないように。そのため Prefab モードのプレビューでは撃った位置にも出る)
        void DisableEmission(ParticleSystem effect)
        {
            if (Application.isPlaying == false || effect.gameObject.scene.IsValid() == false) return;
#if UNITY_EDITOR
            if (UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(effect.gameObject.scene)) return;
#endif
            var emission = effect.emission;
            emission.enabled = false;
#if UNITY_EDITOR
            if (effect.main.simulationSpace != ParticleSystemSimulationSpace.World)
            {
                Debug.LogWarning($"EMHitBoxDetector: 当たったところに直接出す {effect.name} の Simulation Space を World にしてください(Local だと出す位置がずれる)。\"{gameObject.GetHierarchyPath()}\"");
            }
#endif
        }

        // HitEffects の 1 つをどう扱うか
        enum HitEffectKind
        {
            // 弾自身(Emission をオフにすると弾が出なくなるので、出さない)
            Self,
            // エフェクトの外のもの(ほかのエフェクトやシーンのものを書き換えないよう、出さない)
            Outside,
            // Sub Emitters に Manual 以外の種類で入っている(Birth・Death などでも出るので、ここでも出すと 2 回出る。出さない)
            OtherSubEmitter,
            // Sub Emitters に Manual で入っている: サブエミッターとして出す
            SubEmitter,
            // Sub Emitters に入っていない: 当たったところに直接出す
            Direct,
        }

        // index は SubEmitter のときの Sub Emitters の番号(ほかは -1)
        static HitEffectKind ClassifyHitEffect(ParticleSystem effect, ParticleSystem self, Transform effectRoot,
            ParticleSystem.SubEmittersModule subEmitters, int subCount, out int index)
        {
            index = -1;
            if (effect == self) return HitEffectKind.Self;
            if (effect.transform.IsChildOf(effectRoot) == false) return HitEffectKind.Outside;
            int manualIndex = -1;
            for (int i = 0; i < subCount; i++)
            {
                if (subEmitters.GetSubEmitterSystem(i) != effect) continue;
                if (subEmitters.GetSubEmitterType(i) != ParticleSystemSubEmitterType.Manual) return HitEffectKind.OtherSubEmitter;
                if (manualIndex < 0) manualIndex = i;
            }
            if (manualIndex < 0) return HitEffectKind.Direct;
            index = manualIndex;
            return HitEffectKind.SubEmitter;
        }

#if UNITY_EDITOR
        static string GetSkipReason(HitEffectKind kind) => kind switch
        {
            HitEffectKind.Self => "弾の ParticleSystem 自身な",
            HitEffectKind.Outside => "このエフェクトの中のものではない",
            HitEffectKind.OtherSubEmitter => "弾の Sub Emitters に Manual 以外の種類で入っている(2 回出てしまう)",
            _ => "",
        };
#endif

        // 直接出すときの粒の数(Emission の Burst の数の合計。Burst の Probability も見る。Burst が無ければ 1)
        // MEMO: Count が Curve のときは時刻 0 の値。Cycles(繰り返し)は見ない(1 回分だけ出す)
        // MEMO: UnityEngine.Random の状態を変えないよう、自前の乱数を使う(ゲーム側で Random.InitState を使っていても、乱数の並びがずれないように)
        static readonly System.Random burstRandom = new();

        static int GetBurstCount(ParticleSystem effect)
        {
            var emission = effect.emission;
            int burstCount = emission.burstCount;
            if (burstCount == 0) return 1;
            int total = 0;
            for (int i = 0; i < burstCount; i++)
            {
                var burst = emission.GetBurst(i);
                if (burst.probability < 1.0f && (float)burstRandom.NextDouble() > burst.probability) continue;
                total += Mathf.RoundToInt(burst.count.Evaluate(0.0f, (float)burstRandom.NextDouble()));
            }
            return total;
        }

#if UNITY_EDITOR
        // Inspector の案内用: HitEffects のうち、当たったところに直接出すもの(direct)、そのうち Simulation Space が World でないもの(local)、
        // 出さないもの(skipped。ParticleSystem と理由)
        public void GetHitEffectInfo(List<ParticleSystem> direct, List<ParticleSystem> local, List<(ParticleSystem effect, string reason)> skipped)
        {
            direct.Clear();
            local.Clear();
            skipped.Clear();
            var ps = GetComponent<ParticleSystem>();
            if (ps == null) return;
            var subEmitters = ps.subEmitters;
            int subCount = subEmitters.enabled ? subEmitters.subEmittersCount : 0;
            var effectRoot = GetComponentInParent<IEffect>() is Component root ? root.transform : transform.root;
            foreach (var effect in hitEffects)
            {
                if (effect == null) continue;
                var kind = ClassifyHitEffect(effect, ps, effectRoot, subEmitters, subCount, out _);
                switch (kind)
                {
                    case HitEffectKind.SubEmitter:
                        break;
                    case HitEffectKind.Direct:
                        if (direct.Contains(effect)) break;
                        direct.Add(effect);
                        if (effect.main.simulationSpace != ParticleSystemSimulationSpace.World) local.Add(effect);
                        break;
                    default:
                        if (skipped.Exists(item => item.effect == effect) == false) skipped.Add((effect, GetSkipReason(kind)));
                        break;
                }
            }
        }
#endif

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
        readonly List<ShapeInstance> gizmoShapes = new();
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

            // MEMO: 形は XY 平面に描く(Z は弾の ParticleSystem の位置)
            float z = transform.position.z;
            if (ps.particleCount > 0)
            {
                // MEMO: 寿命の尽きた粒だけのときは何も描かない(粒が無いときの形と矢印は出さない)
                gizmoShapes.Clear();
                GetParticleShapes(gizmoShapes);
                foreach (var instance in gizmoShapes) DrawShapeGizmo(instance, z);
            }
            else
            {
                // 粒が無いとき: 発射位置に形(前の端を発射位置にそろえる)と、撃つ向きの矢印
                Vector2 direction = GetFireDirection();
                Vector3 position = transform.position;
                var instance = CalculateShape(shape, radius, capsuleLength, position, direction, float.MaxValue);
                DrawShapeGizmo(instance, z);
                float length = instance.Length * 2.0f;
                Vector3 tip = position + (Vector3)(direction * length);
                Gizmos.DrawLine(position, tip);
                Vector3 side = new Vector3(-direction.y, direction.x, 0.0f) * (length * 0.15f);
                Vector3 back = (Vector3)(direction * (length * 0.25f));
                Gizmos.DrawLine(tip, tip - back + side);
                Gizmos.DrawLine(tip, tip - back - side);
            }

            Gizmos.color = oldColor;
        }

        // 当たりの形を描く(Capsule は Direction の向きに伸ばす)
        static void DrawShapeGizmo(ShapeInstance instance, float z)
        {
            Vector3 center = new Vector3(instance.Center.x, instance.Center.y, z);
            float shapeRadius = instance.Radius;
            if (instance.IsCircle)
            {
                DrawArcGizmo(center, shapeRadius, 0.0f, 360.0f);
                return;
            }

            Vector2 direction = instance.Direction;
            float halfLength = instance.Length * 0.5f - shapeRadius;
            Vector3 axis = new Vector3(direction.x, direction.y, 0.0f);
            Vector3 normal = new Vector3(-direction.y, direction.x, 0.0f);
            Vector3 front = center + axis * halfLength;
            Vector3 back = center - axis * halfLength;
            Gizmos.DrawLine(back + normal * shapeRadius, front + normal * shapeRadius);
            Gizmos.DrawLine(back - normal * shapeRadius, front - normal * shapeRadius);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            DrawArcGizmo(front, shapeRadius, angle - 90.0f, angle + 90.0f);
            DrawArcGizmo(back, shapeRadius, angle + 90.0f, angle + 270.0f);
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
