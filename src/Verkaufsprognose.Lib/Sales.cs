using System.Text.Json.Serialization;

namespace Verkaufsprognose;

public class Sales
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("product_id")]
    public int ProductId { get; set; }

    [JsonPropertyName("product_count")]
    public uint ProductCount { get; set; }

    [JsonPropertyName("customer_count")]
    public uint CustomerCount { get; set; }

    [JsonPropertyName("sale_price")]
    public float SalesPrice { get; set; }
}
