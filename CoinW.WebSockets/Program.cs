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
                //42["subscribe",{ "channel":"spot/level2:BTC-USDT","subject":"spot/level2","data":"{\"startSeq\":3818617089,\"endSeq\":3818617091,\"asks\":[[\"61022.17\",\"0.2432\",\"3818617091\"]],\"bids\":[[\"61021.12\",\"1.2939\",\"3818617089\"],[\"61019.94\",\"0.2966\",\"3818617090\"]]}"}]
                // Извлечение части строки с JSON-объектом
                int startIndex = message.IndexOf('{');
                int endIndex = message.LastIndexOf('}');
                string json = message.Substring(startIndex, endIndex - startIndex + 1);

                // Декодирование вложенного JSON
                var subscriptionData = JsonConvert.DeserializeObject<SubscriptionData>(json);
                Console.WriteLine(subscriptionData.Channel); // spot/level2:BTC-USDT
                Console.WriteLine(subscriptionData.Subject); // spot/level2
                Console.WriteLine(subscriptionData.Data); // {"startSeq":3818717270,"endSeq":3818717270,"asks":[],"bids":[["60947.34","0.0000","3818717270"]]}
                Console.WriteLine();

                var marketData = JsonConvert.DeserializeObject<MarketData>(subscriptionData.Data);
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
                //string subscribeMessage = $"42[\"subscribe\",{{\"args\":\"spot/market-api-ticker:{symbol}\"}}]";
                string subscribeMessage = $"42[\"subscribe\",{{\"args\":\"spot/level2:{symbol}\"}}]";
                //string subscribeMessage = $"42[\"subscribe\",{{\"args\":\"spot/match:{symbol}\"}}]";
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
