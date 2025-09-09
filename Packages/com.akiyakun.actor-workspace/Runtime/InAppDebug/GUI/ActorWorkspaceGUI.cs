using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using afl;
using afl.UI;
using afl.Service.Screen;

namespace ActorWorkspace.InAppDebug
{
    public class ActorWorkspaceGUI : MonoBehaviour, IAsyncInitializable
    {
        [SerializeField] ManagedCanvas managedCanvas;

#if UNITY_EDITOR
        [SerializeField] IAsyncInitializable editorOnly;
#endif

        [SerializeField] UIForm actorWorkspaceForm;
        ActorWorkspaceFormView actorWorkspaceFormView;
        public ActorWorkspaceFormView View => actorWorkspaceFormView;

        // From IAsyncInitializable
        public bool IsInitialized { get; private set; }

        // From IAsyncInitializable
        public async UniTask<int> InitializeAsync(CancellationToken cancellationToken = default)
        {
            Debug.Assert(managedCanvas != null);

#if UNITY_EDITOR
            if (editorOnly != null)
            {
                int ret = await editorOnly.InitializeAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested) return GeneralReturnCode.Canceled;
                if (ret < 0) return ret;
            }
#endif

            return await managedCanvas.InitializeAsync(cancellationToken);
        }

        // From IAsyncInitializable
        public void Terminate()
        {
            managedCanvas.Terminate();
        }

    }
}