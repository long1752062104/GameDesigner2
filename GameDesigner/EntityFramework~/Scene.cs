using Net.System;

namespace Net.EntityFramework
{
    public class Scene
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public FastList<GameObject> Roots { get; set; }

        public Scene() 
        {
            Roots = new FastList<GameObject>();
        }
    }
}
