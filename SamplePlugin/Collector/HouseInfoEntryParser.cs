using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SamplePlugin.Collector;

public static class HouseInfoEntryParser
{
    // From PaissaHouse: https://github.com/zhudotexe/FFXIV_PaissaHouse
    public static unsafe List<HouseInfoEntry> Parse(IntPtr dataPtr)
    {
        using var unmanagedMemoryStream = new UnmanagedMemoryStream((byte*)dataPtr.ToPointer(), 2664L);
        using var binaryReader = new BinaryReader(unmanagedMemoryStream);
        var landIdent = LandIdent.ReadFromBinaryReader(binaryReader);
        var result = new List<HouseInfoEntry>();

        for (var i = 0; i < 60; i++)
        {
            var houseId = new HouseId(landIdent.WorldId, landIdent.LandId, landIdent.WardNumber, i + 1);
            var territoryTypeId = landIdent.TerritoryTypeId;
            // skip house price
            binaryReader.ReadUInt32();
            var infoFlags = (HousingFlags)binaryReader.ReadByte();
            var tagA = (HousingTag)binaryReader.ReadSByte();
            var tagB = (HousingTag)binaryReader.ReadSByte();
            var tagC = (HousingTag)binaryReader.ReadSByte();
            var estateOwnerName = Encoding.UTF8.GetString(binaryReader.ReadBytes(32)).TrimEnd(new char[1]);

            // if a house is unowned, the ownerName can be literally anything, so set it to empty string
            if ((infoFlags & HousingFlags.PlotOwned) == 0)
            {
                estateOwnerName = "";
            }

            var houseMetaInfo = new HouseMetaData(territoryTypeId, estateOwnerName, infoFlags, tagA, tagB, tagC);
            var houseInfoEntry = new HouseInfoEntry()
            {
                HouseId = houseId,
                HouseMetaData = houseMetaInfo,
            };
            result.Add(houseInfoEntry);
        }

        // 0x2440 - Skip PruchaseType
        binaryReader.ReadByte();
        // 0x2441 - padding byte?
        binaryReader.ReadByte();
        // 0x2442 - Skip tenant type
        binaryReader.ReadByte();
        // 0x2443 - padding byte?
        binaryReader.ReadByte();
        // 0x2444 - 0x2447 appear to be padding bytes

        return result;
    }
}
