namespace Morpheus;

public class GoalSeek
{
    public static double Simple( Func<double, double> function, double target, double minimum, double maximum )
    {
        double low = Math.Min( minimum, maximum );
        double high = Math.Max( minimum, maximum );
        double step = (high - low) / 3;
        double left = low + step;
        double right = low + step * 2;

        double leftX = function( left );
        double rightX = function( right );
        double direction = Math.Sign( rightX - leftX );

        double dTarget = target * direction;
        double x, lastX = double.NaN;



        for (int i = 0; i < 1000; i++)
        {
            x = (high + low) / 2;
            if (x == lastX) // ran out of precision- this is as 
                return x; // good as it gets with a double precision number
            lastX = x;

            double fnx = function( x ) * direction;
            if (fnx < dTarget)
                low = x;
            else
                high = x;
        }
        throw new InvalidProgramException( "This should never happen, as the precision of a double should make the loop " +
            "terminate within 56-57 iterations" );
    }
}
