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
namespace AxToolkit.Mathematics;

public static class AxMath
{
    private const double LogCompareDoubleEpsilon = -6.0;

    public const double HalfPI = Math.PI / 2;

    public const double TwoPI = Math.PI * 2;


    public static double Radian2Degree(double val) => val * 180 / Math.PI;
    public static double Degree2Radian(double val) => val * Math.PI / 180;

    public static float Radian2Degree(float val) => val * 180 / (float)Math.PI;
    public static float Degree2Radian(float val) => val * (float)Math.PI / 180;


    public static bool AlmostEqual(double a, double b)
        => Math.Log10(Math.Abs(a - b) / Math.Pow(10, Math.Log10(a + b))) < LogCompareDoubleEpsilon;

}
