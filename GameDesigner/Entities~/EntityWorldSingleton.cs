using Net.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Entities
{
    public class EntityWorldSingleton : Singleton<EntityWorldSingleton>
    {
        public EntityWorld DefaultWorld { get; set; } = new EntityWorld("DefaultWorld");
    }
}
