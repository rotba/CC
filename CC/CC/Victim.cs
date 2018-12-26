using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC
{
    class Victim
    {
        private string _ip;
        private string _port;
        private string _password;
        public Victim(string ip, string port, string password) {
            _ip = ip;
            _port = port;
            _password= password;
        }

        public override string ToString()
        {
            return String.Format("Victim.ip = {0}\nVictim.port = {1}\nVictim.password = {2}", _ip, _port, _password);
        }
    }
}
