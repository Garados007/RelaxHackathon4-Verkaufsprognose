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
        return sales;
    }

    [Path("/orders")]
    [Method(HttpProtocolMethod.Get)]
    [return: JsonDataConverter]
    public List<Order> GetOrders([Get, Converter(typeof(DateConverter))] DateTime date)
    {
        return new()
        {
            new Order { OrderAmount = 420, ProductId = 1337 },
        };
    }

    [Path("/products")]
    [Method(HttpProtocolMethod.Get)]
    [return: JsonDataConverter]
    public Dictionary<int, Product> GetProducts()
    {
        return Program.Data.Products;
    }

    [Path("/storage")]
    [Method(HttpProtocolMethod.Get)]
    [return: JsonDataConverter]
    public Dictionary<int, int> GetStorage()
    {
        return Program.Data.Storage;
    }
}
