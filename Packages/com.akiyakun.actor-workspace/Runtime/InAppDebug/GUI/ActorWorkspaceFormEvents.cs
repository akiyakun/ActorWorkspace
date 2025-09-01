
namespace ActorWorkspace.InAppDebug
{
    public static class ActorWorkspaceFormEvents
    {
        public static readonly (string, IAWActor) ResetUI = ("ResetUI", default);

        public static readonly string PlayList_Clear = "PlayList_Clear";
        public static readonly (string, IAWAnimation) PlayList_Add = ("PlayList_Add", default);
    }
}
