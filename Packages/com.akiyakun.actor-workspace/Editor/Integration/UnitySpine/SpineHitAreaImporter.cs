// FIXME: 使っていないので、ファイルごとコメントアウトした(2026-11-10)。
//        攻撃ごとのデータ(AttackData)の形が決まったら、それに合わせて戻す。
//        HurtBox の部位・倍率・優先度(hurtBoxes)と、攻撃の番号の対応表(attackBindings)。extra_data.json から SpineExtraData に取り込んでいた

// #nullable enable
// using System.IO;
// using UnityEngine;
// using ActorWorkspace.UnitySpine;
//
// namespace ActorWorkspace.Editor.UnitySpine
// {
//     // Spine フォルダの extra_data.json から、当たり判定の追加情報(HurtBox の部位・攻撃の対応表)を SpineExtraData に取り込む
//     // SpineAssetPostprocessor が取り込みのたびに呼ぶ(SpineExtraData は取り込みのたびに作り直される)
//     //
//     // extra_data.json の書き方(どちらも省略できる。ほかの項目はプロジェクトの取り込み(UnitySpineImportCallback)が読む)
//     // {
//     //   "hurtBoxes": [ { "name": "HurtBox1", "region": 1, "damageMultiplier": 1.0, "priority": 0 } ],
//     //   "attackBindings": [ { "localId": 1, "attackId": 2001, "hitBoxNames": ["HitBox1"], "repeatHitInterval": 0.0 } ]
//     // }
//     public static class SpineHitAreaImporter
//     {
//         [System.Serializable]
//         class HurtBoxJson
//         {
//             public string name = "";
//             public int region;
//             public float damageMultiplier = 1.0f;
//             public int priority;
//         }
//
//         [System.Serializable]
//         class AttackBindingJson
//         {
//             public int localId;
//             public int attackId;
//             public string[] hitBoxNames = System.Array.Empty<string>();
//             public float repeatHitInterval;
//         }
//
//         [System.Serializable]
//         class ExtraDataJson
//         {
//             public HurtBoxJson[] hurtBoxes = System.Array.Empty<HurtBoxJson>();
//             public AttackBindingJson[] attackBindings = System.Array.Empty<AttackBindingJson>();
//         }
//
//         // skeletonDataAssetPath と同じフォルダの extra_data.json を読む。無ければ何もしない
//         public static void Import(string skeletonDataAssetPath, SpineExtraDataScriptableObject extraData)
//         {
//             string dir = Path.GetDirectoryName(skeletonDataAssetPath) ?? "";
//             string jsonPath = Path.Combine(dir, SpineUtilityEditor.ExtraDataJsonFileName);
//             if (File.Exists(jsonPath) == false) return;
//
//             Apply(File.ReadAllText(jsonPath), extraData, jsonPath);
//         }
//
//         public static void Apply(string jsonText, SpineExtraDataScriptableObject extraData, string sourceName = "")
//         {
//             ExtraDataJson? json;
//             try
//             {
//                 json = JsonUtility.FromJson<ExtraDataJson>(jsonText);
//             }
//             catch (System.Exception e)
//             {
//                 Debug.LogError($"[SpineHitAreaImporter] extra_data.json を読めません。{sourceName}\n{e}");
//                 return;
//             }
//             if (json == null) return;
//
//             foreach (var item in json.hurtBoxes ?? System.Array.Empty<HurtBoxJson>())
//             {
//                 if (string.IsNullOrEmpty(item.name))
//                 {
//                     Debug.LogError($"[SpineHitAreaImporter] hurtBoxes の name が空です。{sourceName}");
//                     continue;
//                 }
//                 extraData.AddHurtBoxInfo(new AWHurtBoxInfo()
//                 {
//                     Name = item.name,
//                     Region = item.region,
//                     DamageMultiplier = item.damageMultiplier,
//                     Priority = item.priority,
//                 });
//             }
//
//             foreach (var item in json.attackBindings ?? System.Array.Empty<AttackBindingJson>())
//             {
//                 extraData.AddAttackBinding(new AWAttackBinding()
//                 {
//                     LocalId = item.localId,
//                     AttackId = item.attackId,
//                     HitBoxNames = item.hitBoxNames ?? System.Array.Empty<string>(),
//                     RepeatHitInterval = item.repeatHitInterval,
//                 });
//             }
//         }
//     }
// }
// #nullable restore
