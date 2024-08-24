using AxToolkit.Mathematics.Stats;
using System.Diagnostics;

namespace AxToolkit.Mathematics.Intelligence;
public class NeuralNetwork
{
    struct Neurone
    {
        private double[] _weights;
        public double Value { get; set; }
        public void Update(Neurone[] layer)
        {
            double sum = 0.0;
            for (int i = 0, n = _weights.Length; i < n; ++i)
                sum += layer[i].Value * _weights[i];
            Value = Math.Tan(sum);
        }

        public void Setup(Span<double> weights)
        {
            _weights = new double[weights.Length];
            for (int i = 0, n = weights.Length; i < n; ++i)
                _weights[i] = double.IsRealNumber(weights[i]) ? weights[i] : 0.0;
        }
    }

    private Neurone[][] _layers;

    public Statistics Telemetry { get; set; }

    public static int LinksCount(int[] size)
    {
        var sum = 0;
        for (int j = 1; j < size.Length; ++j)
            sum += size[j] * size[j - 1];
        return sum;
    }

    public static double[] RandomWeights(int size, double max = 5.0)
    {
        var half = max / 2;
        var rand = new Random((int)DateTime.Now.ToFileTime());
        var arr = new double[size];
        for (int i = 0; i < size; ++i)
            arr[i] = (rand.NextDouble() * max - half);
        // arr[i] = 1.0 / (rand.NextDouble() * max - half);
        return arr;
    }

    public NeuralNetwork(int[] size, double[] weights = null)
    {
        if (weights == null)
            weights = RandomWeights(LinksCount(size));

        var list = new List<Neurone[]>();
        for (int j = 1; j < size.Length; ++j)
            list.Add(new Neurone[size[j]]);

        _layers = list.ToArray();

        int off = 0;
        for (int j = 1; j < _layers.Length; ++j)
        {
            var sub = _layers[j - 1];
            var sup = _layers[j];

            int len = sub.Length;
            for (int i = 0; i < sup.Length; ++i)
            {
                sup[i].Setup(weights.AsSpan(off, len));
                off += len;
            }
        }
    }
    public void Compute()
    {
        var chrono = Telemetry != null ? Stopwatch.StartNew() : null;
        for (int j = 1; j < _layers.Length; ++j)
        {
            var sub = _layers[j - 1];
            var sup = _layers[j];
            for (int i = 0; i < sup.Length; ++i)
                sup[i].Update(sub);
        }

        if (chrono != null)
        {
            chrono.Stop();
            Telemetry.Push(chrono.ElapsedMilliseconds);
        }
    }

    public int InputLength => _layers[0].Length;
    public int OutputLength => _layers[_layers.Length - 1].Length;
    public double this[int idx]
    {
        get => _layers[_layers.Length - 1][idx].Value;
        set => _layers[0][idx].Value = value;
    }
}

