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
namespace AxToolkit.Mathematics.Stats;

public class Statistics
{
    public int Count { get; set; }
    public double Sum { get; set; }
    public double SqSum { get; set; }
    public double Sq3Sum { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Avg => Count != 0 ? Sum / Count : 0;
    public double Var => Count != 0 ? SqSum / Count - (Avg * Avg) : 0;
    public double Var3 => Count != 0 ? Sq3Sum / Count - (Avg * Avg) : 0;

    public void Push(double value) => Push(value, 1);
    public void Push(double value, int count)
    {
        if (Count == 0)
            Min = Max = value;
        Count += count;
        Sum += value * count;
        SqSum += value * value * count;
        Sq3Sum += value * value * value * count;
        if (value < Min)
            Min = value;
        if (value > Max)
            Max = value;
    }
}
