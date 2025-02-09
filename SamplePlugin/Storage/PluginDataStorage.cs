using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SamplePlugin.Collector;

namespace SamplePlugin.Storage;

public class PluginDataStorage
{
    [JsonProperty]
    private readonly List<HouseInfoEntry> houseInfoEntries = [];

    [JsonProperty]
    private readonly PaginationInfo paginationInfo = new();

    public HousingTag? TagFilter { get; private set; }
    public string GlobalFilter = "";
    public short WorldIdFilter = -1;
    public bool OnlyOpen = false;

    [JsonIgnore]
    public List<HouseInfoEntry> EntriesToDisplay = [];
    [JsonIgnore]
    public int CurrentPageCount = 0;
    [JsonIgnore]
    public int CurrentAvailableEntries = 0;

    public static PluginDataStorage Instantiate()
    {
        var loaded = Load();
        var res = loaded ?? new PluginDataStorage();
        res.RecalculateEntries();
        return res;
    }

    public void AddInfoEntries(List<HouseInfoEntry> entries)
    {
        entries.ForEach(AddToEntries);
        Persist();
        RecalculateEntries();
    }

    public void SetTagFilter(HousingTag filter)
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

    public void SetWorldIdFilter(short worldId)
    {
        WorldIdFilter = worldId;
        Persist();
        RecalculateEntries();
    }

    public void SetGlobalFilter(string globalFilter)
    {
        GlobalFilter = globalFilter;
        Persist();
        RecalculateEntries();
    }
    
    public void SetOpenFilter(bool onlyOpen)
    {
        OnlyOpen = onlyOpen;
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
                               .ThenBy(entry => entry.HouseId.LandId)
                               .ThenBy(entry => entry.HouseId.WardNumber)
                               .ThenBy(entry => entry.HouseId.PlotNumber)
                               .ToList();
    }

    private void AddToEntries(HouseInfoEntry newEntry)
    {
        var matchingEntry = houseInfoEntries.Find(existingEntry => existingEntry.IsSamePot(newEntry));
        if (matchingEntry != null)
        {
            houseInfoEntries.Remove(matchingEntry);
            houseInfoEntries.Add(newEntry);
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

    private void RecalculateEntries()
    {
        var filtered = houseInfoEntries
                       .Where(entry => entry.HouseMetaData.EstateOwnerName.ToLower()
                                            .Contains(GlobalFilter.ToLower()))
                       .Where(entry => entry.HasTag(TagFilter))
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
                       .OrderBy(entry => entry.HouseId.WorldId)
                       .ThenBy(entry => entry.HouseId.LandId)
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
