using System;
using Newtonsoft.Json;

namespace SamplePlugin.Collector;

public class HouseId(short worldId, short territoryTypeId, short wardNumber, int plotNumber)
{
    [JsonProperty]
    public readonly short WorldId = worldId;
    [JsonProperty]
    public readonly short TerritoryTypeId =  territoryTypeId;
    [JsonProperty]
    public readonly short WardNumber = wardNumber;
    [JsonProperty]
    public readonly int PlotNumber = plotNumber;

    protected bool Equals(HouseId other)
    {
        return WorldId == other.WorldId && TerritoryTypeId == other.TerritoryTypeId && WardNumber == other.WardNumber && PlotNumber == other.PlotNumber;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((HouseId) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(WorldId, TerritoryTypeId, WardNumber, PlotNumber);
    }

    public override string ToString()
    {
        return $"${WorldId}-{TerritoryTypeId}-{WardNumber}-{PlotNumber}";
    }
}
