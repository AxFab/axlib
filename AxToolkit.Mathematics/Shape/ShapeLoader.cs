// This file is part of AxLib.
// 
// AxLib is free software: you can redistribute it and/or modify it under the
// terms of the GNU General Public License as published by the Free Software 
// Foundation, either version 3 of the License, or (at your option) any later 
// version.
// 
// AxLib is distributed in the hope that it will be useful, but WITHOUT ANY 
// WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more 
// details.
// 
// You should have received a copy of the GNU General Public License along 
// with AxLib. If not, see <https://www.gnu.org/licenses/>.
// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
namespace AxToolkit.Mathematics.Shape;

public class ShapeLoader
{
    private enum SHPShapeType
    {
        NullShape = 0,
        Point = 1,
        PolyLine = 3,
        Polygon = 5,
        MultiPoint = 8,
        PointZ = 11,
        PolyLineZ = 13,
        PolygonZ = 15,
        MultiPointZ = 18,
        PointM = 21,
        PolyLineM = 23,
        PolygonM = 25,
        MultiPointM = 28,
        MultiPatch = 31,
    }
    private static int SwapBE(int value)
    {
        var b = ((value >> 24) & 0xff) | ((value >> 8) & 0xff00)
            | ((value << 8) & 0xff0000) | ((value << 24) & 0xff000000);
        return (int)b;
    }
    public List<PolyLine> LoadShapeFile(Stream stream) => LoadShapeFile(stream, 0);
    public List<PolyLine> LoadShapeFile(Stream stream, int tag)
    {
        using var file = new BinaryReader(stream);

        // Read file header
        int filecode = file.ReadInt32();
        if (filecode != 0x0a270000)
            throw new FormatException("Bad format");
        file.BaseStream.Seek(24, SeekOrigin.Begin);

        var fileLength = SwapBE(file.ReadInt32()); // BE
        file.ReadInt32(); // LE: fileVersion
        var shapeType = (SHPShapeType)file.ReadInt32(); // LE

        for (int i = 0; i < 8; ++i)
            file.ReadDouble(); // Xmin, Ymin, Xmax, Ymax, Zmin, Zmax, Mmin, Mmax

        var restData = fileLength - (file.BaseStream.Position / 2);
        var list = new List<PolyLine>();
        while (restData > 0)
        {
            // Record Headers
            var recordNumber = SwapBE(file.ReadInt32()); // BE
            var contentLength = SwapBE(file.ReadInt32()); // BE
            var shape = (SHPShapeType)file.ReadInt32(); // LE
            var sz = 0;
            if (shape == SHPShapeType.PolyLine)
                list.Add(LoadPolyLine(file, tag, ref sz));
            else
                throw new FormatException("Unsupported shape");

            if (contentLength != sz)
                throw new FormatException("Corrupted data");

            restData -= contentLength + 4;
        }
        return list;
    }
    private static PolyLine LoadPolyLine(BinaryReader file, int tag, ref int sz)
    {
        var line = new PolyLine();
        line.Tag = tag;
        line.XMin = file.ReadDouble();
        line.YMin = file.ReadDouble();
        line.XMax = file.ReadDouble();
        line.YMax = file.ReadDouble();
        var partCount = file.ReadInt32();
        var pointCount = file.ReadInt32();
        sz = 18 + 4 + 2 * partCount + 8 * pointCount;
        var parts = new int[partCount];
        for (var j = 0; j < partCount; ++j)
            parts[j] = file.ReadInt32();
        for (var j = 0; j < pointCount; ++j)
        {
            var x = file.ReadDouble();
            var y = file.ReadDouble();
            line.Points.Add([x, y]);
        }

        return line;
    }
}
