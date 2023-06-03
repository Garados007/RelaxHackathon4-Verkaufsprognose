using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace Verkaufsprognose;

public sealed class Product
{
    [Name("id")]
    public int Id { get; set; }

    [Name("category_id")]
    public int CategoryId { get; set; }

    [Name("price")]
    public float Price { get; set; }

    [Name("storage_cost")]
    public float StorageCost { get; set; }

    [Name("shipping_duration")]
    public int ShippingDuration { get; set; }

    [Name("shipping_cost")]
    public float ShippingCost { get; set; }

    [Name("rating")]
    public float Rating { get; set; }

    public static async IAsyncEnumerable<Product> GetProductsAsync(string fileName)
    {
        using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
        await foreach (var product in csv.GetRecordsAsync<Product>())
        {
            yield return product;
        }
    }
}
