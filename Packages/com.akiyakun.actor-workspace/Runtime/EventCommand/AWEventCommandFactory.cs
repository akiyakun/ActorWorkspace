#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using afl.EventDirector;

namespace ActorWorkspace.EventCommand
{
    public class AWEventCommandFactory : EventCommandFactory
    {
        IAWActorManager actorManager;
        // public IAWActorManager ActorManager => actorManager;

        public AWEventCommandFactory(IAWActorManager actorManager)
        {
            this.actorManager = actorManager;
        }

        public override bool IsValid(int id)
        {
            return (int)AWEventCommandId._Begin <= id && id < (int)AWEventCommandId._End;
        }

        protected override IEventCommand? CreateCommand(int id)
        {
            return id switch
            {
                (int)AWEventCommandId.AddToActorPool => new ECAddToActorPool(actorManager),
                (int)AWEventCommandId.SpawnActorAsync => new ECSpawnActorAsync(actorManager),
                _ => throw new System.ArgumentOutOfRangeException(
                    nameof(id), $"AWEventCommandFactory: Invalid AWEventCommandId={id}, Enum={((AWEventCommandId)id).ToString()}"),
            };
        }

        protected override async UniTask<int> PrecreateImmediateCommandsAsync(CancellationToken cancellationToken)
        {
            AddImmediateCommand(new ECSpawnActor(actorManager));
            return await base.PrecreateImmediateCommandsAsync(cancellationToken);
        }

        public override int ParseCommandId(string str)
        {
            if (System.Enum.TryParse<AWEventCommandId>(str, out var commandId) == true)
            {
                // return System.Convert.ToInt32(commandId);
                return (int)commandId;
            }

            return (int)CoreEventCommandId.Undefined;
        }
    }
}
#nullable restore