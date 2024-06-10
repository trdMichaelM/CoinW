using CoinW.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace CoinW.GetSecurities;

public class Connector
{
    private readonly HttpClient httpClient = new HttpClient();

    private const string httpUrl = "https://api.coinw.com";
    private const string tradingPairUrl = "/api/v1/public?command=returnSymbol";
    private const string coinInformationUrl = "/api/v1/public?command=returnCurrencies";
    private const string tradingPairInformationUrl = "/api/v1/public?command=returnTicker";

    public List<TradingPair> TradingPairs { get; private set; }
    //public List<Coin> Coins { get; private set; }
    public Dictionary<string, Coin> Coins { get; private set; }
    //public List<TradingPairInformation> TradingPairsInformation { get; private set; }
    public Dictionary<string, TradingPairInformation> TradingPairsInformation { get; private set; }

    public Connector()
    {
        HttpResponseMessage httpResponseMessage = httpClient.GetAsync(httpUrl + tradingPairUrl).Result;
        string response = httpResponseMessage.Content.ReadAsStringAsync().Result;
        //MainResponse = JsonConvert.DeserializeAnonymousType<MainResponse>(Json, new MainResponse());
        TradingPairs = JsonConvert.DeserializeObject<MainResponse<List<TradingPair>>>(response).data;

        httpResponseMessage = httpClient.GetAsync(httpUrl + coinInformationUrl).Result;
        response = httpResponseMessage.Content.ReadAsStringAsync().Result;
        Coins = JsonConvert.DeserializeObject<MainResponse<Dictionary<string, Coin>>>(response).data;

        httpResponseMessage = httpClient.GetAsync(httpUrl + tradingPairInformationUrl).Result;
        response = httpResponseMessage.Content.ReadAsStringAsync().Result;
        TradingPairsInformation = JsonConvert.DeserializeObject<MainResponse<Dictionary<string, TradingPairInformation>>>(response).data;
    }
}

public class Program
{
    static void Main(string[] args)
    {
        Connector CoinW = new Connector();

        Console.WriteLine(CoinW.TradingPairs);
        Console.WriteLine(CoinW.Coins);
        Console.WriteLine(CoinW.TradingPairsInformation);

        Console.WriteLine("SUCCESS!!!");
    }
}
