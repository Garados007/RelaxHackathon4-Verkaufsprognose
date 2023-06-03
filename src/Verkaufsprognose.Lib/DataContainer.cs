namespace Verkaufsprognose;

public sealed record class DataContainer(
    Dictionary<int, Product> Products,
    Dictionary<int, int> Storage
)
{
    public static DataContainer Create(IEnumerable<Product> products, IEnumerable<Storage> storage)
    {
        return new(
            products.ToDictionary(x => x.Id),
            storage.ToDictionary(x => x.ProductId, x => x.Count)
        );
    }
}
