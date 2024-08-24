using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxToolkit.Charts
{
    public class NiceScale
    {
        /// <summary>Instanciates a new instance of the NiceScale class.</summary>
        /// <param name="min">the minimum data point on the axis</param>
        /// <param name="max">the maximum data point on the axis</param>
        /// <param name="maxTicks">the maximum number of tick allowed on the axis</param>
        public NiceScale(decimal min, decimal max, int maxTicks = 10)
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

        /// <summary>Returns a "nice" number approximation equal to range.
        /// Rounds the number if round = true. Take the Ceiling if round = false.</summary>
        /// <param name="range"></param>
        /// <param name="round"></param>
        /// <returns></returns>
        public static decimal NiceNum(decimal range, bool round = false)
        {
            double niceFraction; // nice, rounded fraction
            var exponent = Math.Floor(Math.Log10((double)range)); // exponent of range
            var fraction = (double)range / Math.Pow(10, (double)exponent); // fractionnal part of range

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
}
