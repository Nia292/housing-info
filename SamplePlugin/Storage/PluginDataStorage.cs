using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Newtonsoft.Json;
using SamplePlugin.Collector;

namespace SamplePlugin.Storage;

public class PluginDataStorage
{
    [JsonProperty]
    private readonly List<HouseInfoEntry> houseInfoEntries = [];

    [JsonProperty]
    private readonly PaginationInfo paginationInfo = new();

    [JsonProperty]
    public HousingTag? TagFilter { get; private set; }
    [JsonProperty]
    public HousingTag? TagFilter2 { get; private set; }
    [JsonProperty]
    public HousingTag? TagFilter3 { get; private set; }
    [JsonProperty]
    public string OwnerFilter = "";
    [JsonProperty]
    public short? WardFilter = -1;
    public short? WardNumberFilter = null;
    [JsonProperty]
    public short WorldIdFilter = -1;
    [JsonProperty]
    public bool OnlyOpen = false;
    [JsonProperty]
    public bool OnlyFavorites = false;

    [JsonIgnore]
    public List<HouseInfoEntry> EntriesToDisplay = [];
    [JsonIgnore]
    public int CurrentPageCount = 0;
    [JsonIgnore]
    public int CurrentAvailableEntries = 0;
    [JsonIgnore]
    public IPluginLog log;

    public static PluginDataStorage Instantiate(IPluginLog log)
    {
        var loaded = Load();
        var res = loaded ?? new PluginDataStorage();
        res.RecalculateEntries();
        res.log = log;
        return res;
    }

    public void AddInfoEntries(List<HouseInfoEntry> entries)
    {
        entries.ForEach(AddToEntries);
        Persist();
        RecalculateEntries();
    }

    public void SetTagFilter(HousingTag? filter)
    {
        if (filter == HousingTag.None)
        {
            TagFilter = null;
        }
        else
        {
            TagFilter = filter;
        }

        Persist();
        RecalculateEntries();
    }
    
    public void SetTagFilter2(HousingTag? filter)
    {
        if (filter == HousingTag.None)
        {
            TagFilter2 = null;
        }
        else
        {
            TagFilter2 = filter;
        }

        Persist();
        RecalculateEntries();
    }
    
    public void SetTagFilter3(HousingTag? filter)
    {
        if (filter == HousingTag.None)
        {
            TagFilter3 = null;
        }
        else
        {
            TagFilter3 = filter;
        }

        Persist();
        RecalculateEntries();
    }


    public void SetWorldIdFilter(short worldId)
    {
        WorldIdFilter = worldId;
        Persist();
        RecalculateEntries();
    }
    
    public void SetWardNumberFilter(short? wardNumber)
    {
        WardNumberFilter = wardNumber;
        Persist();
        RecalculateEntries();
    }

    public void SetOwnerFilter(string ownerFilter)
    {
        OwnerFilter = ownerFilter;
        Persist();
        RecalculateEntries();
    }
    
    public void SetWardFilter(short wardFilter)
    {
        WardFilter = wardFilter;
        Persist();
        RecalculateEntries();
    }

    
    public void SetOpenFilter(bool onlyOpen)
    {
        OnlyOpen = onlyOpen;
        Persist();
        RecalculateEntries();
    }
    
    public void SetOnlyFavoritesFilter(bool onlyFavorites)
    {
        OnlyFavorites = onlyFavorites;
        Persist();
        RecalculateEntries();
    }

    public void NextPage()
    {
        if (HasNextPage())
        {
            paginationInfo.Page++;
            Persist();
            RecalculateEntries();
        }
    }

    public void PreviousPage()
    {
        if (HasPreviousPage())
        {
            paginationInfo.Page--;
            Persist();
            RecalculateEntries();
        }
    }

    public void FirstPage()
    {
        paginationInfo.Page = 0;
        Persist();
        RecalculateEntries();
    }

    public void LastPage()
    {
        paginationInfo.Page = (CurrentPageCount - 1);
        Persist();
        RecalculateEntries();
    }

    public bool HasNextPage()
    {
        return paginationInfo.Page < (CurrentPageCount - 1);
    }

    public bool HasPreviousPage()
    {
        return paginationInfo.Page > 0;
    }

    public void MarkFavorite(HouseId houseId, bool favorite)
    {
        houseInfoEntries
            .First(entry => entry.HouseId.Equals(houseId))
            .Favorite = favorite;
        Persist();
    }
    
    public void MarkVisit(HouseId houseId, bool visit)
    {
        houseInfoEntries
            .First(entry => entry.HouseId.Equals(houseId))
            .Visit = visit;
        Persist();
    }

    public void SetComment(HouseId houseId, string comment)
    {
        houseInfoEntries
            .First(entry => entry.HouseId.Equals(houseId))
            .Comment = comment;
        Persist();
    }

    public List<HouseInfoEntry> GetHousesToVisit()
    {
        return houseInfoEntries.Where(entry => entry.Visit)
                               .OrderBy(entry => entry.HouseId.WorldId)
                               .ThenBy(entry => entry.HouseId.TerritoryTypeId)
                               .ThenBy(entry => entry.HouseId.WardNumber)
                               .ThenBy(entry => entry.HouseId.PlotNumber)
                               .ToList();
    }

    private void AddToEntries(HouseInfoEntry newEntry)
    {
        var matchingEntry = houseInfoEntries.Find(existingEntry => existingEntry.IsSamePot(newEntry));
        if (matchingEntry != null)
        {
            // houseInfoEntries.Remove(matchingEntry);
            // houseInfoEntries.Add(newEntry);
            // TODO need to check if meta update is needed
        }
        else
        {
            houseInfoEntries.Add(newEntry);
        }
    }


    private void Persist()
    {
        TextWriter? writer = null;
        try
        {
            var contentsToWriteToFile = JsonConvert.SerializeObject(this);
            writer = new StreamWriter(ConfigFilePath(), false);
            writer.Write(contentsToWriteToFile);
        } finally
        {
            writer?.Close();
        }
    }

    private static PluginDataStorage? Load()
    {
        TextReader? reader = null;
        if (!File.Exists(ConfigFilePath()))
        {
            return null;
        }

        try
        {
            reader = new StreamReader(ConfigFilePath());
            var fileContents = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<PluginDataStorage>(fileContents);
        } finally
        {
            reader?.Close();
        }
    }

    private static string ConfigFilePath()
    {
        return Plugin.PluginInterface.GetPluginConfigDirectory() + "/housing-data.json";
    }

    public long GetCurrentPage()
    {
        return paginationInfo.Page;
    }

    public HashSet<short> GetAvailableWorlds()
    {
        return houseInfoEntries.Select(entry => entry.HouseId.WorldId).ToHashSet();
    }
    
    public HashSet<short> GetAvailableWards()
    {
        return houseInfoEntries.Select(entry => entry.HouseId.TerritoryTypeId).ToHashSet();
    }

    private void RecalculateEntries()
    {
        var filtered = houseInfoEntries
                       .Where(entry => entry.HouseMetaData.EstateOwnerName.ToLower()
                                            .Contains(OwnerFilter.ToLower()))
                       .Where(entry =>
                       {
                           bool hasTag1 = TagFilter == null || entry.HasTag(TagFilter);
                           bool hasTag2 = TagFilter2 == null || entry.HasTag(TagFilter2);
                           return hasTag1 && hasTag2;
                       })
                       .Where(entry =>
                       {
                           if (WardFilter == -1  || WardFilter == null)
                           {
                               return true;
                           }
                           return entry.HouseId.TerritoryTypeId == WardFilter;
                       })
                       .Where(entry =>
                       {
                           if (WorldIdFilter == -1)
                           {
                               return true;
                           }

                           return entry.HouseId.WorldId == WorldIdFilter;
                       })
                       .Where(entry =>
                       {
                           if (OnlyOpen)
                           {
                               return entry.IsOpen();
                           }

                           return true;
                       })
                       .Where(entry =>
                       {
                           if (OnlyFavorites)
                           {
                               return entry.Favorite;
                           }

                           return true;
                       })
                       .OrderBy(entry => entry.HouseId.WorldId)
                       .ThenBy(entry => entry.HouseId.TerritoryTypeId)
                       .ThenBy(entry => entry.HouseId.WardNumber)
                       .ThenBy(entry => entry.HouseId.PlotNumber)
                       .ToList();
        
        // If the page exceeds what is currently visible, clamp the page back to the first page where stuff is still visible
        var highestPage = (int) Math.Ceiling(filtered.Count / (double)paginationInfo.ItemsPerPage);
        var resultPage = Math.Min(highestPage, paginationInfo.Page);
        paginationInfo.Page = resultPage;
        CurrentPageCount = highestPage;
        CurrentAvailableEntries = filtered.Count;

        var paged = filtered
                    .Skip((int)(paginationInfo.ItemsPerPage * paginationInfo.Page))
                    .Take(paginationInfo.ItemsPerPage);
        EntriesToDisplay = paged.ToList();
    }
}
