using MaxLib.WebServer.Builder.Tools;

namespace Verkaufsprognose;

public class DateConverter : IConverter
{
    public Func<object?, object?>? GetConverter(Type source, Type target)
    {
        if (source.IsAssignableTo(typeof(string)) && target.IsAssignableTo(typeof(DateTime)))
        {
            return (dateStr) =>
            {
                if (dateStr is string str && DateTime.TryParse(str, out var result))
                    return result;
                else return null;
            };
        }
        return null;
    }
}
