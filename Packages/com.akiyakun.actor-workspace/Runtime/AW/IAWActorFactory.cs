using UnityEngine;
using Cysharp.Threading.Tasks;

namespace ActorWorkspace
{
    public interface IAWActorFactory
    {
        UniTask<IAWActor> CreateAsync(int id);
    }
}
