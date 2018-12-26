using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CC
{
    class Victim
    {
        private IPAddress _ip;
        private int _port;
        private string _password;
        public static Victim createVictim(IPAddress ip, string port, string password) {
            if (password.Length != 6)
            {
                throw new ArgumentException();
            }
            else {
                return new Victim(ip, port, password);
            }
        }
        private Victim(IPAddress ip, string port, string password) {
            _ip = ip;
            Int32.TryParse(port, out _port);
            _password= password;
        }
        public int get_port() {
            return _port;
        }
        public string get_password()
        {
            return _password;
        }
        public IPAddress get_ip_address()
        {
            return _ip;
        }

        public override string ToString()
        {
            return String.Format("Victim.ip = {0}\nVictim.port = {1}\nVictim.password = {2}", _ip, _port, _password);
        }
    }
}
