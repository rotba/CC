using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CC
{
    class Bot
    {
        private EndPoint _end_point;
        private int _port;
        public Bot(EndPoint end_point, int port) {
            _end_point = end_point;
            _port = port;
        }
    }
}
