namespace GlobalAzureSpain2023.Demo.ApimMonetization.Web.Models;

public class Monetization
{
    public string Id { get; set; }

    public string PricingModelType { get; set; }

    public MonetizationRecurrance Recurring { get; set; }

    public MonetizationPrices Prices { get; set; }
}

public class MonetizationRecurrance
{
    public string Interval { get; set; }

    public int IntervalCount { get; set; }
}

public class MonetizationPrices
{ 
    public Price Metered { get; set; }

    public UnitPrice Unit { get; set; }
}

public class Price
{
    public string Currency { get; set; }

    public decimal UnitAmount { get; set; }
}

public class UnitPrice : Price
{
    public int Quota { get; set; }
}