using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC
{
    class Program
    {
        static void Main(string[] args)
        {
            int curr_port = 2019;
            string curr_ip = "192.168.0.26";
            Console.WriteLine("Command and control server {0} active", "Hadas&roteM");
            CCServer ccs = new CCServer(curr_port, curr_ip);
            ccs.control();
        }
    }
}
