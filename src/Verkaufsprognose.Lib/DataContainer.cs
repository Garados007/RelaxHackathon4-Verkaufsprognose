namespace Verkaufsprognose;

public sealed class DataContainer
{
    public DataContainer(Dictionary<int, Product> products, Dictionary<int, int> storage,
        List<Sales> predictableSales, Dictionary<int, (float, float)>? factors = null,
        float knownFactor = 0.5f, float estimateFactor = 0.6f
    )
    {
        Inventory = new();
        var estimation = new Estimation.PredictableRestockEstimation();
        foreach (var product in products.Values)
        {
            var lowCost = predictableSales.Where(x => x.ProductId == product.Id).Min(x => x.SalesPrice);
            var highCost = predictableSales.Where(x => x.ProductId == product.Id).Max(x => x.SalesPrice);

            Info info;
            if (factors is null)
                info = new Info(product, (lowCost + highCost) * 0.5f, knownFactor, estimateFactor);
            else
            {
                (knownFactor, estimateFactor) = factors[product.Id];
                info = new Info(product, (lowCost + highCost) * 0.5f, knownFactor, estimateFactor);
            }

            Inventory.Add(info.Product.Id, info);
            if (storage.TryGetValue(info.Product.Id, out int amount))
                info.Storage = amount;

            foreach (var sale in predictableSales)
                if (sale.ProductId == product.Id)
                    info.PredictableSales.Add(sale.Date, sale.ProductCount);

            estimation.GetOrders(info);
        }
        if (predictableSales.Count > 0)
        {
            LatestPrediction = predictableSales.Max(x => x.Date);
            Armageddon = new DateTime(LatestPrediction.Year, LatestPrediction.Month + 2, 1).AddDays(-1);
        }
    }

    public Dictionary<int, Info> Inventory { get; }

    public DateTime LatestPrediction { get; }

    /// <summary>
    /// No sales after this point. The storage has to be empty.
    /// </summary>
    public DateTime Armageddon { get; }

    public void AddSells(IEnumerable<Sales> sales)
    {
        foreach (var sale in sales)
        {
            if (!Inventory.TryGetValue(sale.ProductId, out var info))
                continue;
            info.Storage -= sale.ProductCount;
            info.Sold += sale.ProductCount;
            info.FinializeOrders(sale.Date);
            info.Storage = Math.Max(0, info.Storage);
        }
    }

    public IEnumerable<Order> GetOrders(DateTime now)
    {
        now = now.Date;
        foreach (var item in Inventory.Values)
        {
            item.FinializeOrders(now);

            foreach (var order in GetPredictedOrder(now, item))
                yield return order;
            if (now > LatestPrediction.AddDays(-item.Product.ShippingDuration))
            {
                var order = GetUnknownOrder(now, item);
                if (order != null)
                    yield return order;
            }
        }
    }

    private static IEnumerable<Order> GetPredictedOrder(DateTime now, Info item)
    {
        foreach (var order in item.PredictedOrders)
        {
            if (order.Date != now)
                continue;
            var arrival = now.AddDays(item.Product.ShippingDuration);
            item.Orders.Add(new(arrival, order.Amount));
            yield return new Order
            {
                ProductId = item.Product.Id,
                OrderAmount = order.Amount,
            };
        }
    }

    private Order? GetUnknownOrder(DateTime now, Info item)
    {
        // check if this item was sold at all
        if (item.Sold == 0)
            return null;
        // check if there is not enough time
        if (now.AddDays(item.Product.ShippingDuration) > Armageddon)
            return null;
        // get the remaining days this item will be on stock
        var remainingDays = item.RemainingDays(now);
        // check if the rebuy interval is large enough for the current stock
        if (remainingDays > item.Product.ShippingDuration + 1)
            return null;;
        // get the first time we are out of stock
        var estimation = new Estimation.StockEstimation(
            item.ExpectedSellsPerDay(now),
            item.Storage,
            now.AddDays(1),
            item.Orders
        );
        estimation.Fill();
        var arrival = now.AddDays(item.Product.ShippingDuration);
        var outOfStock = estimation.FirstDayWithoutStock(arrival);
        // check if restocking needs to be done right now
        if ((outOfStock - now).TotalDays > item.Product.ShippingDuration + 1)
            return null;;
        // calculate the maximum stock that is needed for the next optimal interval
        var duration = Math.Min(item.BestPredictionDuration, (Armageddon - now).TotalDays);
        var expected = duration * item.ExpectedSellsPerDay(now);
        // create order
        var order = new Order
        {
            ProductId = item.Product.Id,
            OrderAmount = (int)Math.Ceiling(expected),
        };
        item.Orders.Add(new(arrival, order.OrderAmount));
        return order;
    }
}
