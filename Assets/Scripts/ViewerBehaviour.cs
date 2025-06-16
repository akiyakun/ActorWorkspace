using Project.InAppDebug;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using afl;
using Spine;
using Spine.Unity;

using afl.MasterData;
using ActorWorkspace.InAppDebug;

public class ViewerBehaviour : MonoBehaviour, ISceneBehaviour
{
    public ActorWorkspaceGUI actorWorkspaceGUI;

    public bool IsInitialized { get; private set; }

    public async UniTask<bool> InitializeAsync(CancellationToken cancellationToken)
    {
        await UniTask.Yield();

#if UNITY_EDITOR
        actorWorkspaceGUI.ActorAssetDatabase = new Project.InAppDebug.Editor.ActorAssetDatabaseInEditor();
#else
        // actorWorkspaceGUI.ActorAssetDatabase = new Project.InAppDebug.ActorAssetDatabaseInReference();
        actorWorkspaceGUI.ActorAssetDatabase = GetComponent<ActorAssetDatabaseInReference>();
#endif

        // TextureLoader textureLoader = new();
        // Atlas atlas = new Atlas("myAtlas.atlas", textureLoader);
        // AtlasAttachmentLoader attachmentLoader = new AtlasAttachmentLoader(atlas);
        // SkeletonJson json = new SkeletonJson(attachmentLoader);
        // SkeletonData skeletonData = json.readSkeletonData("mySkeleton.json");

        await actorWorkspaceGUI.InitAsync();

        return true;
    }

    public void Terminate()
    {
    }

    public void DoUpdate()
    {
    }

}
