using System.Text.Json.Serialization;

namespace Verkaufsprognose;

public class Order
{
    [JsonPropertyName("product_id")]
    public int ProductId { get; set; }

    [JsonPropertyName("order_amount")]
    public int OrderAmount { get; set; }
}
