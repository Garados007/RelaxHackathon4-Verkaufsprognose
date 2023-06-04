namespace Verkaufsprognose;

public class Statistics
{
    private readonly Info info;
    private readonly DateTime now;

    public Statistics(Info info, DateTime now)
    {
        this.info = info;
        this.now = now;
    }

    public int Id => info.Product.Id;

    public float ExpectedSellsPerDay => info.ExpectedSellsPerDay(now);

    public float RemainingDays => info.RemainingDays(now);
}
