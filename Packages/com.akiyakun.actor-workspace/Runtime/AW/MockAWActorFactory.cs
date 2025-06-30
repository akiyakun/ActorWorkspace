using UnityEngine;
using Cysharp.Threading.Tasks;

namespace ActorWorkspace
{
    public class MockAWActorFactory : IAWActorFactory
    {
        public UniTask<IAWActor> CreateAsync(int id)
        {
            return UniTask.FromResult<IAWActor>(new MockAWActor());
        }
    }
}
