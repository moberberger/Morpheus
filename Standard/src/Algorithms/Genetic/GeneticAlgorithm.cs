using System;
using System.Collections.Generic;
using System.Threading;

namespace Morpheus
{
    /// <summary>
    /// A generalized Genetic Algorithm. Uses the <see cref="Chromosome"/> class.
    /// </summary>
    public class GeneticAlgorithm
    {
        /// <summary>
        /// Percentage chance of a mutation occurring
        /// </summary>
        public double MutationRate = 0.05;

        /// <summary>
        /// Chance of additional mutated bits per mutation
        /// </summary>
        public double MutationStrength = 0.5;

        /// <summary>
        /// Set this to terminate a Run after a certain amount of time
        /// </summary>
        public DateTime StopTime = DateTime.MaxValue;

        /// <summary>
        /// The number of chromosomes in the pool. Ignored if set/modified after the GA is run.
        /// </summary>
        public int PoolSize = 100;

        /// <summary>
        /// How many of the top chromosomes will always make it into the next generation as-is
        /// </summary>
        public int ElitismCount = 1;




        /// <summary>
        /// When the worst chromosome error is more than this TIMES the best chromosome error,
        /// switch sampling algorithms. This number is a wild guess.
        /// </summary>
        private const double SAMPLING_HEURISTIC = 100000;

        /// <summary>
        /// A list containing all "good" chromosomes- these are used in crossover and mutation
        /// to move into the "New Pool"
        /// </summary>
        private List<Chromosome> m_fullPool;

        /// <summary>
        /// A list that contains "blank" chromosomes, that must be created using any of a
        /// variety of methods. This helps alleviate garbage collection by not allocating new
        /// chromosomes all the time.
        /// </summary>
        private List<Chromosome> m_workingPool;

        /// <summary>
        /// Reset at the start of every generation, this keeps track of any chromosomes that the
        /// application has injected into the generation via the <see cref="OnGenerationStart"/>
        /// event.
        /// </summary>
        private int m_workingIndex = 0;

        /// <summary>
        /// 
        /// </summary>
        private int m_lastUpdateCount;


        /// <summary>
        /// The template chromosome for the application
        /// </summary>
        private readonly Chromosome m_template;

        /// <summary>
        /// The current Generation that the GA is working on
        /// </summary>
        public int Generation { get; private set; }

        /// <summary>
        /// The maximum number of generations to evolve through. May be set during evolution by
        /// the Application via <see cref="GetWorkingChromosome"/>
        /// </summary>
        public int MaxGeneration { get; set; }

        /// <summary>
        /// The number of generations that have been processed since the last time an Update
        /// callback was performed.
        /// </summary>
        public int GenerationsSinceLastUpdate { get; private set; }

        /// <summary>
        /// The currently "full" pool of chromosomes that the application may access in order to
        /// inject new chromosomes (via <see cref="GetWorkingChromosome"/> )
        /// </summary>
        public List<Chromosome> Pool => m_fullPool;

        /// <summary>
        /// How often should the <see cref="OnUpdateStatus"/> event be called, in Milliseconds
        /// </summary>
        public int UpdateInterval = 1000;

        /// <summary>
        /// Called every so often for the purpose of allowing the application to update any
        /// status information, such as a status bar or printing current status of the GA. The
        /// application should not modify any of the GA parameters- This should be done inside
        /// the OnGenerationStart event handler.
        /// </summary>
        public event Action<GeneticAlgorithm> OnUpdateStatus;

        /// <summary>
        /// Allow the application to interfere at the beginning of each generation
        /// </summary>
        public event Action<GeneticAlgorithm> OnGenerationStart;

        /// <summary>
        /// Counter to communicate the number of Normal distributions (high difference in errors
        /// of best and worst chromosomes)
        /// </summary>
        /// <remarks>
        /// If someone wants to use these numbers to tune the SAMPLING_HEURISTIC they can. Right
        /// now, its constant, but it could be made into an instance variable easily enough.
        /// </remarks>
        public int NormalSampleCount { get; private set; }

        /// <summary>
        /// Counter to communicate the number of Linear distributions (low difference in errors
        /// of best and worst chromosomes)
        /// </summary>
        /// <remarks>
        /// If someone wants to use these numbers to tune the SAMPLING_HEURISTIC they can. Right
        /// now, its constant, but it could be made into an instance variable easily enough.
        /// </remarks>
        public int LinearSampleCount { get; private set; }


        /// <summary>
        /// Create a new GA using a specific chromosome as a template.
        /// </summary>
        /// <param name="_template">The template chromosome</param>
        public GeneticAlgorithm( Chromosome _template )
        {
            m_template = _template;
            Generation = -1;
        }

        /// <summary>
        /// Construct a new GA using data that describes the template chromosome
        /// </summary>
        /// <param name="_wordCount">The number of words in the chromosome</param>
        /// <param name="_bitsPerWord">The number of bits per word</param>
        /// <param name="_evaluator">
        /// The (presumably non-trivial) evaluator for the chromosome
        /// </param>
        public GeneticAlgorithm( int _wordCount, int _bitsPerWord, Func<Chromosome, double> _evaluator )
        {
            m_template = new Chromosome( _wordCount, _bitsPerWord, _evaluator );
            Generation = -1;
        }

        /// <summary>
        /// Grab the next uninitialized working chromosome. Return NULL when none are left.
        /// </summary>
        /// <returns>
        /// NULL if there are no more working chromosomes available, or a reference to an
        /// uninitialized chromosome if one is available.
        /// </returns>
        public Chromosome GetWorkingChromosome()
        {
            if (Generation < 0)
                throw new InvalidOperationException( "Not allowed to get working chromosomes until the GA is Run()" );

            if (m_workingIndex >= m_workingPool.Count)
                return null;

            return m_workingPool[m_workingIndex++];
        }

        /// <summary>
        /// Called by the application in the <see cref="OnGenerationStart"/> handler to
        /// terminate evolution immediately. This does NOT imply any sort of error- It may be
        /// called simply because the application believes that one of the chromosomes is "good
        /// enough" already.
        /// </summary>
        public void StopEvolving()
        {
            if (Generation < 0)
                throw new InvalidOperationException( "Not allowed to StopEvolving until the GA is Run()" );
            StopTime = DateTime.MinValue;
        }


        /// <summary>
        /// Run the GA for a specific number of generations.
        /// </summary>
        /// <param name="_generations">The number of generations to run for</param>
        public Chromosome Run( int _generations )
        {
            // Build a pool of random chromos
            m_fullPool = new List<Chromosome>();
            m_workingPool = new List<Chromosome>();
            MaxGeneration = _generations;

            // Generate the two pools of chromosomes
            for (var i = 0; i < PoolSize; i++)
            {
                var chromo = new Chromosome( m_template );
                chromo.Randomize();
                m_workingPool.Add( chromo ); // first operation in loop is to swap pools!

                chromo = new Chromosome( m_template );
                chromo.Randomize();
                m_fullPool.Add( chromo ); // Don't care about these counts right now
            }


            // Loop through the generations
            var updateTime = DateTime.Now;
            for (Generation = 0; Generation < MaxGeneration; Generation++)
            {
                m_workingIndex = 0;

                // Switch pools new pool by turning the current one into a working pool
                m_fullPool = m_fullPool.Swap( ref m_workingPool );

                // Sort the new pool (generated from previous generation's working pool)
                m_fullPool.Sort();

                // Let the application have its chance at the new working pool. This can be used
                // to implement special kinds of elitism or other operations which add
                // chromosomes to the working pool.
                OnGenerationStart?.Invoke( this ); // Assumes Application will sort pool properly if it changes sort order

                // If its time to end this, then do so now. If the application calls StopNow(),
                // this will be how we know to stop
                if (DateTime.Now >= StopTime)
                    break;

                // Update the Application if its time to do so. Assume that the application
                // doesn't modify anything in the GA during this event.
                if (DateTime.Now >= updateTime)
                {
                    GenerationsSinceLastUpdate = Generation - m_lastUpdateCount;
                    m_lastUpdateCount = Generation;
                    OnUpdateStatus?.Invoke( this );
                    updateTime += new TimeSpan( 0, 0, 0, 0, UpdateInterval );
                }

                Chromosome working;

                // Handle Elitism
                for (var i = 0; i < ElitismCount; i++)
                {
                    working = GetWorkingChromosome();
                    if (working != null) // may have run out already... that would be lame.
                        working.FromOther( m_fullPool[i] );
                    else
                        break;
                }

                // Handle Xover and Mutation
                while ((working = GetWorkingChromosome()) != null) // while there are working chromosomes to fill
                {
                    // Assumes that "value" is an error value, so use the "inverse" sampling to
                    // favor small values.
                    var first = Sample();

                    if (Rng.Default.NextDouble() < MutationRate) // mutation only
                    {
                        working.FromOther( first );
                        working.Mutate( MutationStrength );
                    }
                    else // crossover
                    {
                        var second = Sample();

                        if (Rng.Default.NextBool())
                            working.CrossoverSplice( first, second );
                        else
                            working.CrossoverBits( first, second );

                        if (Rng.Default.NextDouble() < MutationRate) // mutation
                        {
                            working.Mutate( MutationStrength );
                        }
                    }
                }
            }
            GenerationsSinceLastUpdate = Generation - m_lastUpdateCount;
            OnUpdateStatus?.Invoke( this );


            m_fullPool.Sort();
            // Return the "best" chromosome in the pool
            return m_fullPool[0];
        }

        /// <summary>
        /// Sample the Pool based on a heuristic choice of methods.
        /// 
        /// If the best and worst chromosomes are many magnitudes different than each other, the
        /// linear sampling method in MorpheusUtil will produce abnormally highly skewed values
        /// towards the best chromosomes due to the linear inverse method of error weighting
        /// that algorithm uses. In this case, a Gaussian (Normal) curve can be used to give a
        /// certain sanity to the selection.
        /// 
        /// When the best and worst are relatively close, however, there is little to
        /// differentiate the best from the worst. The Gaussian (Normal) approach would not
        /// recognize that, treating the worst as "much worst" compared to the best when in
        /// reality they are close. It is here that the linear method found in MorpheusUtil will
        /// work better.
        /// </summary>
        /// <returns>A sampled chromosome</returns>
        private Chromosome Sample()
        {
            var best = Pool[0].GetValue();
            var worst = Pool[Pool.Count - 1].GetValue();

            if (worst / best > SAMPLING_HEURISTIC) // Use the LARGE difference algorithm (Normal) 
            {
                NormalSampleCount++;
                while (true)
                {
                    var x = Rng.Default.NextGaussian( 0, Pool.Count / 2 ) + 0.5;
                    var idx = (int) Math.Abs( x );

                    // Roughly 21 of 22 of the samples should be in this range. 2 standard
                    // deviations of the Gaussian generated above will yield a value in this
                    // range.
                    if (idx < Pool.Count)
                        return Pool[idx];
                }
            }
            else
            {
                // Not really linear, because we're using the inverse. If an error is zero or
                // extremely small, this will never (rarely) select anything other than that
                // chromosome, which doesn't promote diversity. Elitism should guarantee those
                // chromosomes' survival, not skewed selection.
                LinearSampleCount++;
                return Pool.Sample( _c => _c.GetValue(), true );
            }
        }
    }
}
