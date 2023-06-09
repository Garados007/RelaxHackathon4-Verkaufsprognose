﻿using System.Text.Json;
using MaxLib.WebServer;
using MaxLib.WebServer.Builder;
using Serilog;
using Serilog.Events;

namespace Verkaufsprognose;

public class Program
{
    public static DataContainer Data { get; private set; } = new(new(), new(), new());

    public static Dictionary<int, (float, float)> TrainingConstants { get; } = new();

    public static async Task Main(string[] args)
    {
        // setup serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(LogEventLevel.Verbose,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        WebServerLog.LogPreAdded += WebServerLog_LogPreAdded;

        // load data
        if (!await SetupData(args))
            return;

        // setup server
        using var server = new Server(new WebServerSettings(port: 3000, connectionTimeout: 600000));
        server.InitialDefault(); // initialize default services
        var builder = Service.Build<Routings>();
        if (builder is not null)
            server.AddWebService(builder);

        // run server
        await server.RunAsync();
    }

    // load data

    private static async Task<bool> SetupData(string[] args)
    {
        if (args.Length == 0)
        {
            Log.Fatal("A minimum of one argument expected");
            return false;
        }
        if (!Directory.Exists(args[0]))
        {
            Log.Fatal("Directory {dir} does not exists as data directory", args[0]);
            return false;
        }

        // load products
        Log.Debug("Load products");
        var file = Path.Combine(args[0], "products.csv");
        if (!File.Exists(file))
        {
            Log.Fatal("File {file} not found", file);
            return false;
        }
        var prods = new Dictionary<int, Product>();
        await foreach (var product in Product.GetProductsAsync(file))
            prods.Add(product.Id, product);

        // load storage
        Log.Debug("load storage");
        file = Path.Combine(args[0], "storage_02.csv");
        if (!File.Exists(file))
        {
            Log.Fatal("File {file} not found", file);
            return false;
        }
        var storage = new Dictionary<int, int>();
        await foreach (var entry in Storage.GetStorageAsync(file))
            storage.Add(entry.ProductId, entry.Count);

        // load predictable sales
        Log.Debug("load predictable sales");
        file = Path.Combine(args[0], "sales_task2.csv");
        if (!File.Exists(file))
        {
            Log.Fatal("File {file} not found", file);
            return false;
        }
        var sales = new List<Sales>();
        await foreach (var entry in Sales.GetSalesAsync(file))
            sales.Add(entry);

        if (args.Length == 1)
        {
            Data = new(prods, storage, sales);
            return true;
        }

        // load training data
        Log.Debug("load training data");
        if (!File.Exists(args[1]))
        {
            Log.Fatal("Training file {file} not found", args[1]);
            return false;
        }

        using var stream = new FileStream(args[1], FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
        var doc = await JsonDocument.ParseAsync(stream);
        foreach (var node in doc.RootElement.EnumerateObject())
        {
            var key = int.Parse(node.Name);
            var a = node.Value.GetProperty("a").GetSingle();
            var b = node.Value.GetProperty("b").GetSingle();
            TrainingConstants.Add(key, (a, b));
        }

        Log.Debug("prepare data");
        Data = new(prods, storage, sales, TrainingConstants);

        Log.Debug("data loading finished");
        return true;
    }

    // just some code to move logs from internal web server to serilog

    private static readonly MessageTemplate serilogMessageTemplate =
        new Serilog.Parsing.MessageTemplateParser().Parse(
            "{infoType}: {info}"
        );

    private static void WebServerLog_LogPreAdded(ServerLogArgs e)
    {
        e.Discard = true;
        Log.Write(new LogEvent(
            e.LogItem.Date,
            e.LogItem.Type switch
            {
                ServerLogType.Debug => LogEventLevel.Verbose,
                ServerLogType.Information => LogEventLevel.Debug,
                ServerLogType.Error => LogEventLevel.Error,
                ServerLogType.FatalError => LogEventLevel.Fatal,
                _ => LogEventLevel.Information,
            },
            null,
            serilogMessageTemplate,
            new[]
            {
                new LogEventProperty("infoType", new ScalarValue(e.LogItem.InfoType)),
                new LogEventProperty("info", new ScalarValue(e.LogItem.Information))
            }
        ));
    }
}
