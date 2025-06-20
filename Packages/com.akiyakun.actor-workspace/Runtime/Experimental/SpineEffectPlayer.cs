using UnityEngine;
using Spine;
using Spine.Unity;

public class SpineEffectPlayer : MonoBehaviour
{
    public string prefabBasePath = "Effects"; // Resources/Effects 内にプレハブ配置
    public Transform effectRoot;              // 再生位置（未指定なら this）

    void Awake()
    {
        var sa = GetComponent<SkeletonAnimation>();
        if (sa != null)
            sa.AnimationState.Event += OnSpineEvent;

        if (effectRoot == null)
            effectRoot = this.transform;
    }

    void OnSpineEvent(TrackEntry entry, Spine.Event e)
    {
        // string effectName = e.Data.Name; // または e.String / e.Data.AudioPath など
        string effectName = "TestEffect";

        if (!string.IsNullOrEmpty(effectName))
        {
            PlayEffect(effectName);
        }
    }

    void PlayEffect(string effectName)
    {
        string path = System.IO.Path.Combine(prefabBasePath, effectName); // 例: "Effects/Explosion"

        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab != null)
        {
            GameObject effect = Instantiate(prefab, effectRoot.position, Quaternion.identity);
            // Destroy(effect, 5f); // 5秒で自動破棄（必要に応じて）
        }
        else
        {
            Debug.LogWarning($"エフェクト '{path}' が Resources に見つかりませんでした。");
        }
    }
}
