#nullable enable

namespace ActorWorkspace.UnitySpine
{
    public sealed class RawSpineActor
        : SpineActorBase<Tests.StubAWActorParam>
    {
        protected override Tests.StubAWActorParam CreateActorParam()
        {
            return new Tests.StubAWActorParam();
        }
    }
}
#nullable restore