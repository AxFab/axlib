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

public class Histogram
{
    public Histogram() { }
    public Histogram(double[] arr)
    {
        arr = arr.OrderBy(x => x).ToArray();
        Min = arr[0];
        Max = arr[arr.Length - 1];
        Avg = arr.Average();

        Quart1 = arr[arr.Length / 4];
        Median = arr[arr.Length / 2];
        Quart3 = arr[3 * arr.Length / 4];

        Decil1 = arr[1 * arr.Length / 10];
        Decil2 = arr[2 * arr.Length / 10];
        Decil3 = arr[3 * arr.Length / 10];
        Decil4 = arr[4 * arr.Length / 10];
        Decil6 = arr[6 * arr.Length / 10];
        Decil7 = arr[7 * arr.Length / 10];
        Decil8 = arr[8 * arr.Length / 10];
        Decil9 = arr[9 * arr.Length / 10];
    }

    public double Min { get; set; }
    public double Max { get; set; }
    public double Avg { get; set; }
    public double Quart1 { get; set; }
    public double Median { get; set; }
    public double Quart3 { get; set; }
    public double Decil1 { get; set; }
    public double Decil2 { get; set; }
    public double Decil3 { get; set; }
    public double Decil4 { get; set; }
    public double Decil6 { get; set; }
    public double Decil7 { get; set; }
    public double Decil8 { get; set; }
    public double Decil9 { get; set; }
}
