using System.Text.Json;
using Verkaufsprognose;

namespace Trainer;

public class Program
{
    public static TrainingSetup Data { get; private set; } = new(new(), new(), new(), new());

    public static async Task Main(string[] args)
    {
        // load data
        if (!await SetupData(args))
            return;

        if (!Directory.Exists("sol"))
            Directory.CreateDirectory("sol");

        var list = new List<TrainerSingle>();
        for (int i = 0; i < 100; i++)
            list.Add(new TrainerSingle(Data, i, 0.01f));
        await Task.WhenAll(list.Select(x => x.Train()));

        using var stream = new FileStream("solution.json", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Indented = true,
        });
        writer.WriteStartObject();

        foreach (var trainer in list)
        {
            writer.WriteStartObject(trainer.ProductId.ToString());
            writer.WriteNumber("score", trainer.Score);
            writer.WriteNumber("a", trainer.Result.Item1);
            writer.WriteNumber("b", trainer.Result.Item2);
            writer.WriteEndObject();
        }

        writer.WriteEndObject();
        await writer.FlushAsync();

        stream.SetLength(stream.Position);
    }

    // load data

    private static async Task<bool> SetupData(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("A minimum of one argument expected");
            return false;
        }
        if (!Directory.Exists(args[0]))
        {
            Console.Error.WriteLine("Directory {dir} does not exists as data directory", args[0]);
            return false;
        }

        // load products
        var file = Path.Combine(args[0], "products.csv");
        if (!File.Exists(file))
        {
            Console.Error.WriteLine("File {file} not found", file);
            return false;
        }
        var prods = new Dictionary<int, Product>();
        await foreach (var product in Product.GetProductsAsync(file))
            prods.Add(product.Id, product);

        // load storage
        file = Path.Combine(args[0], "storage_02.csv");
        if (!File.Exists(file))
        {
            Console.Error.WriteLine("File {file} not found", file);
            return false;
        }
        var storage = new Dictionary<int, int>();
        await foreach (var entry in Storage.GetStorageAsync(file))
            storage.Add(entry.ProductId, entry.Count);

        // load predictable sales
        file = Path.Combine(args[0], "sales_task2.csv");
        if (!File.Exists(file))
        {
            Console.Error.WriteLine("File {file} not found", file);
            return false;
        }
        var partialSales = new List<Sales>();
        await foreach (var entry in Sales.GetSalesAsync(file))
            partialSales.Add(entry);
        file = Path.Combine(args[0], "sales_task.csv");
        if (!File.Exists(file))
        {
            Console.Error.WriteLine("File {file} not found", file);
            return false;
        }
        var fullSales = new List<Sales>();
        await foreach (var entry in Sales.GetSalesAsync(file))
            fullSales.Add(entry);

        Data = new(prods, storage, fullSales, partialSales);
        return true;
    }
}
