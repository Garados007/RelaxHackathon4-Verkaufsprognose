using CsvHelper.Configuration.Attributes;

namespace Verkaufsprognose.Estimation;

public sealed class StockEstimation
{
    public StockEstimation(float expectedSellsPerDay, int startStock, DateTime now, List<PlanedOrder> orders)
    {
        ExpectedSellsPerDay = expectedSellsPerDay;
        StartStock = startStock;
        Now = now;
        Orders = orders;
    }

    public float ExpectedSellsPerDay { get; }

    public int StartStock { get; }

    public DateTime Now { get; }

    public List<PlanedOrder> Orders { get; }

    public ReadOnlyMemory<float> PrognosedStock { get; private set; }

    public void Fill()
    {
        var prognosed = GetEmptyPrognosedStock().Span;

        // fill start stock
        prognosed[0] = StartStock;

        // add orders
        foreach (var (arrival, amount) in Orders)
        {
            var index = (int)(arrival - Now).TotalDays;
            if (index < 0 || index >= prognosed.Length)
                continue;
            prognosed[index] += amount;
        }

        // spread stock among the days
        var previousStock = 0f;
        for (int i = 0; i < prognosed.Length; ++i)
        {
            previousStock -= ExpectedSellsPerDay;
            previousStock = Math.Max(0, previousStock + prognosed[i]);
            prognosed[i] = previousStock;
        }
    }

    public DateTime FirstDayWithoutStock(DateTime start)
    {
        var startIndex = Math.Max(0, (int)(start - Now).TotalDays);
        if (startIndex >= PrognosedStock.Length)
            return Now.AddDays(PrognosedStock.Length);
        var index = PrognosedStock.Span[startIndex..].IndexOf(0);
        if (index < 0)
            index = PrognosedStock.Length;
        else index += startIndex;
        return Now.AddDays(index);
    }

    private Memory<float> GetEmptyPrognosedStock()
    {
        // estimate the maximum prognosed stock size.
        var stockSum = StartStock;
        if (Orders.Count > 0)
            stockSum += Orders.Sum(x => x.Amount);
        var days = (int)Math.Ceiling(stockSum / ExpectedSellsPerDay);
        if (Orders.Count > 0)
            days += (int)Math.Ceiling((Orders.Max(x => x.Date) - Now).TotalDays);
        Memory<float> prognosedStock = new float[days];
        PrognosedStock = prognosedStock;
        return prognosedStock;
    }
}
