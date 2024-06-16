using System;
using System.Net.Http;
using System.Threading;

using CoinW.Entities;

using Newtonsoft.Json;
using WebSocket4Net;

namespace CoinW.WebSockets
{
    public class Connector
    {
        private readonly HttpClient httpClient = new HttpClient();

        private const string HOST = "https://www.coinw.com";
        private const string PUBLIC_TOKEN_URL = $"{HOST}/pusher/public-token";

        private WebSocket socket;

        public WebSocketInformation webSocketInformation { get; set; }

        public Connector()
        {
            HttpResponseMessage response = httpClient.GetAsync(PUBLIC_TOKEN_URL).Result;
            if (response.IsSuccessStatusCode)
            {
                string responseContent = response.Content.ReadAsStringAsync().Result;
                webSocketInformation = JsonConvert.DeserializeObject<APIResponse<WebSocketInformation>>(responseContent).data;
            }
            else
            {
                throw new Exception("Failed to get token");
            }

            string endpoint = webSocketInformation.endpoint;
            string token = webSocketInformation.token;

            string url = $"{endpoint}/socket.io/?token={token}&EIO=3&transport=websocket";
            socket = new WebSocket(url);

            socket.Opened += Socket_Opened;
            socket.MessageReceived += Socket_MessageReceived;
            socket.Error += Socket_Error;
            socket.Closed += Socket_Closed;
        }

        public void Connect()
        {
            socket.Open();
        }

        private void Socket_Closed(object? sender, EventArgs e)
        {
            Console.WriteLine("Connection closed.");
        }

        private void Socket_Error(object? sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine($"Error: {e.Exception.Message}");
        }

        private void Socket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            string message = e.Message;
            if (message.StartsWith("3")) // Pong message
            {
                Console.WriteLine("Pong received");
            }
            else if (message.StartsWith("42")) // Event message
            {
                Console.WriteLine($"Event received: {message}");
            }
            else
            {
                Console.WriteLine($"Message received: {message}");
            }
        }

        private void Socket_Opened(object? sender, EventArgs e)
        {
            Console.WriteLine("Connected!");

            socket.Send("2probe");
            socket.Send("5");
        }

        public void Subscribe(string symbol)
        {
            if (socket.State == WebSocketState.Open)
            {
                string subscribeMessage = $"42[\"subscribe\",{{\"args\":\"spot/market-api-ticker:{symbol}\"}}]";
                socket.Send(subscribeMessage);
                Console.WriteLine($"Subscribed to: {symbol}");
            }
            else
            {
                Console.WriteLine("WebSocket is not connected.");
            }
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            Connector connector = new Connector();
            connector.Connect();

            Thread.Sleep(2000);

            connector.Subscribe("BTC-USDT");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
