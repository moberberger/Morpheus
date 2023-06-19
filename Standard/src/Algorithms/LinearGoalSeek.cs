namespace Morpheus;

public class LinearGoalSeek
{
    public static double GoalSeek( Func<double, double> function, double target, double minimum, double maximum, int iterations = 64 )
    {
        double low = minimum;
        double high = maximum;
        double x = double.NaN;
        double lastX = x;

        for (int i = 0; i < iterations; i++)
        {
            x = (high + low) / 2;
            if (x == lastX)
                return x;
            lastX = x;

            double fnx = function( x );

            if (fnx < target)
            {
                low = x;
            }
            else
            {
                high = x;
            }
        }
        return x;
    }
}
