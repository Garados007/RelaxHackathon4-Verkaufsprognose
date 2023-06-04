namespace Verkaufsprognose.Estimation;

public sealed class PredictableRestockEstimation
{
    public void GetOrders(Info info)
    {
        var last = DateUtils.DaysFromEpoch(info.PredictableSales.Keys.Max());
        Span<int> orders = new int[last + 1];
        foreach (var (date, sale) in info.PredictableSales)
            orders[DateUtils.DaysFromEpoch(date)] = sale;

        var list = new List<PlanedOrder>();

        var start = DateUtils.DaysFromEpoch(new DateTime(2023, 03, 01));
        var window = (int)info.BestDuration;
        for (; start < orders.Length; ++start)
        {
            // skip if they are no orders at the start date
            if (orders[start] == 0)
                continue;
            // get a sum of the following dates
            var sum = 0;
            var end = Math.Min(orders.Length - 1, start + window);
            for (int i = start; i <= end; i++)
                sum += orders[i];

            info.PredictedOrders.Add(new(DateUtils.Epoch.AddDays(start - info.Product.ShippingDuration), sum));
            start = end;
        }
    }
}
