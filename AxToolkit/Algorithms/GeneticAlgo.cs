using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxToolkit.Algorithms
{
    public abstract class GeneticAlgo<TGenome, TStatus> where TGenome : IGenome
    {
        protected Random _random = new Random();
        public int PopulasSize { get; set; }
        public int SurvivorSze { get; set; }
        public TGenome Searching(TStatus status)
        {
            StartSearch();
            var populas = new List<TGenome>();
            while (populas.Count < PopulasSize)
                populas.Add(GenerateGenome());

            for (; ; )
            {
                foreach (var genome in populas)
                    EvaluateScore(genome, status);
                populas.Sort(Compare);
                if (SearchDone())
                    return populas.First();

                var nextGen = new List<TGenome>();
                nextGen.AddRange(populas.Take(SurvivorSze));
                while (nextGen.Count < PopulasSize)
                {
                    var a = _random.Next(SurvivorSze);
                    var b = _random.Next(SurvivorSze);
                    while (a == b)
                        b = _random.Next(SurvivorSze);
                    nextGen.Add(Mutate(nextGen[a], nextGen[b]));
                }
                populas = nextGen;
            }
        }

        protected virtual void StartSearch() { }
        protected abstract bool SearchDone();
        protected abstract TGenome GenerateGenome();
        protected abstract TGenome Mutate(TGenome x, TGenome y);
        protected abstract void EvaluateScore(TGenome genome, TStatus status);
        protected int Compare(TGenome x, TGenome y)
        {
            var sc = y.Score - x.Score;
            if (sc > 0)
                return 1;
            if (sc < 0) 
                return -1;
            return 0;
        }
    }

    public abstract class TimeLimitedGeneticAlgo<TGenome, TStatus> : GeneticAlgo<TGenome, TStatus> where TGenome : IGenome
    {
        public TimeSpan Duration { get; set; }
        private Stopwatch _chrono;
        protected override void StartSearch() 
        {
            _chrono = Stopwatch.StartNew();
        }

        protected override bool SearchDone()
            => _chrono.Elapsed >= Duration;
    }

    public interface IGenome
    {
        public double Score { get; }
    }
}
