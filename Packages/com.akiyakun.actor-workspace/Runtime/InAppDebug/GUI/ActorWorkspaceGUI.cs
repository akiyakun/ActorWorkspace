using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using afl;
using afl.Service.Screen;

namespace ActorWorkspace.InAppDebug
{
    public class ActorWorkspaceGUI : MonoBehaviour, IAsyncInitializable
    {
        [SerializeField] ManagedCanvas managedCanvas;

        // [SerializeField] ActorWorkspaceFormLogic actorWorkspaceFormLogic;
        // public ActorWorkspaceFormLogic Logic => actorWorkspaceFormLogic;

        // From IAsyncInitializable
        public bool IsInitialized { get; }

        // From IAsyncInitializable
        public async UniTask<int> InitializeAsync(CancellationToken cancellationToken = default)
        {
            Debug.Assert(managedCanvas != null);
            return await managedCanvas.InitializeAsync(cancellationToken);
        }

        // From IAsyncInitializable
        public void Terminate()
        {
            managedCanvas.Terminate();
        }

    }
}