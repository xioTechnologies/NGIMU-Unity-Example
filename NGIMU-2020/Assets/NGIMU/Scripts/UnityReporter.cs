using System;
using NgimuApi;
using Rug.Osc;

namespace NGIMU.Scripts
{
    internal class UnityReporter : IReporter
    {
        public void Clear()
        {
            Console.Clear();
        }

        public void OnCompleted(object sender, EventArgs args)
        {
            Print("Completed");
        }

        public void OnConnected(object sender, System.EventArgs e)
        {
            Print("Connection Connected");
        }

        public void OnDisconnected(object sender, System.EventArgs e)
        {
            Print("Connection Disconnected");
        }

        public void OnError(object sender, MessageEventArgs args)
        {
            Print("Error: " + args.Message);
        }

        public void OnException(object sender, ExceptionEventArgs args)
        {
            Print("Exception: " + args.Message);
        }

        public void OnInfo(object sender, MessageEventArgs args)
        {
            Print(args.Message);
        }

        public void OnMessage(Connection source, MessageDirection direction, OscMessage message)
        {
            Print((direction == MessageDirection.Receive ? "RX " : "TX ") + message.ToString());
        }

        public void OnUnknownAddress(object sender, Rug.Osc.UnknownAddressEventArgs e)
        {
            Print("Unknown Address: " + e.Packet.ToString());
        }

        public void OnUpdated(object sender, EventArgs args)
        {
        }

        private void Print(string str)
        {
            Console.Print(str);
        }
    }
}