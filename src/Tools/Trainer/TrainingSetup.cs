using Verkaufsprognose;

namespace Trainer;

public record class TrainingSetup(
    Dictionary<int, Product> Products,
    Dictionary<int, int> Storage,
    List<Sales> FullSales,
    List<Sales> PartialSales
);
