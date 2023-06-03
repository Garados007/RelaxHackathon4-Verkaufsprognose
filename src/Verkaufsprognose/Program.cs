using MaxLib.WebServer;
using MaxLib.WebServer.Builder;
using Serilog;
using Serilog.Events;

namespace Verkaufsprognose;

public class Program
{
    public static async Task Main(string[] args)
    {
        // setup serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console(LogEventLevel.Verbose,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        WebServerLog.LogPreAdded += WebServerLog_LogPreAdded;

        // setup server
        using var server = new Server(new WebServerSettings(port: 3000, connectionTimeout: 5000));
        server.InitialDefault(); // initialize default services
        var builder = Service.Build<Routings>();
        if (builder is not null)
            server.AddWebService(builder);

        // run server
        await server.RunAsync();
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
