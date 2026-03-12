#if UNITY_EDITOR
#nullable enable
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BehaviorDesigner.Runtime;
using afl.BehaviorTask;
using System.Linq;

namespace Project.BehaviorTask
{
    public static class ExternalBehaviorTreeEx
    {
        public static void RemoveVariable(this ExternalBehaviorTree self, string variableName)
        {
            // 削除したい変数名
            // string variableNameToRemove = "Animator";

            var variable = self.GetVariable(variableName);
            if (variable == null) return;

            // 現在の変数リストを取得
            var behaviorSource = self.GetBehaviorSource();
            var variableList = behaviorSource.GetAllVariables();

            // 削除対象を除外した新しいリストを作成
            // var updatedVariables = currentVariables
            //     .Where(v => v.Name != variableName)
            //     .ToList();
            variableList.Remove(variable);

            // 更新されたリストを設定
            behaviorSource.SetAllVariables(variableList);

            // // アセットを保存
            // EditorUtility.SetDirty(self);
            // AssetDatabase.SaveAssets();
        }

    }
}
#nullable restore
#endif