using Net.Common;

namespace Net.Entities
{
    public class WorldSingleton : Singleton<WorldSingleton>
    {
        public World DefaultWorld { get; set; } = new World("DefaultWorld");
    }
}