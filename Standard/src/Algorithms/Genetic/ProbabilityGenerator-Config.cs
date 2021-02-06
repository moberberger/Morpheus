using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Morpheus.PGNS
{
    public class Config
    {
        public virtual double TargetValue { get; set; } = 222;
        public virtual double[] Values { get; set; } = new double[] { 1, 5, 10, 25, 50, 100, 250, 500, 1000, 5000, 1000000 };

        public virtual int PopulationSize { get; set; } = 300;
        public virtual int MutationCount { get; set; } = 2;
        public virtual double MultiMutateChance { get; set; } = 0.35;
        public virtual double InitialStdev { get; set; } = 0.2;

        public virtual double ErrorTolerance { get; set; } = .40;
        public virtual double TargetValueAcceptableErrorPercent { get; set; } = 0.05;
        public virtual double MinimumProbability { get; set; } = 1e-20;

        public virtual double ProbabilityErrorWeight { get; internal set; } = 10;
        public virtual double AngleErrorWeight { get; internal set; } = 1000;

        public virtual int DirectionCountTarget { get; set; } = 1;
        public virtual double DirectionCountPenalty { get; set; } = 10.0;

        public virtual double MeanIncrementRate { get; set; } = 3;
        public virtual double MinStddevIncrementRate { get; set; } = 1.0;
        public virtual double MaxStddevIncrementRate { get; set; } = 20.0;
        public virtual double StddevIncrementRate
        { // be more exploratory when the error is high
            get
            {
                if (Best == null) return MaxStddevIncrementRate;
                var err = Math.Sqrt( Best.Error );
                var retval = err.Clamp( MinStddevIncrementRate, MaxStddevIncrementRate );
                return retval;
            }
        }
        public virtual int ValueCount => Values.Length;

        public virtual int IterationCount { get; private set; }
        public virtual Chromosome Best { get; internal set; }
        public virtual ObjectPool<Chromosome> Pool { get; internal set; }

        internal IEnumerable<int> LoopUntilErrorSatisfactory()
        {
            for (IterationCount = 0; Best.Error > ErrorTolerance; IterationCount++)
                yield return IterationCount;
        }

    }
}
