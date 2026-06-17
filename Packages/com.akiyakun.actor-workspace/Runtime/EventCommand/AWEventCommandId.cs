#nullable enable
using afl.EventDirector;

namespace ActorWorkspace.EventCommand
{
    public enum AWEventCommandId
    {
        _Begin = CoreEventCommandId._AWEventCommandId,

        AddToActorPool,
        SpawnActor,
        SpawnActorAsync,
        DespawnActor,

        _ImmediateBegin,
        _ImmediateEnd,

        _End,
    }
}
#nullable restore