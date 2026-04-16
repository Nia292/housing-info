using System;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;
using HousingDatabase.Collector;
using Action = System.Action;

namespace HousingDatabase.Windows;

public static class InterfaceUtils
{

    public static void WithinId(string id, Action action)
    {
        ImGui.PushID(id);
        action();
        ImGui.PopID();
    }

    public static short ResolveTerritoryId(string name)
    {
        if (name.Equals("The Goblet", StringComparison.OrdinalIgnoreCase))
        {
            return 341;
        }
        if (name.Equals("The Lavender Beds", StringComparison.OrdinalIgnoreCase))
        {
            return 340;
        }
        if (name.Equals("Mist", StringComparison.OrdinalIgnoreCase))
        {
            return 339;
        }
        if (name.Equals("Empyreum", StringComparison.OrdinalIgnoreCase))
        {
            return 979;
        }
        if (name.Equals("Shirogane", StringComparison.OrdinalIgnoreCase))
        {
            return 641;
        }

        return -1;
    }
    
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
        
        if (landId == 641)
        {
            return "Shirogane";
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
