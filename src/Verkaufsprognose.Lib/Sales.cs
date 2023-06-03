using System.Globalization;
using System.Text.Json.Serialization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace Verkaufsprognose;

public class Sales
{
    [JsonPropertyName("date"), Ignore]
    public DateTime Date { get; set; }

    [JsonIgnore, Name("date")]
    public string DateString
    {
        get => Date.ToString("yyyy-MM-dd");
        set => Date = DateTime.Parse(value);
    }

    [JsonPropertyName("product_id"), Name("product_id")]
    public int ProductId { get; set; }

    [JsonPropertyName("product_count"), Name("product_count")]
    public uint ProductCount { get; set; }

    [JsonPropertyName("customer_count"), Name("customer_count")]
    public uint CustomerCount { get; set; }

    [JsonPropertyName("sale_price"), Name("sale_price")]
    public float SalesPrice { get; set; }

    public static async IAsyncEnumerable<Sales> GetSalesAsync(string fileName)
    {
        using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {

        });
        await foreach (var sales in csv.GetRecordsAsync<Sales>())
        {
            yield return sales;
        }
    }
}
