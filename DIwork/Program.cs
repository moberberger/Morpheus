using Morpheus;

var di = DI.Default.New();

di.For<Random>().UseNewInstance<Random>();

var o1 = DI.Default.Get<Random>();
var o2 = di.Get<Random>();

Console.WriteLine( $"{o1.GetType()}    {o2.GetType()}" );

var o4 = DI.Default.Get<Rng>();
var o5 = di.Get<Rng>();
Console.WriteLine( o4.GetType() + "    " + o5.GetType() );

timetest( new XorShiftStar() );
timetest( new XorShift() );
timetest( new XorShift() );
timetest( new LCPRNG_MMIX() );
timetest( new Xoshiro() );
timetest( new RDRAND() );
Console.WriteLine();
Console.WriteLine();
timetest( new XorShiftStar() );
timetest( new XorShift() );
timetest( new XorShift() );
timetest( new LCPRNG_MMIX() );
timetest( new Xoshiro() );
timetest( new RDRAND() );

void timetest( Rng rng )
{
    using (_ = new SimpleTimer( $"Running {rng.GetType()}" ))
    {
        var nums = new double[10000000];
        for (int i = 0; i < nums.Length; i++)
            nums[i] = rng.NextDouble();
        var stats = nums.GetStats();
        var std = nums.StandardDeviation( stats.Mean );
        Console.WriteLine( $"Mean: {stats.Mean}  StdDev: {std}" );
    }
}

Console.WriteLine( "bye" );

