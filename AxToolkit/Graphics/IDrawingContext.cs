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

public interface IDrawingContext
{
    float Width { get; }
    float Height { get; }

    void BeginPath();
    void ClosePath();


    void MoveTo(float x, float y);
    void LineTo(float x, float y);
    void QuadTo(float x1, float y1, float x, float y);
    void CurveTo(float x1, float y1, float x2, float y2, float x, float y);

    void Rect(float x, float y, float width, float height, float rx, float ry);
    void Rect(float x, float y, float width, float height) => Rect(x, y, width, height, 0, 0);
    void Arc(float x, float y, float radius, float start, float end);


    void Text(float x, float y, string value, TextAlignement align);
    void Text(float x, float y, string value) => Text(x, y, value, TextAlignement.Left);
    void FontStyle(string family, float size, TextVariant variant = TextVariant.None);

    void StrokeStyle(Color color, float width);
    void StrokeStyle(Color color) => StrokeStyle(color, 1.0f);
    void FillStyle(Color color);

    void Stroke();
    void Fill();


    void Save();
    void Restore();
    void Translate(float x, float y);
    void Scale(float x, float y);
    void Rotate(float arg);
    //IFontFace ResolveFont(string fontFamily, float fontSize, string fontVariant);
    FontMeasure MeasureText(string title);
    //void FontStyle(IFontFace fontFace);
    //void ClipRect(float left, float top, float width, float height);
}
public enum TextAlignement
{
    Left = 0, Right = 1, Center = 2,
    Top = 0, Bottom = 4, Middle = 8,
}
[Flags]
public enum TextVariant
{
    None = 0,
    Bold = 1, Italic = 2, Underline = 4, Strikeout = 8,
    SmallCaps = 16,
}

public interface IFontFace
{
    public string Family { get; }
    public float Size { get; }
    public float LineHeight { get; }
    public float Baseline { get; }
}
public class FontMeasure
{
    /// <summary>The size require into two line, which is font specific</summary>
    public float LineHeight { get; set; }
    /// <summary>The vertical space betwwen the top of the text bounding box and the writing line</summary>
    public float Baseline { get; set; }
    /// <summary>The width of a specific text</summary>
    public float Width { get; set; }
    /// <summary>The height of a specific text painted above the text baseline, should be bellow or equal to baseline</summary>
    public float Ascender { get; set; }
    /// <summary>The height of a specific text painted bellow the text baseline, sould be bellow LineHeight minus Baseline</summary>
    public float Descender { get; set; }
    /// <summary>The height of a specific text, computed by adding Ascender and Descender values.</summary>
    public float Height => Ascender + Descender;
    /// <summary>Is regular boxing rule preserved</summary>
    internal bool RegularBoxing => Ascender <= Baseline && Descender < LineHeight - Baseline;
}
