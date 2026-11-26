#nullable enable
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ActorWorkspace.ArcaneLedger.Tests
{
    // EMHitBoxDetector の当たりの形(CalculateShape・GetParticleShapes)の EditMode テスト
    public class EMHitBoxDetectorTests
    {
        const float Tolerance = 1e-4f;

        GameObject? gameObject;

        [TearDown]
        public void TearDown()
        {
            if (gameObject != null) Object.DestroyImmediate(gameObject);
            gameObject = null;
        }

        [Test]
        public void CalculateShape_Circle_IsCenteredOnParticle()
        {
            var shape = EMHitBoxDetector.CalculateShape(EMHitBoxDetector.HitShape.Circle, 0.2f, 1.0f, new Vector2(1.0f, 2.0f), Vector2.right, 5.0f);

            Assert.That(shape.IsCircle, Is.True);
            AssertVector(new Vector2(1.0f, 2.0f), shape.Center);
            Assert.That(shape.Radius, Is.EqualTo(0.2f).Within(Tolerance));
            Assert.That(shape.Length, Is.EqualTo(0.4f).Within(Tolerance));
        }

        [Test]
        public void CalculateShape_Capsule_ExtendsBehindParticle()
        {
            // 十分に進んだ粒: 長さは CapsuleLength。前の端の丸みの中心が粒の位置
            var shape = EMHitBoxDetector.CalculateShape(EMHitBoxDetector.HitShape.Capsule, 0.1f, 1.0f, new Vector2(3.0f, 0.0f), Vector2.right, 10.0f);

            Assert.That(shape.IsCircle, Is.False);
            Assert.That(shape.Length, Is.EqualTo(1.0f).Within(Tolerance));
            // 中心は粒の位置から (長さ / 2 - 半径) だけ後ろ
            AssertVector(new Vector2(2.6f, 0.0f), shape.Center);
            AssertVector(Vector2.right, shape.Direction);
        }

        [Test]
        public void CalculateShape_Capsule_IsShortWhenJustFired()
        {
            // 出たばかりの粒は、出た位置より後ろへ伸ばさない(長さ = 太さ + 進んだ距離)
            var shape = EMHitBoxDetector.CalculateShape(EMHitBoxDetector.HitShape.Capsule, 0.1f, 1.0f, Vector2.zero, Vector2.up, 0.3f);
            Assert.That(shape.Length, Is.EqualTo(0.5f).Within(Tolerance));
            AssertVector(new Vector2(0.0f, -0.15f), shape.Center);

            // 進んでいなければ円と同じ
            var justFired = EMHitBoxDetector.CalculateShape(EMHitBoxDetector.HitShape.Capsule, 0.1f, 1.0f, Vector2.zero, Vector2.up, 0.0f);
            Assert.That(justFired.IsCircle, Is.True);
            AssertVector(Vector2.zero, justFired.Center);
        }

        [Test]
        public void CalculateShape_Capsule_ShorterThanWidthIsCircle()
        {
            var shape = EMHitBoxDetector.CalculateShape(EMHitBoxDetector.HitShape.Capsule, 0.5f, 0.6f, Vector2.zero, Vector2.right, 10.0f);
            Assert.That(shape.IsCircle, Is.True);
            Assert.That(shape.Length, Is.EqualTo(1.0f).Within(Tolerance));
        }

        [Test]
        public void GetParticleShapes_ReturnsShapePerParticle()
        {
            var detector = CreateDetector(ParticleSystemSimulationSpace.World, Vector3.zero);
            detector.Shape = EMHitBoxDetector.HitShape.Capsule;
            detector.Radius = 0.1f;
            detector.CapsuleLength = 1.0f;
            // +X へ速さ 2 で 0.25 秒進んだ粒(進んだ距離 0.5)と、-Y へ速さ 1 で 2 秒進んだ粒(進んだ距離 2)
            Emit(detector, new Vector3(1.0f, 0.0f, 0.0f), new Vector3(2.0f, 0.0f, 0.0f), age: 0.25f);
            Emit(detector, new Vector3(0.0f, 5.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f), age: 2.0f);

            var results = new List<EMHitBoxDetector.ShapeInstance>();
            int count = detector.GetParticleShapes(results);

            Assert.That(count, Is.EqualTo(2));
            Assert.That(results.Count, Is.EqualTo(2));
            // 粒の順番は決まっていないので、向きで見分ける
            var right = results.Find(r => r.Direction.x > 0.5f);
            var down = results.Find(r => r.Direction.y < -0.5f);

            // 長さ = 太さ 0.2 + 進んだ距離 0.5、中心は粒の位置から 0.35 - 0.1 だけ後ろ
            Assert.That(right.Length, Is.EqualTo(0.7f).Within(Tolerance));
            AssertVector(new Vector2(0.75f, 0.0f), right.Center);
            // 長さ = CapsuleLength、中心は粒の位置から 0.5 - 0.1 だけ後ろ(上)
            Assert.That(down.Length, Is.EqualTo(1.0f).Within(Tolerance));
            AssertVector(new Vector2(0.0f, 5.4f), down.Center);
        }

        [Test]
        public void GetParticleShapes_LocalSpace_ConvertsToWorld()
        {
            var detector = CreateDetector(ParticleSystemSimulationSpace.Local, new Vector3(10.0f, 5.0f, 0.0f));
            detector.Shape = EMHitBoxDetector.HitShape.Circle;
            detector.Radius = 0.3f;
            Emit(detector, new Vector3(1.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), age: 0.5f);

            var results = new List<EMHitBoxDetector.ShapeInstance>();
            Assert.That(detector.GetParticleShapes(results), Is.EqualTo(1));
            Assert.That(results[0].IsCircle, Is.True);
            AssertVector(new Vector2(11.0f, 5.0f), results[0].Center);
        }

        [Test]
        public void GetParticleShapes_NoParticles_AddsNothing()
        {
            var detector = CreateDetector(ParticleSystemSimulationSpace.World, Vector3.zero);
            var results = new List<EMHitBoxDetector.ShapeInstance>();
            Assert.That(detector.GetParticleShapes(results), Is.EqualTo(0));
            Assert.That(results.Count, Is.EqualTo(0));
        }

        EMHitBoxDetector CreateDetector(ParticleSystemSimulationSpace space, Vector3 position)
        {
            gameObject = new GameObject("EMHitBoxDetectorTests");
            gameObject.transform.position = position;
            var ps = gameObject.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.simulationSpace = space;
            main.maxParticles = 16;
            var emission = ps.emission;
            emission.enabled = false;
            var shape = ps.shape;
            shape.enabled = false;
            // MEMO: 編集中は、一度 Simulate して(止めた状態にして)からでないと Emit しても粒が出ない
            ps.Simulate(0.0f, true, true);
            return gameObject.AddComponent<EMHitBoxDetector>();
        }

        // 粒を 1 つ出し、出てから age 秒たったことにする(寿命 10 秒)
        static void Emit(EMHitBoxDetector detector, Vector3 position, Vector3 velocity, float age)
        {
            const float lifetime = 10.0f;
            var ps = detector.GetComponent<ParticleSystem>();
            ps.Emit(new ParticleSystem.EmitParams
            {
                position = position,
                velocity = velocity,
                startLifetime = lifetime,
                startSize = 0.1f,
            }, 1);

            var particles = new ParticleSystem.Particle[ps.particleCount];
            int count = ps.GetParticles(particles);
            // 出した粒(位置で見分ける。粒の順番は決まっていない)の残りの寿命を減らす
            int found = 0;
            for (int i = 0; i < count; i++)
            {
                if (Vector3.Distance(particles[i].position, position) > Tolerance) continue;
                particles[i].remainingLifetime = lifetime - age;
                found++;
            }
            Assert.That(found, Is.EqualTo(1), "出した粒が見つからない");
            ps.SetParticles(particles, count);
        }

        static void AssertVector(Vector2 expected, Vector2 actual)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance), $"x: expected={expected} actual={actual}");
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance), $"y: expected={expected} actual={actual}");
        }
    }
}
#nullable restore
