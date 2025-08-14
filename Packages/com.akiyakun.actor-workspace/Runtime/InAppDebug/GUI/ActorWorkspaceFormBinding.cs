using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Threading;
using Cysharp.Threading.Tasks;
using afl;
using afl.MasterData;
using afl.UI.v1;
using afl.UI;
using TMPro;
using ActorWorkspace.MasterData;

namespace ActorWorkspace.InAppDebug
{
    public class ActorWorkspaceFormBinding : UIFormBinding
    {
        public static class Event
        {
            public const string ResetUI = "ResetUI";

            public const string PlayList_Clear = "PlayList_Clear";
            public const string PlayList_Add = "PlayList_Add";
        }
    }
}
