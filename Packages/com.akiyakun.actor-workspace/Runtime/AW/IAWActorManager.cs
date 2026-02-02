#nullable enable
namespace ActorWorkspace
{
    public interface IAWActorManager : System.IDisposable
    {
        public void DoUpdate(float deltaTime);
		public void DoLateUpdate(float deltaTime);
        public void DoFixedUpdate();

        public bool Add(IAWActor actor);
        public bool Remove(IAWActor actor);
    }
}
#nullable restore