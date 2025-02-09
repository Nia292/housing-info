using System;
using Lumina.Excel.Sheets;
using SamplePlugin.Collector;

namespace SamplePlugin.Windows;

public class InterfaceUtils
{
    public static string TranslateTerritoryTypeId(short landId)
    {
        if (landId == -1)
        {
            return "";
        }
        
        if (landId == 341)
        {
            return "The Goblet";
        }
        
        if (landId == 340)
        {
            return "The Lavender Beds";
        }

        if (landId == 339)
        {
            return "Mist";
        }

        if (landId == 979)
        {
            return "Empyreum";
        }
        return landId.ToString();
    }

    public static string TranslateWorld(short worldId)
    {
        if (worldId == -1)
        {
            return "";
        }

        Plugin.DataManager.GetExcelSheet<World>().TryGetRow((uint)worldId, out var worldRow);
        return worldRow.Name.ExtractText();
    }

    public static string TranslateHousingTag(HousingTag? tag)
    {
        if (tag == null)
        {
            return "";
        }

        return tag switch
        {
            HousingTag.None => "",
            HousingTag.Emporium => "Emporium",
            HousingTag.Boutique => "Boutique",
            HousingTag.DesignerHome => "Designer Home",
            HousingTag.MessageBook => "Message Book",
            HousingTag.Tavern => "Tavern",
            HousingTag.Eatery => "Eatery",
            HousingTag.ImmersiveExperience => "Immersive Experience",
            HousingTag.Cafe => "Cafe",
            HousingTag.Aquarium => "Aquarium",
            HousingTag.Sanctuary => "Sanctuary",
            HousingTag.Venue => "Venue",
            HousingTag.Florist => "Florist",
            HousingTag.Unknown13 => "Unknown (13)",
            HousingTag.Library => "Library",
            HousingTag.PhotoStudio => "Photo Studio",
            HousingTag.HauntedHouse => "Haunted House",
            HousingTag.Atelier => "Atelier",
            HousingTag.Bathhouse => "Bathhouse",
            HousingTag.Garden => "Garden",
            HousingTag.FarEastern => "Far Eastern",
            HousingTag.VisitorsWelcome => "Visitors Welcome",
            HousingTag.UnderConstruction => "Under Construction",
            HousingTag.Bakery => "Bakery",
            HousingTag.ConcertHall => "Concert Hall",
            HousingTag.Unknown25 => "Unknown (25)",
            HousingTag.Unknown26 => "Unknown (27)",
            HousingTag.Unknown27 => "Unknown (27)",
            HousingTag.Unknown28 => "Unknown (28)",
            HousingTag.Unknown29 => "Unknown (29)",
            HousingTag.Unknown30 => "Unknown (30)",
            _ => throw new ArgumentOutOfRangeException(nameof(tag), tag, null)
        };
    }
}
