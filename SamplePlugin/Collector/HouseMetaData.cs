using Newtonsoft.Json;

namespace SamplePlugin.Collector;

public class HouseMetaData
{
   
    [JsonProperty]
    public readonly string EstateOwnerName;
    [JsonProperty]
    public readonly HousingFlags InfoFlags;
    [JsonProperty]
    public readonly HousingTag TagA;
    [JsonProperty]
    public readonly HousingTag TagB;
    [JsonProperty]
    public readonly HousingTag TagC;

    public HouseMetaData(string estateOwnerName, HousingFlags infoFlags, HousingTag tagA, HousingTag tagB, HousingTag tagC)
    {
        EstateOwnerName = estateOwnerName;
        InfoFlags = infoFlags;
        TagA = tagA;
        TagB = tagB;
        TagC = tagC;
    }
}
