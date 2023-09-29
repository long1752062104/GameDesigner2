using Net.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS
{
    public class GameComponent : Component
    {
        public TransformEntity transform = new TransformEntity();
    }
}
