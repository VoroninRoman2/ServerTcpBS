using System.Net.Sockets;

namespace ServerTcpBS
{
    class ConnectedClients
    {

        public TcpClient Player1 { get; set; }
        public TcpClient Player2 { get; set; }
        public string NickName1 { get; set; }
        public string NickName2 { get; set; }
        public bool Ready1 { get; set; }
        public bool Ready2 { get; set; }
        public string FirstMove { get; set; }

        public ConnectedClients(TcpClient client1,string nick1)
        {
            Player1 = client1;
            NickName1 = nick1;
            Ready1 = false;
            Ready2 = false;
            FirstMove = "0";
        }
        public ConnectedClients(TcpClient client1, TcpClient client2, string nick1, string nick2)
        {
            Player1 = client1;
            Player2 = client2;
            NickName1 = nick1;
            NickName2 = nick2;
            Ready1 = false;
            Ready2 = false;
            FirstMove = "0";
        }
    }
}
