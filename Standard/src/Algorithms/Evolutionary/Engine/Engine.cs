using System;
using System.Linq;
using System.Threading;

namespace Morpheus.Evolution
{
    public class Engine<TChromosome, TInputType, TDeviationDetailType>
        where TChromosome : Chromosome
        where TInputType : class
        where TDeviationDetailType : class
    {
        public delegate TChromosome FnDeviationCalculator( TInputType input, TChromosome chromo, TDeviationDetailType detail );
        public delegate void FnEvolver( Func<TChromosome> sampler, TChromosome evolveInto );
        public delegate TChromosome FnChromosomeCreator( TInputType input, bool initialized );



        public readonly LCPRNG Rng = new LCPRNG_MMIX();

        private float[] sampleDeviationsSums;


        public bool UseElitism { get; set; } = true;

        /// <summary>
        /// Marginally most efficient as an exponent of 2
        /// </summary>
        public int PopulationSize { get; private set; }

        public TInputType InputConfig { get; set; }

        public FnDeviationCalculator DeviationFunction { get; set; }

        public FnEvolver Evolver { get; set; }

        public FnChromosomeCreator ChromosomeCreator { get; set; }



        public int IterationCount { get; private set; }

        public TChromosome Best { get; private set; }

        public TChromosome[] SampleSet { get; private set; }

        public TChromosome[] ResultSet { get; private set; }


        protected Engine()
        {
            Rng.UseFastScale = true;
        }

        public Engine( int populationSize, TInputType input, FnDeviationCalculator deviationFunction, FnEvolver evolver, FnChromosomeCreator chromosomeCreator )
            : this()
        {
            InputConfig = input ?? throw new ArgumentNullException( "input" );
            DeviationFunction = deviationFunction ?? throw new ArgumentNullException( "deviationFunction" );
            Evolver = evolver ?? throw new ArgumentNullException( "evolver" ); ;
            ChromosomeCreator = chromosomeCreator ?? throw new ArgumentNullException( "chromosomeCreator" ); ;

            Resize( populationSize );
        }


        /// <summary>
        /// Reallocates and Initializes (where appropriate) a new population of chromosomes
        /// while keeping the specified deviation, evolution and creation functions
        /// </summary>
        /// <param name="newPopulationSize"></param>
        /// <returns></returns>
        public int Resize( int newPopulationSize )
        {
            var retval = PopulationSize;
            PopulationSize = newPopulationSize;

            SampleSet = new TChromosome[PopulationSize];
            ResultSet = new TChromosome[PopulationSize];
            sampleDeviationsSums = new float[PopulationSize];

            Initialize();

            return retval;
        }

        /// <summary>
        /// Resets the populations to initial (randomized) values. Reallocates all chromosomes
        /// using the <see cref="ChromosomeCreator"/> . Does not reallocate internal arrays.
        /// </summary>
        public void Initialize()
        {
            for (int i = 0; i < PopulationSize; i++)
            {
                var chromo = ChromosomeCreator( InputConfig, true );
                DeviationFunction( InputConfig, chromo, default );
                SampleSet[i] = chromo;

                ResultSet[i] = ChromosomeCreator( InputConfig, false );
            }
            Best = ChromosomeCreator( InputConfig, false );

            // Make sure new SampleSet is ready for a binary search
            ProcessSampleSet();
            IterationCount = 0;
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
            var best = SampleSet[0];
            float max = best.Deviation;

            for (int i = 1; i < PopulationSize; i++)
            {
                var dev = SampleSet[i].Deviation;
                if (dev > max)
                    max = dev;
                if (dev < best.Deviation)
                    best = SampleSet[i];
            }
            best.CopyTo( Best );

            float sum = 0;
            for (int i = 0; i < PopulationSize; i++)
            {
                var dev = SampleSet[i].Deviation;
                var x = max / dev;
                sum += x;
                sampleDeviationsSums[i] = sum;
            }
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
            // Elitism
            if (UseElitism)
                Best.CopyTo( ResultSet[0] );

            // Generation
            for (int i = UseElitism ? 1 : 0; i < PopulationSize; i++)
            {
                // start can be parallel

                var output = ResultSet[i];
                Evolver( Sample, output );
                DeviationFunction( InputConfig, output, default );

                // end can be parallel
            }

            // swap lists
            var tmp = SampleSet;
            SampleSet = ResultSet;
            ResultSet = tmp;

            // Make sure new SampleSet is ready for a binary search
            ProcessSampleSet();
            IterationCount++;

            return Best;
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
        public TChromosome Sample()
        {
            int low = 0;
            int mid = 0;
            int high = PopulationSize;

            var max = sampleDeviationsSums.Last();
            var selection = Rng.NextDouble() * max;

            while (low < high)
            {
                mid = low + (high - low) / 2;

                if (selection > sampleDeviationsSums[mid])
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return SampleSet[low];
        }

    }
}
