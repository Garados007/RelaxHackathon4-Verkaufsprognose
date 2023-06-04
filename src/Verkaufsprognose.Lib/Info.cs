namespace Verkaufsprognose;

public class Info
{
    public Product Product { get; }

    public float SellPrice { get; }

    public float BestDuration { get; }

    public float BestPredictionDuration { get; }

    public int Storage { get; set; }

    public int Sold { get; set; }

    public List<PlanedOrder> PredictedOrders { get; } = new();

    public List<PlanedOrder> Orders { get; } = new();

    public Dictionary<DateTime, int> PredictableSales { get; } = new();

    public float ExpectedSellsPerDay(DateTime now)
    {
        var days = now.DaysFromEpoch() + 1;
        return (float)Sold / days;
    }

    public int RemainingDays(DateTime now)
    {
        var expected = ExpectedSellsPerDay(now);
        return (int)Math.Floor(Storage / expected);
    }

    public void FinializeOrders(DateTime now)
    {
        var deleteList = new List<PlanedOrder>();
        foreach (var entry in Orders)
        {
            if (entry.Date > now)
                continue;
            deleteList.Add(entry);
            Storage += entry.Amount;
        }
        foreach (var entry in deleteList)
            Orders.Remove(entry);
    }

    public Info(Product product, float sellPrice, float knownFactor = 0.5f, float estimateFactor = 0.6f)
    {
        // Console.WriteLine($"knownFactor={knownFactor}, estimateFactor={estimateFactor}");
        Product = product;
        SellPrice = sellPrice;
        // BestDuration = (sellPrice - product.Price - 0.5f * product.StorageCost) / product.StorageCost;
        BestDuration = product.ShippingDuration * knownFactor;
        BestPredictionDuration = product.ShippingDuration * estimateFactor;
    }
}
