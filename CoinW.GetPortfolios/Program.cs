using CoinW.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace CoinW.GetPortfolios;

public class Connector
{
    private readonly HttpClient httpClient = new HttpClient();

    private const string httpUrl = "https://api.coinw.com";
    private const string availableBalanceUrl = "/api/v1/private?command=returnBalances";
    private const string allBalancesUrl = "/api/v1/private?command=returnCompleteBalances";

    private string apiKey = Key.apiKey;
    private string secreteKey = Key.secreteKey;

    public Dictionary<string, string> AvailableBalance { get; set; }
    public Dictionary<string, AllBalances> AllBalances { get; set; }

    public Connector()
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("api_key", apiKey);

        SortedDictionary<string, string> sortedParameters = new SortedDictionary<string, string>(parameters);

        List<KeyValuePair<string, string>> sortedParametersList = sortedParameters.ToList();

        string signString = string.Empty;
        for (int i = 0; i < sortedParametersList.Count; i++)
        {
            signString += $"{sortedParametersList[i].Key}={sortedParametersList[i].Value}";
            signString += "&";
        }

        signString += $"secret_key={secreteKey}";

        string signature = CreateMD5(signString);

        parameters.Add("sign", signature);

        FormUrlEncodedContent content = new FormUrlEncodedContent(parameters);

        HttpResponseMessage responseMessage = httpClient.PostAsync(httpUrl + availableBalanceUrl, content).Result;
        if (responseMessage.IsSuccessStatusCode)
        {
            string responseBody = responseMessage.Content.ReadAsStringAsync().Result;
            AvailableBalance = JsonConvert.DeserializeObject<APIResponse<Dictionary<string, string>>>(responseBody).data;
        }
        else
        {
            throw new Exception($"Ошибка: {responseMessage.StatusCode}");
            //Console.WriteLine($"Ошибка: {responseMessage.StatusCode}");
        }

        responseMessage = httpClient.PostAsync(httpUrl + allBalancesUrl, content).Result;
        if (responseMessage.IsSuccessStatusCode)
        {
            string responseBody = responseMessage.Content.ReadAsStringAsync().Result;
            AllBalances = JsonConvert.DeserializeObject<APIResponse<Dictionary<string, AllBalances>>>(responseBody).data;
        }
        else
        {
            throw new Exception($"Ошибка: {responseMessage.StatusCode}");
            //Console.WriteLine($"Ошибка: {responseMessage.StatusCode}");
        }
    }

    private string CreateMD5(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}

public class Program
{
    static void Main(string[] args)
    {
        Connector connector = new Connector();

        Console.WriteLine("SUCCESS!!!");
    }
}
