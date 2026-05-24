#nullable enable
using UnityEngine;
using afl.EventDirector;
using ActorWorkspace;

namespace ActorWorkspace.EventCommand
{
    public class AWEventCommandFactory : EventCommandFactory
    {
        IAWActorManager actorManager;
        public IAWActorManager ActorManager => actorManager;

#nullable disable
        private AWEventCommandFactory() {}
#nullable enable

        public AWEventCommandFactory(IAWActorManager actorManager) : base()
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
                (int)AWEventCommandId.SpawnActorAsync => new ECSpawnActorAsync(this),
                _ => throw new System.ArgumentOutOfRangeException(
                    nameof(id), $"AWEventCommandFactory: Invalid AWEventCommandId={id}, Enum={((AWEventCommandId)id).ToString()}"),
            };
        }

        protected override void PrecreateImmediateCommands()
        {
            AddImmediateCommand(new ECSpawnActor(this));
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