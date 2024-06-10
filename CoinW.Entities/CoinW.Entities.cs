namespace CoinW.Entities;

public enum ErrorCode
{
    Success = 200,
    OperationFailed = 500,
    NetworkError = 10001,
    APINotFound = 10002,
    ParameterError = 10003,
    NoTradingPermission = 10004,
    NoWithdrawPermission = 10005,
    ApiKeyError = 10006,
    SignatureError = 10007
}

public class MainResponse<T>
{
    public string code;
    public T data;
    public string failed;
    public string msg;
    public string success;
}

public class TradingPairInformation
{
    public string id;
    public string last;
    public string lowestAsk;
    public string highestBid;
    public string percentChange;
    public string isFrozen;
    public string high24hr;
    public string low24hr;
    public string baseVolume;
}

public class Coin
{
    public string chain;
    public string maxQty;
    public string minQty;
    public string recharge;
    public string symbol;
    public string symbolId;
    public string txFee;
    public string withDraw;
}

public class TradingPair
{
    public string currencyPair;
    public string currencyBase;
    public string currencyQuote;
    public string maxBuyCount;
    public string minBuyCount;
    public string pricePrecision;
    public string countPrecision;
    public string minBuyAmount;
    public string maxBuyAmount;
    public string minBuyPrice;
    public string maxBuyPrice;
    public string state;
}
