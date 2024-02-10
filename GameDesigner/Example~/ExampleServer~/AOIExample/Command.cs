using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOIExample
{
    public class Command : Net.Component.Command
    {
        public const byte EnterArea = 151;
        public const byte ExitArea = 152;
        public const byte RobotUpdate = 153;
    }
}
