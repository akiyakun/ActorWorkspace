using UnityEditor;
using afl.Editor;
using ActorWorkspace.UnitySpine;

namespace ActorWorkspace.Editor.UnitySpine
{
    // See also: SpineSettings.cs
    [CustomEditor(typeof(UnitySpineSettings))]
    public class SpineSettingsEditor : AutoAddressingEditor
    {
        public override string Address => Environment.UnitySpineSettingsAddress;
    }
}
