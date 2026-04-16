using System.Text;
using Newtonsoft.Json;

namespace HousingDatabase.Collector;

public class HouseInfoEntry
{

    [JsonProperty]
    public required HouseId HouseId;
    [JsonProperty]
    public required HouseMetaData HouseMetaData;

    public bool Favorite;
    public bool Visit;
    public string Comment = "";
    public string Greeting = "";


    public bool IsSamePlot(HouseInfoEntry other)
    {
        return Equals(HouseId, other.HouseId);
    }
    
    public bool IsSamePlot(HouseId other)
    {
        return Equals(HouseId, other);
    }

    public override string ToString()
    {
        var props = this.GetType().GetProperties();
        var sb = new StringBuilder();
        foreach (var p in props)
        {
            sb.AppendLine(p.Name + ": " + p.GetValue(this, null));
        }
        return sb.ToString();
    }

    public bool HasTag(HousingTag? tagFilter)
    {
        if (tagFilter == null)
        {
            return true;
        }
        return tagFilter == HouseMetaData.TagA || tagFilter == HouseMetaData.TagB || tagFilter == HouseMetaData.TagC;
    }

    public bool IsOpen()
    {
        return !((HouseMetaData.InfoFlags & HousingFlags.VisitorsAllowed) == 0);
    }
    
    public bool IsOwned()
    {
        return !((HouseMetaData.InfoFlags & HousingFlags.PlotOwned) == 0);
    }

    
    public bool IsFreeCompany()
    {
        return !((HouseMetaData.InfoFlags & HousingFlags.OwnedByFC) == 0);
    }

    public string GetFormattedOwnerName()
    {
        if (!IsOwned())
        {
            return "";
        }
        if (IsFreeCompany())
        {
            return $"<{HouseMetaData.EstateOwnerName}>";
        }
        return HouseMetaData.EstateOwnerName;
    }
}
