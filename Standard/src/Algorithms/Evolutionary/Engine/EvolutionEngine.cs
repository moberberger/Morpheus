#nullable disable

namespace Morpheus.Evolution;

public class EvolutionEngine<TChromosome>
    where TChromosome : class
{
    public delegate float FnEvolver( TChromosome evolveInto, Func<double, TChromosome> sampler );

    private float[] sampleDeviations;
    private float[] sampleDeviationsSums1;
    private float[] sampleDeviationsSums2;
    private float[] sampleDeviationsSums3;

    private int baseIndex = -1;
    private int bestIndex = -1;
    private float minDeviation = float.MaxValue;
    private float maxDeviation = float.MinValue;
    private float sumDeviations = 0;


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






    public EvolutionEngine( int populationSize, FnEvolver evolver )
    {
        Evolver = evolver;
        Reset( populationSize );
    }

    public EvolutionEngine( int populationSize, Func<TChromosome> chromosomeGenerator, FnEvolver evolver )
    {
        Evolver = evolver;
        Reset( populationSize, chromosomeGenerator );
    }

    public EvolutionEngine( TChromosome[] doubleInitialPopulation, FnEvolver evolver )
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

        sampleDeviations = new float[PopulationSize];
        sampleDeviationsSums1 = new float[PopulationSize >> 6];
        sampleDeviationsSums2 = new float[PopulationSize >> 3];
        sampleDeviationsSums3 = new float[PopulationSize];

        sumDeviations = 0;
        for (int i = 0; i < PopulationSize; i++)
        {
            var dev = Evolver( Population[i], null );
            sampleDeviations[i] = dev;
            sumDeviations += dev;
            minDeviation = Math.Min( minDeviation, dev );
            maxDeviation = Math.Max( maxDeviation, dev );
        }

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
    public void ProcessSampleSet()
    {
        int endIndex = baseIndex + PopulationSize;
        float sum = 0.0f;
        for (int i = baseIndex; i < endIndex; i++)
        {
            float dev = sampleDeviations[i];

            float x = maxDeviation / dev;

            sum += x;

            sampleDeviationsSums3[i] = sum;
            sampleDeviationsSums2[i >> 3] = sum;
            sampleDeviationsSums1[i >> 6] = sum;
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
        float selection = (float)_selection * sumDeviations;

        while (low < high)
        {
            int mid = low + (high - low) / 2; // assumed const-2 optimized to shift in JIT

            if (selection > sampleDeviationsSums3[mid])
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

        minDeviation = float.MaxValue;
        maxDeviation = float.MinValue;
        sumDeviations = 0;

        // Elitism
        if (UseElitism)
        {
            Population.SwapElements( bestIndex, start );
            bestIndex = start;
            sampleDeviations[start] =
                minDeviation =
                maxDeviation =
                sumDeviations = Evolver( Population[start], null );

            start++;
        }

        // Generation
        for (int i = start; i < end; i++)
        {
            var output = Population[i];
            var deviation = Evolver( output, Sample );
            sampleDeviations[i] = deviation;

            sumDeviations += deviation;

            if (deviation > maxDeviation)
                maxDeviation = deviation;

            if (deviation < minDeviation)
            {
                minDeviation = deviation;
                bestIndex = i;
            }
        }

        // Swap base index
        baseIndex = PopulationSize - baseIndex;

        // Make sure new SampleSet is ready for a binary search
        ProcessSampleSet();
        IterationCount++;

        return Best;
    }
}
