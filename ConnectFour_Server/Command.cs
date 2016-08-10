using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFour_Server
{
    public enum Command
    {
        Move = 1,
        SetUsername = 2,
        Message = 3,
        ChangeUserName = 4,
        Disconnect = 10
    }
}
