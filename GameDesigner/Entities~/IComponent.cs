using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.Entities
{
    public interface IComponent
    {
        Entity Entity { get; set; }
    }
}
