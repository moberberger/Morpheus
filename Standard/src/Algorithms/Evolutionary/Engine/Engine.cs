using System;
using System.Linq;
using System.Threading;

namespace Morpheus.Evolution
{
    public class Engine<TChromosome>
        where TChromosome : class
    {
        public delegate float FnEvolver( TChromosome evolveInto, Func<double, TChromosome> sampler );

        private float[] sampleDeviations;
        private float[] sampleDeviationsSums;

        private int baseIndex;
        private int bestIndex;


        /// <summary>
        /// When TRUE, the best chromosome from each generation is automatically moved into the
        /// next generation. When FALSE, the next generation may be worse than the current one.
        /// </summary>
        public bool UseElitism { get; set; } = true;

        /// <summary>
        /// The evolver used to both evolve and determine deviation.
        /// </summary>
        public FnEvolver Evolver { get; set; }

        /// <summary>
        /// Current iteration count within the evolution.
        /// </summary>
        public int IterationCount { get; private set; }


        /// <summary>
        /// References the currently best chromosome in the SamplePopulation
        /// </summary>
        public TChromosome Best => Population[bestIndex];

        /// <summary>
        /// Single piece of allocated memory- hopefully more CPU cache hits
        /// </summary>
        public TChromosome[] Population { get; private set; }

        /// <summary>
        /// The population size. The actual working population array is twice this size.
        /// </summary>
        public int PopulationSize => Population.Length >> 1;






        public Engine( int populationSize, FnEvolver evolver )
        {
            Evolver = evolver;
            Reset( populationSize );
        }

        public Engine( int populationSize, Func<TChromosome> chromosomeGenerator, FnEvolver evolver )
        {
            Evolver = evolver;
            Reset( populationSize, chromosomeGenerator );
        }

        public Engine( TChromosome[] doubleInitialPopulation, FnEvolver evolver )
        {
            Evolver = evolver;
            Reset( doubleInitialPopulation );
        }



        /// <summary>
        /// Reset assuming that we can use <see cref="Activator.CreateInstance(Type)"/> to
        /// generate chromosomes. Twice as many chromosomes will be generated than the
        /// PopulationSize provided.
        /// </summary>
        /// <param name="populationSize"></param>
        public void Reset( int populationSize ) =>
            Reset( populationSize, () => Activator.CreateInstance<TChromosome>() );

        /// <summary>
        /// Construct using a chromosome generator
        /// </summary>
        /// <param name="chromosomeGenerator"></param>
        /// <param name="evolver"></param>
        public void Reset( int populationSize, Func<TChromosome> chromosomeGenerator ) =>
            Reset( new TChromosome[populationSize * 2].Fill( chromosomeGenerator ) );

        /// <summary>
        /// Reset using pre-created chromosomes. Twice as many chromosomes must be provided to
        /// allow evolution to go from one population to the next.
        /// </summary>
        /// <param name="doubleInitialPopulation">
        /// PopulationCount * 2 <see cref="TChromosome"/> 's. The first half should be
        /// appropriately initialized. The second half probably don't need to be initialized to
        /// any values.
        /// </param>
        public void Reset( TChromosome[] doubleInitialPopulation )
        {
            Population = doubleInitialPopulation;
            int len = doubleInitialPopulation.Length;

            sampleDeviations = Enumerable.
                Range( 0, len ).
                Select( idx => Evolver( Population[idx], null ) ).
                ToArray();

            sampleDeviationsSums = new float[len];

            IterationCount = 0;
            baseIndex = 0;
            bestIndex = 0;

            ProcessSampleSet();
        }



        /// <summary>
        /// Primary function is to create the _sampleSums array so that the
        /// <see cref="SampleSet"/> can be searched using a simple binary search.
        /// 
        /// Secondarily finds the <see cref="Best"/> <see cref="Chromosome"/> in the
        /// <see cref="SampleSet"/> .
        /// </summary>
        /// <remarks>
        /// TODO: Refactor to handle a <see cref="Chromosome.Deviation"/> ==0 situation, which
        /// would <see cref="DivideByZeroException"/> now.
        /// 
        /// Propose to immediately return Best = First chromosome where Deviation ==0, leaving
        /// SampleSet in an invalid state. This is tail-wagging-dog justified by stating that
        /// there could not possibly be a better chromosome, therefore the sample set's state is
        /// irrelevant.
        /// 
        /// This of course leaves the debugging of the invalid SampleSet more interesting...
        /// </remarks>
        private void ProcessSampleSet()
        {
            float min = sampleDeviations[baseIndex], max = min;
            int endIndex = baseIndex + PopulationSize;
            bestIndex = baseIndex;

            // handed [0] in previous initialization
            for (int i = baseIndex + 1; i < endIndex; i++)
            {
                float dev = sampleDeviations[i];

                if (dev > max)
                    max = dev;

                if (dev < min)
                {
                    min = dev;
                    bestIndex = i;
                }
            }

            float sum = 0.0f;
            for (int i = baseIndex; i < endIndex; i++)
            {
                float dev = sampleDeviations[i];

                float x = max / dev;
                sum += x;

                sampleDeviationsSums[i] = sum;
            }
        }


        /// <summary>
        /// Callback provided to the Evolver when it needs to sample the set for a chromosome.
        /// 
        /// Executes in O(ln) time.
        /// 
        /// This routine is re-entrant, but depends on a stable SampleSet. The application must
        /// coordinate access to the SampleSet, as this routine ASSUMES stability for the sake
        /// of performance.
        /// </summary>
        /// <returns>A chromosome selected (pseudo)randomly from the set</returns>
        public TChromosome Sample( double _selection )
        {
            int low = baseIndex;
            int high = low + PopulationSize;
            float max = sampleDeviationsSums[high - 1];
            float selection = (float)_selection * max; // target for vectorization

            while (low < high)
            {
                int mid = low + (high - low) / 2; // assumed const-2 optimized to shift in JIT

                if (selection > sampleDeviationsSums[mid])
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return Population[low];
        }



        /// <summary>
        /// Advance the evolution through one generation.
        /// 
        /// If using Elitism, the return value's <see cref="Chromosome.Deviation"/> value will
        /// be no larger than the previous value returned by this method.
        /// </summary>
        /// <returns>
        /// The <see cref="Chromosome"/> indicative of the best/lowest
        /// <see cref="Chromosome.Deviation"/> value.
        /// </returns>
        public TChromosome EvolveOneGeneration()
        {
            int start = PopulationSize - baseIndex;
            int end = start + PopulationSize;

            // Elitism
            if (UseElitism)
                Population.SwapElements( bestIndex, start++ );

            // Generation
            for (int i = start; i < end; i++)
            {
                // start can be parallel

                var output = Population[i];
                var deviation = Evolver( output, Sample );
                sampleDeviations[i] = deviation;

                // end can be parallel
            }

            // Swap base index
            baseIndex = PopulationSize - baseIndex;

            // Make sure new SampleSet is ready for a binary search
            ProcessSampleSet();
            IterationCount++;

            return Best;
        }
    }
}
