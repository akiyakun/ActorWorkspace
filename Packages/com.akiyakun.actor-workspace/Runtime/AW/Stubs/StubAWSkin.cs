#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace.Tests
{
    public class StubAWSkin : IAWSkin
    {
        public string Name => nameof(StubAWSkin);
    }
}
#nullable restore