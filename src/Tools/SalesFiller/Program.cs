using System.Globalization;
using CsvHelper;
using Verkaufsprognose;

namespace Tools.SalesFiller;

public static class Program
{
    public const int PRODUCT_COUNT = 100;

    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("expect path to sales_task.csv as first argument");
            return;
        }
        if (!File.Exists(args[0]))
        {
            Console.Error.WriteLine($"File not found {args[0]}");
            return;
        }

        var dict = new Dictionary<DateTime, Memory<Sales>>();
        await foreach (var sales in Sales.GetSalesAsync(args[0]))
        {
            if (!dict.TryGetValue(sales.Date, out var list))
            {
                dict.Add(sales.Date, list = CreateNew(sales.Date));
            }
            if (sales.ProductId >= list.Length)
                Console.Error.WriteLine(sales.ProductId);
            list.Span[sales.ProductId] = sales;
        }

        var minDate = dict.Keys.Min().Date;
        var maxDate = dict.Keys.Max().Date;
        for (var date = minDate; date < maxDate; date = date.AddDays(1))
            if (!dict.ContainsKey(date))
                dict.Add(date, CreateNew(date));

        using var csv = new CsvWriter(Console.Out, CultureInfo.InvariantCulture, true);
        foreach (var (_, d) in dict)
        {
            for (int i = 0; i < d.Length; ++i)
            {
                csv.WriteRecord(d.Span[i]);
                csv.NextRecord();
            }
        }
        csv.Flush();
    }

    private static Memory<Sales> CreateNew(DateTime time)
    {
        var list = new Sales[PRODUCT_COUNT];
        for (int i = 0; i < PRODUCT_COUNT; ++i)
        {
            list[i] = new Sales
            {
                CustomerCount = 0,
                Date = time,
                ProductCount = 0,
                ProductId = i,
                SalesPrice = 0,
            };
        }
        return list;
    }
}
