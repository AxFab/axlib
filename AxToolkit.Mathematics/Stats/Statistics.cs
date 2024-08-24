using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
