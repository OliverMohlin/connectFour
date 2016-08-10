using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFour_Server
{
    public class Message
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public Command CommandType { get; set; }
        public string MessageData { get; set; }
        public string Sender { get; set; }
    }
}
