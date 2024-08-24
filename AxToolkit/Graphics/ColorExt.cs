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
using System.Text.RegularExpressions;

namespace AxToolkit.Graphics;

public static class ColorExt
{
    public static Color Alpha(this Color color, float value)
    {
        var alpha = 255 * Math.Min(1, Math.Max(0, value));
        return Color.FromArgb((int)alpha, color.R, color.G, color.B);
    }

    public static Color Parse(string value)
    {
        int a, r, g, b;
        var rg1 = new Regex("^#[0-9a-f]{6}$");
        if (rg1.IsMatch(value))
        {
            r = Convert.ToInt32(value.Substring(1, 2), 16);
            g = Convert.ToInt32(value.Substring(3, 2), 16);
            b = Convert.ToInt32(value.Substring(5, 2), 16);
            return Color.FromArgb(r, g, b);
        }

        var rg2 = new Regex("^rgb\\(\\s*(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)\\s*\\)$");
        var mt2 = rg2.Match(value);
        if (mt2.Success)
        {
            r = int.Parse(mt2.Groups[1].Value);
            g = int.Parse(mt2.Groups[2].Value);
            b = int.Parse(mt2.Groups[3].Value);
            return Color.FromArgb(r, g, b);
        }

        var rg3 = new Regex("^rgba\\(\\s*(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+)\\s*,\\s*(\\d+(\\.\\d+)?)\\s*\\)$");
        var mt3 = rg3.Match(value);
        if (mt3.Success)
        {
            r = int.Parse(mt2.Groups[1].Value);
            g = int.Parse(mt2.Groups[2].Value);
            b = int.Parse(mt2.Groups[3].Value);
            a = (int)(255 * float.Parse(mt3.Groups[4].Value));
            return Color.FromArgb(a, r, g, b);
        }

        return Color.FromKnownColor(Enum.Parse<KnownColor>(value));
    }

    public static Color Lighter(this Color color, float value)
    {
        var hsl = HSLColor.FromColor(color);
        hsl.Luminance = 1.0f - (1.0f - hsl.Luminance) * Math.Min(1, Math.Max(0, value));
        return (Color)hsl;
    }
    public static Color Darker(this Color color, float value)
    {
        var hsl = HSLColor.FromColor(color);
        hsl.Luminance = hsl.Luminance * Math.Min(1, Math.Max(0, value));
        return (Color)hsl;
    }
    public static Color Brighter(this Color color, float value)
    {
        var hsl = HSLColor.FromColor(color);
        hsl.Saturation = 1.0f - (1.0f - hsl.Saturation) * Math.Min(1, Math.Max(0, value));
        return (Color)hsl;
    }
}

public struct HSLColor
{
    public float Hue { get; set; }
    public float Saturation { get; set; }
    public float Luminance { get; set; }

    public static HSLColor FromColor(Color color)
    {
        throw new NotImplementedException();
    }

    public static explicit operator Color(HSLColor hsl)
    {
        throw new NotImplementedException();
    }

}
