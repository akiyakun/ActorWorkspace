#nullable enable
namespace ActorWorkspace
{
    public struct ActorCreateParam
    {
        public int Id;
        public int Category;
        public object? UserData;

        public ActorCreateParam(int id, int category = 0, object? userData = null)
        {
            Id = id;
            Category = category;
            UserData = userData;
        }
    }
}
#nullable restore