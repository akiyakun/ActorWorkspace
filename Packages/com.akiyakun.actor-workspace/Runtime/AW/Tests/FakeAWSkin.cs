using System.Collections.Generic;
using UnityEngine;

namespace ActorWorkspace.Tests
{
    public class FakeAWSkin : IAWSkin
    {
        public string Name => nameof(FakeAWSkin);
    }
}
