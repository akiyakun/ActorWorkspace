using System;
using afl.UI;

namespace ActorWorkspace.InAppDebug
{
    public class ActorWorkspaceFormBinding : UIFormBinding
    {
        public static class Events
        {
            public static readonly (string, IAWActor) ResetUI = ("ResetUI", default);

            public static readonly string PlayList_Clear = "PlayList_Clear";
            public static readonly (string, IAWAnimation) PlayList_Add = ("PlayList_Add", default);
        }
    }
}
