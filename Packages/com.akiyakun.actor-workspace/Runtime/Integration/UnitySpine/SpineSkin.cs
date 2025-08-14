#nullable enable
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace ActorWorkspace.UnitySpine
{
    public class SpineSkin : IAWSkin
    {
        public string Name { get; private set; }

        Spine.Skin skin;
        public Spine.Skin Skin => skin;

#nullable disable
        private SpineSkin() { }
#nullable enable

        public SpineSkin(Spine.Skin skin)
        {
            this.skin = skin;
            Name = skin.Name;
        }
    }
}
#nullable restore