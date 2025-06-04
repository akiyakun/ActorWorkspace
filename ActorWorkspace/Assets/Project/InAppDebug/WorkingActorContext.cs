using UnityEngine;
using afl;

namespace Project.InAppDebug
{
    // 現在ビューワーで読み込んでいるアクターの情報
    public class WorkingActorContext
    {
        public GameObject GameObject;

        public void Release()
        {
            if (GameObject != null)
            {
                Object.DestroyImmediate(GameObject);
                GameObject = null;
            }
        }
    }
}