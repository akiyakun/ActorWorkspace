#nullable enable
using UnityEngine;

namespace ActorWorkspace
{
    public struct ActorCreateParam
    {
        public int Id;
        public int Category;
        public Vector3 Position;
        public object? UserData;

        public ActorCreateParam(int id, int category, Vector3 position, object? userData = null)
        {
            Id = id;
            Category = category;
            Position = position;
            UserData = userData;
        }
    }
}
#nullable restore