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
        private IPAddress _ip;
        private int _port;
        public Bot(IPAddress ip, int port) {
            _ip = ip;
            _port = port;
        }
        public int get_port() {
            return _port;
        }
        public IPAddress get_ip()
        {
            return _ip;
        }

        public override string ToString()
        {
            return String.Format("IP: {0}, port: {1}\n", _ip.ToString(), _port);
        }

        public override bool Equals(object obj)
        {
            if ((obj.GetType() == typeof(Bot))) {
                Bot other = (Bot)obj;
                return other.get_port().Equals(_port) && other.get_ip().Equals(_ip);
            }
            return false;
        }
    }

}
