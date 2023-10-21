using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Entities
{
    public class Entity
    {
        internal List<int> componentIds = new List<int>();

        internal EntityWorld World { get; set; }

        private Entity() { }

        internal Entity(EntityWorld world) 
        {
            this.World = world;
        }

        public ref T GetComponent<T>() where T : IComponent, new() => ref World.GetEntityComponent<T>(this);

        public ref IComponent AddComponent(Type component)
        {
            return ref World.AddEntityComponent(this, component);
        }
    }
}
