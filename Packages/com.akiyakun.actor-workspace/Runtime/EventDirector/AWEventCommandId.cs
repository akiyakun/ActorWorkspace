#nullable enable
using afl.EventDirector;

namespace ActorWorkspace.EventDirector
{
    public enum AWEventCommandId
    {
        _Begin = CoreEventCommandId._AWEventCommandId,

        AddToActorPool,
        // SpawnActor,

        _ImmediateBegin,
        _ImmediateEnd,

        _End,
    }
}
#nullable restore