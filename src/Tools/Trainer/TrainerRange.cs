namespace Trainer;

public class TrainerRange
{
    public TrainerRange(TrainingSetup setup, (float, float) factorA, (float, float) factorB)
    {
        Setup = setup;
        FactorA = factorA;
        FactorB = factorB;
    }

    public TrainingSetup Setup { get; }

    public (float, float) FactorA { get; }

    public (float, float) FactorB { get; }

    public async Task<((float, float), (float, float), float score)> RunAsync()
    {
        var da = (FactorA.Item2 - FactorA.Item1) / 2;
        var db = (FactorB.Item2 - FactorB.Item1) / 2;

        var slots = new List<TrainerSlot>();
        for (float a = FactorA.Item1; a <= FactorA.Item2; a += da)
            for (float b = FactorB.Item1; b <= FactorB.Item2; b += db)
            {
                // Console.WriteLine($"Add for {a};{b}");
                slots.Add(new TrainerSlot(Setup, a, b));
            }
        await Task.WhenAll(slots.Select(x => x.RunAsync()));
        slots.Sort((a, b) => -a.Score.CompareTo(b.Score));

        var ba = slots[0].FactorA;
        var bb = slots[0].FactorB;

        da *= 0.5f;
        db *= 0.5f;

        return ((ba - da, ba + da), (bb - db, bb + db), slots[0].Score);
    }
}
