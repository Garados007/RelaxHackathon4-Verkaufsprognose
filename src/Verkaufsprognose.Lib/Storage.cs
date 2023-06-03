using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace Verkaufsprognose;

public sealed class Storage
{
    [Name("product_id")]
    public int ProductId { get; }

    [Name("count")]
    public int Count { get; }

    public static async IAsyncEnumerable<Storage> GetStorageAsync(string fileName)
    {
        using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
        await foreach (var storage in csv.GetRecordsAsync<Storage>())
        {
            yield return storage;
        }
    }
}
