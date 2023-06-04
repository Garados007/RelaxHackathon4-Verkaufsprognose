using System.Text.Json;

namespace Trainer;

public class TrainerSingle
{
    public TrainerSingle(TrainingSetup setup, int productId, float cutoff)
    {
        Setup = new(
            setup.Products.Where(x => x.Key == productId).ToDictionary(x => x.Key, x => x.Value),
            setup.Storage.Where(x => x.Key == productId).ToDictionary(x => x.Key, x => x.Value),
            setup.FullSales.Where(x => x.ProductId == productId).ToList(),
            setup.PartialSales.Where(x => x.ProductId == productId).ToList()
        );
        ProductId = productId;
        Cutoff = cutoff;
    }

    public TrainingSetup Setup { get; }

    public int ProductId { get; }

    public float Cutoff { get; }

    public (float, float) Result { get; private set; }

    public float Score { get; private set; }

    public async Task Train()
    {
        using var stream = new FileStream($"sol/trainer-{ProductId:#00}.json", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Indented = true,
        });
        writer.WriteStartArray();

        var previous = float.MinValue;
        var a = (0.0f, 4f);
        var b = (0.0f, 4f);
        var step = 0;
        int force = 0;
        while (true)
        {
            var trainer = new TrainerRange(Setup, a, b);
            var (na, nb, score) = await trainer.RunAsync();
            a = Clamp(na);
            b = Clamp(nb);
            step++;

            writer.WriteStartObject();
            writer.WriteNumber("step", step);
            writer.WriteNumber("score", score);
            writer.WriteStartArray("a");
            writer.WriteNumberValue(a.Item1);
            writer.WriteNumberValue(a.Item2);
            writer.WriteEndArray();
            writer.WriteStartArray("b");
            writer.WriteNumberValue(b.Item1);
            writer.WriteNumberValue(b.Item2);
            writer.WriteEndArray();
            writer.WriteEndObject();

            Score = score;

            Console.WriteLine($"[{ProductId:#00}] {score}: {a};{b}");
            if (Math.Abs(score - previous) < Cutoff)
            {
                if (force > 3)
                    break;
                else force++;
            }
            else force = 0;
            previous = score;
        }

        writer.WriteEndArray();
        await writer.FlushAsync();

        stream.SetLength(stream.Position);

        Result = (Middle(a), Middle(b));
    }

    private static float Middle((float, float) value)
    {
        return (value.Item1 + value.Item2) * 0.5f;
    }

    private static (float, float) Clamp((float, float) value)
    {
        return (Math.Clamp(value.Item1, 0, 4), Math.Clamp(value.Item2, 0, 4));
    }
}
