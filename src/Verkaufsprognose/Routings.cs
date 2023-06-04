using MaxLib.WebServer;
using MaxLib.WebServer.Builder;

namespace Verkaufsprognose;

#pragma warning disable CA1822 // Der Member "..." greift nicht auf Instanzdaten zu und kann als "static" markiert werden.

public sealed class Routings : Service
{
    [Path("/hello")]
    public string HelloWorld()
    {
        return "Hello World";
    }

    [Path("/sales")]
    [Method(HttpProtocolMethod.Post)]
    [return: JsonDataConverter]
    public List<Sales> PostSales([JsonConverter, TextPost] List<Sales> sales)
    {
        Program.Data.AddSells(sales);
        return sales;
    }

    [Path("/orders")]
    [Method(HttpProtocolMethod.Get)]
    [return: JsonDataConverter]
    public IEnumerable<Order> GetOrders([Get, Converter(typeof(DateConverter))] DateTime date)
    {
        return Program.Data.GetOrders(date);
    }

    [Path("/products")]
    [Method(HttpProtocolMethod.Get)]
    [return: JsonDataConverter]
    public Dictionary<int, Product> GetProducts()
    {
        return Program.Data.Inventory.ToDictionary(x => x.Key, x => x.Value.Product);
    }

    [Path("/inventory")]
    [Method(HttpProtocolMethod.Get)]
    [return: JsonDataConverter]
    public Dictionary<int, Info> GetProductInventory()
    {
        return Program.Data.Inventory;
    }

    [Path("/stats")]
    [Method(HttpProtocolMethod.Get)]
    [return: JsonDataConverter]
    public Dictionary<int, Statistics> GetStats([Get, Converter(typeof(DateConverter))] DateTime date)
    {
        return Program.Data.Inventory.ToDictionary(
            x => x.Key,
            x => new Statistics(x.Value, date)
        );
    }

    [Path("/storage")]
    [Method(HttpProtocolMethod.Get)]
    [return: JsonDataConverter]
    public Dictionary<int, int> GetStorage()
    {
        return Program.Data.Inventory.ToDictionary(x => x.Key, x => x.Value.Storage);
    }
}
