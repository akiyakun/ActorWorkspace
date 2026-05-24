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
                _ => throw new System.ArgumentOutOfRangeException(
                    nameof(id), $"Invalid AWEventCommandId={id}, Enum={((AWEventCommandId)id).ToString()}"),
            };
        }

        protected override void PrecreateImmediateCommands()
        {
            // AddImmediateCommand(new ECLog());
            // AddImmediateCommand(new ECEmpty());
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