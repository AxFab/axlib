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
using System.Drawing;

namespace AxToolkit.Graphics;

public struct HSLColor : IEquatable<HSLColor>
{
    public float Hue { get; set; }
    public float Saturation { get; set; }
    public float Luminance { get; set; }

    public static HSLColor FromColor(Color color)
    {
        throw new NotImplementedException();
    }

    public Color AsColor
    {
        get
        {
            throw new NotImplementedException();
        }
    }


    public bool Equals(HSLColor other)
        => AsColor.Equals(other.AsColor);

    public static explicit operator Color(HSLColor hsl) => hsl.AsColor;
}
