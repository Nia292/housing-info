namespace SamplePlugin.Collector;

public enum HousingFlags : byte {
    PlotOwned = 1 << 0,
    VisitorsAllowed = 1 << 1,
    HasSearchComment = 1 << 2,
    HouseBuilt = 1 << 3,
    OwnedByFC = 1 << 4
}
