using Verkaufsprognose;

namespace Trainer;

public class TrainerSlot
{
    public TrainerSlot(TrainingSetup setup, float factorA, float factorB)
    {
        Setup = setup;
        FactorA = factorA;
        FactorB = factorB;
        Data = new DataContainer(Setup.Products, Setup.Storage, Setup.PartialSales, null, factorA, factorB);
    }

    public TrainingSetup Setup { get; }

    public DataContainer Data { get; }

    public float FactorA { get; }

    public float FactorB { get; }

    public float Score { get; private set; }

    public async Task<float> RunAsync()
    {
        return Score = await Task.Run(() => Run());
    }

    private float Run()
    {
        float totalGrossProfit = 0;
        float totalStorageCost = 0;
        float totalProductCost = 0;
        float totalShippingCost = 0;

        var storage = new int[100];
        foreach (var (id, store) in Setup.Storage)
            storage[id] = store;

        var currentDate = Setup.FullSales[0].Date;
        var endDate = Setup.FullSales[^1].Date;
        var remainingOrder = new List<(Order order, DateTime arrival)>();

        while (currentDate <= endDate)
        {
            float dayGrossProfit = 0;
            float dayStorageCost = 0;
            float dayProductCost = 0;
            float dayShippingCost = 0;

            // get data for the current day
            var daySales = Setup.FullSales.Where(x => x.Date == currentDate).ToList();
            Data.AddSells(daySales);
            var dayOrders = Data.GetOrders(currentDate);

            // update storage cost
            foreach (var info in Data.Inventory.Values)
            {
                dayStorageCost += info.Product.StorageCost * storage[info.Product.Id];
            }

            // update order/arrivals
            foreach (var order in dayOrders)
            {
                var info = Data.Inventory[order.ProductId];
                remainingOrder.Add((order, currentDate.AddDays(info.Product.ShippingDuration)));
                dayProductCost += info.Product.Price * order.OrderAmount;
                dayShippingCost += info.Product.ShippingCost;
            }
            foreach (var order in remainingOrder.Where(x => x.arrival <= currentDate))
            {
                storage[order.order.ProductId] += order.order.OrderAmount;
            }
            remainingOrder.RemoveAll(x => x.arrival <= currentDate);

            // update sales
            foreach (var sale in daySales)
            {
                var realCount = Math.Min(storage[sale.ProductId], sale.ProductCount);
                storage[sale.ProductId] -= realCount;
                dayGrossProfit += sale.SalesPrice * realCount;
            }

            currentDate = currentDate.AddDays(1);
            totalGrossProfit += dayGrossProfit;
            totalProductCost += dayProductCost;
            totalShippingCost += dayShippingCost;
            totalStorageCost += dayStorageCost;
        }

        return totalGrossProfit - totalProductCost - totalShippingCost - totalStorageCost;
    }
}
