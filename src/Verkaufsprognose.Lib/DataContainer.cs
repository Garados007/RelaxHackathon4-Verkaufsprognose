namespace Verkaufsprognose;

public sealed class DataContainer
{
    public DataContainer(Dictionary<int, Product> products, Dictionary<int, int> storage)
    {
        Inventory = new();
        foreach (var product in products.Values)
        {
            var info = new Info(product);
            Inventory.Add(info.Product.Id, info);
            if (storage.TryGetValue(info.Product.Id, out int amount))
                info.Storage = amount;
        }
    }

    public Dictionary<int, Info> Inventory { get; }

    public static DataContainer Create(IEnumerable<Product> products, IEnumerable<Storage> storage)
    {
        return new(
            products.ToDictionary(x => x.Id),
            storage.ToDictionary(x => x.ProductId, x => x.Count)
        );
    }

    public void AddSells(IEnumerable<Sales> sales)
    {
        foreach (var sale in sales)
        {
            if (!Inventory.TryGetValue(sale.ProductId, out var info))
                continue;
            info.Storage -= sale.ProductCount;
            info.Sold += sale.ProductCount;
        }
    }

    public IEnumerable<Order> GetOrders(DateTime now)
    {
        foreach (var item in Inventory.Values)
        {
            item.FinializeOrders(now);
            // check if this item was sold at all
            if (item.Sold == 0)
                continue;
            // get the remaining days this item will be on stock
            var remainingDays = item.RemainingDays(now);
            // check if the rebuy interval is large enough for the current stock
            if (remainingDays > item.Product.ShippingDuration + 1)
                continue;
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
                continue;
            // calculate the maximum stock that is needed for the next optimal interval
            var expected = item.BestDuration * item.ExpectedSellsPerDay(now);
            // create order
            var order = new Order
            {
                ProductId = item.Product.Id,
                OrderAmount = (int)Math.Ceiling(expected),
            };
            item.Orders.Add((arrival, order.OrderAmount));
            yield return order;
        }
    }
}
