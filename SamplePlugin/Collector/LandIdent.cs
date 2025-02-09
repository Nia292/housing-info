using System.IO;

namespace SamplePlugin.Collector;

public class LandIdent {
    public short LandId;
    public short TerritoryTypeId;
    public short WardNumber;
    public short WorldId;

    public static LandIdent ReadFromBinaryReader(BinaryReader binaryReader) {
        var landIdent = new LandIdent();
        landIdent.LandId = binaryReader.ReadInt16();
        landIdent.WardNumber = binaryReader.ReadInt16();
        landIdent.TerritoryTypeId = binaryReader.ReadInt16();
        landIdent.WorldId = binaryReader.ReadInt16();
        return landIdent;
    }
}
