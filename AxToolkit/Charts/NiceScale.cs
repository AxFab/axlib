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
namespace AxToolkit.Charts;

public class NiceScale
{
    /// <summary>Instanciates a new instance of the NiceScale class with 10 max ticks.</summary>
    /// <param name="min">the minimum data point on the axis</param>
    /// <param name="max">the maximum data point on the axis</param>
    public NiceScale(decimal min, decimal max) : this(min, max, 10) { }

    /// <summary>Instanciates a new instance of the NiceScale class.</summary>
    /// <param name="min">the minimum data point on the axis</param>
    /// <param name="max">the maximum data point on the axis</param>
    /// <param name="maxTicks">the maximum number of tick allowed on the axis</param>
    public NiceScale(decimal min, decimal max, int maxTicks)
    {
        MinPoint = min;
        MaxPoint = max;
        MaxTicks = maxTicks;
        Compute();
    }

    public decimal MinPoint { get; private set; }
    public decimal MaxPoint { get; private set; }
    public int MaxTicks { get; private set; }
    public decimal Range { get; private set; }
    public decimal TickSpacing { get; private set; }
    public decimal NiceMin { get; private set; }
    public decimal NiceMax { get; private set; }
    public decimal FinalRange => NiceMax - NiceMin;
    public int Decimals => TickSpacing.Scale; // TickSpacing < 1 ? 0 : (int)Math.Abs(Math.Floor(Math.Log10((double)TickSpacing)));

    /// <summary>Calculate and update values for the tick spacing and nice minimum and maximum data point on the axis.</summary>
    private void Compute()
    {
        Range = NiceNum(MaxPoint - MinPoint, false);
        TickSpacing = NiceNum(Range / (MaxTicks - 1), true);
        NiceMin = Math.Floor(MinPoint / TickSpacing) * TickSpacing;
        NiceMax = Math.Ceiling(MaxPoint / TickSpacing) * TickSpacing;
    }

    /// <summary>Returns a "nice" number approximation equal to range and rounds the number.</summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public static decimal NiceNum(decimal range) => NiceNum(range, false); 

    /// <summary>Returns a "nice" number approximation equal to range.
    /// Rounds the number if round = true. Take the Ceiling if round = false.</summary>
    /// <param name="range"></param>
    /// <param name="round"></param>
    /// <returns></returns>
    public static decimal NiceNum(decimal range, bool round)
    {
        double niceFraction; // nice, rounded fraction
        var exponent = Math.Floor(Math.Log10((double)range)); // exponent of range
        var fraction = (double)range / Math.Pow(10, exponent); // fractionnal part of range

        if (round)
            niceFraction = fraction < 1.5 ? 1 : fraction < 3 ? 2 : fraction < 7 ? 5 : 10;
        else
            niceFraction = fraction < 1 ? 1 : fraction < 2 ? 2 : fraction < 5 ? 5 : 10;
        return (decimal)(niceFraction * Math.Pow(10, exponent));
    }

    public IEnumerable<decimal> Enumerate()
    {
        var value = NiceMin;
        while (value <= NiceMax)
        {
            yield return value;
            value += TickSpacing;
        }
    }
}
