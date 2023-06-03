namespace Verkaufsprognose;

public static class DateUtils
{
    public static readonly DateTime Epoch = new(2023, 02, 01);

    public static int DaysFromEpoch(this DateTime dateTime)
    {
        return (int)(dateTime - Epoch).TotalDays;
    }
}
