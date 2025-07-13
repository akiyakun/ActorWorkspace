using Project.InAppDebug;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using afl;
using Spine;
using Spine.Unity;

using afl.MasterData;
using ActorWorkspace.InAppDebug;

namespace Project.InAppDebug
{
    public class ActorWorkspaceSceneBehaviour : afl.SceneMonoBehaviour
    {
        [SerializeField] ActorWorkspaceGUI actorWorkspaceGUI;

        // public bool IsInitialized { get; private set; }

        protected override async UniTask<int> OnInitializeAsync(CancellationToken cancellationToken = default)
        {
            Debug.Assert(actorWorkspaceGUI != null, "ActorWorkspaceGUI Prefabの参照を設定してください。");

#if UNITY_EDITOR
            // actorWorkspaceGUI.ActorAssetDatabase = new Project.InAppDebug.Editor.ActorAssetDatabaseInEditor();
            actorWorkspaceGUI.SetContextProvider(new ActorWorkspaceGUIContextProvider());
#else
        // actorWorkspaceGUI.ActorAssetDatabase = new Project.InAppDebug.ActorAssetDatabaseInReference();
        actorWorkspaceGUI.ActorAssetDatabase = GetComponent<ActorAssetDatabaseInReference>();
#endif

            // TextureLoader textureLoader = new();
            // Atlas atlas = new Atlas("myAtlas.atlas", textureLoader);
            // AtlasAttachmentLoader attachmentLoader = new AtlasAttachmentLoader(atlas);
            // SkeletonJson json = new SkeletonJson(attachmentLoader);
            // SkeletonData skeletonData = json.readSkeletonData("mySkeleton.json");

            // return UniTask.FromResult<int>(0);
            return await actorWorkspaceGUI.InitializeAsync(cancellationToken: cancellationToken);
        }

        // public void Terminate()
        // {
        // }

        // public void DoUpdate()
        // {
        // }

        protected override void OnStateUpdate()
        {
            // switch ((State)CurrentState)
            // {
            // }
        }

    }
}
