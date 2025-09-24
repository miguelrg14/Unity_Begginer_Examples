using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ESNEServer23
{
    internal class Pool
    {
        private int maxConnections;
        private List<Connection> connections = new List<Connection>();

        public Pool(int maxConnections)
        {
            this.maxConnections = maxConnections;
        }

        public bool AddConnection(TcpClient client)
        {
            if (connections.Count >= maxConnections)
                return false;

            Connection connection = new Connection(client);
            connection.SendToAllDelegate = SendToAll;
            connection.SendToAllButThisDelegate = SendToAllButThis;

            lock (connections)
            {
                connections.Add(connection);

                Console.WriteLine("New connection: " + connection);
                return true;
            }
        }

        public void RemoveConnection(Connection connection)
        {
            lock (connections)
            {
                connection.socket.Close();
                Console.WriteLine("Removing connection: " + connection);
                connections.Remove(connection);
            }
        }

        public void Process()
        {
            List<Connection> connectionsToClose = new List<Connection>();

            lock (connections)
            {
                foreach (Connection connection in connections)
                {
                    if (!connection.Process())
                        connectionsToClose.Add(connection);
                }
            }

            foreach (Connection connection in connectionsToClose)
                RemoveConnection(connection);
        }

        private void SendToAll(string str)
        {
            lock(connections)
            {
                foreach (Connection connection in connections)
                {
                    connection.Send(str);
                }
            }
        }

        private void SendToAllButThis(string str, Connection sender)
        {
            lock (connections)
            {
                foreach (Connection connection in connections)
                {
                    if (connection != sender)
                        connection.Send(str);
                }
            }
        }

    }
}
