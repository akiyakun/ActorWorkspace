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
    // 毎フレーム、粒がそのフレームに進んだ線を円(Radius)で調べ(Physics2D.CircleCast)、近い順に次のようにする
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
        [Tooltip("当たりの半径(粒の中心から)")]
        [SerializeField] float radius = 0.15f;

        [Header("演出")]
        [Tooltip("当たったところで出すサブエミッターの番号(種類は Manual にする)。-1 なら出さない")]
        [SerializeField] int hitSubEmitterIndex = 0;

        public override UpdateFlags UpdateFlags => UpdateFlags.LateUpdate;

        public LayerMask TargetLayers { get => targetLayers; set => targetLayers = value; }
        public LayerMask TerrainLayers { get => terrainLayers; set => terrainLayers = value; }
        public bool PassThroughEffectors { get => passThroughEffectors; set => passThroughEffectors = value; }
        public bool Pierce { get => pierce; set => pierce = value; }
        public float Radius { get => radius; set => radius = value; }

        protected ParticleSystem Shuriken { get; private set; } = null!;
        // 同じ GameObject の当たりの情報(陣営を見る。無ければ陣営は見ない)
        protected IALCollisionExtraData? Source { get; private set; }

        ParticleSystem.Particle[] particles = System.Array.Empty<ParticleSystem.Particle>();
        readonly RaycastHit2D[] hits = new RaycastHit2D[16];
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
            Vector2 velocity = particle.velocity;
            float speed = velocity.magnitude;
            if (speed <= 0.0f) return false;

            // 出たばかりの粒は、出た位置より後ろを調べない
            float age = particle.startLifetime - particle.remainingLifetime;
            float distance = speed * Mathf.Min(deltaTime, Mathf.Max(age, 0.0f));
            Vector2 direction = velocity / speed;
            Vector2 origin = position - direction * distance;

            int hitCount = Physics2D.CircleCast(origin, radius, direction, filter, hits, distance);
            if (hitCount == 0) return false;
            // MEMO: 結果が近い順に並んでいるかは、この形の CircleCast では保証されていないので並べ直す
            System.Array.Sort(hits, 0, hitCount, DistanceComparer);

            bool changed = false;
            for (int i = 0; i < hitCount; i++)
            {
                var hit = hits[i];
                var collider = hit.collider;
                int layerBit = 1 << collider.gameObject.layer;

                if ((layerBit & terrainLayers.value) != 0 && IsTerrain(collider))
                {
                    Kill(ref particle, hit.centroid);
                    return true;
                }

                if ((layerBit & targetLayers.value) == 0) continue;
                if (TryGetTarget(collider, out var root, out var detector) == false) continue;

                // 同じ粒は、同じ相手に 1 回だけ
                if (AddHistory(particle.randomSeed, root.GetInstanceID()) == false) continue;

                // 相手に知らせるので、Me が相手、Other が弾(ShurikenCollisionEventDispatcher と同じ)
                var info = new CollisionContactInfo(CollisionEventType.ShurikenAtPosition, me: root, other: gameObject)
                {
                    Position = hit.centroid,
                };
                detector.InvokeOnCollision(info);

                if (pierce == false)
                {
                    Kill(ref particle, hit.centroid);
                    return true;
                }
                EmitHitEffect(ref particle, hit.centroid);
                changed = true;
            }
            return changed;
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
    }
}
#nullable restore
