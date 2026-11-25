#nullable enable
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ActorWorkspace.ArcaneLedger;

namespace ActorWorkspace.Editor.ArcaneLedger
{
    // EMHitBoxDetector の Inspector。Hit Effects のうち、弾の Sub Emitters に入っていないので直接出すものや、出さないもの(とその理由)を案内する
    // See also: EMHitBoxDetector.cs
    [CustomEditor(typeof(EMHitBoxDetector))]
    public class EMHitBoxDetectorEditor : UnityEditor.Editor
    {
        readonly List<ParticleSystem> direct = new();
        readonly List<ParticleSystem> local = new();
        readonly List<(ParticleSystem effect, string reason)> skipped = new();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var detector = (EMHitBoxDetector)target;
            detector.GetHitEffectInfo(direct, local, skipped);
            foreach (var (effect, reason) in skipped)
            {
                EditorGUILayout.HelpBox($"Hit Effects の {effect.name} は{reason}ので、出しません。", MessageType.Warning);
            }
            if (direct.Count > 0)
            {
                EditorGUILayout.HelpBox(
                    $"Hit Effects の {string.Join("、", direct.Select(effect => effect.name))} は、弾の Sub Emitters に(種類 Manual で)入っていないので、当たったところに直接出します。" +
                    "ゲーム中は、これらは自分の Emission では出ません(Prefab モードのプレビューでは、撃った位置にも出ます)。",
                    MessageType.Info);
            }
            if (local.Count > 0)
            {
                EditorGUILayout.HelpBox(
                    $"直接出す {string.Join("、", local.Select(effect => effect.name))} の Simulation Space を World にしてください(Local だと出す位置がずれます)。",
                    MessageType.Warning);
            }
        }
    }
}
#nullable restore
