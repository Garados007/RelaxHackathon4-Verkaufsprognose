namespace Verkaufsprognose;

public class Info
{

    public Product Product { get; }

    public float BestDuration { get; }

    public int Storage { get; set; }

    public int Sold { get; set; }

    public List<(DateTime arrival, int amount)> Orders { get; } = new();

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
        var deleteList = new List<(DateTime arrival, int amount)>();
        foreach (var (arrival, amount) in Orders)
        {
            if (arrival > now)
                continue;
            deleteList.Add((arrival, amount));
            Storage += amount;
        }
        foreach (var entry in deleteList)
            Orders.Remove(entry);
    }

    public Info(Product product)
    {
        Product = product;
        BestDuration = (product.Price - 0.5f * product.StorageCost) / product.StorageCost;
    }
}
