namespace Morpheus;

public class GoalSeek
{
    public static double Simple( Func<double, double> function, double target, double minimum, double maximum )
    {
        double low = Math.Min( minimum, maximum );
        double high = Math.Max( minimum, maximum );

        double x = (high + low) / 2;
        double fnx = function( x );

        // Set up for a second "probing" function() call to see if function is inverted
        double low2, high2;
        if (fnx < target)
        {
            low2 = x;
            high2 = high;
        }
        else
        {
            low2 = low;
            high2 = x;
        }

        double x2 = (high2 + low2) / 2;
        double fnx2 = function( x2 );

        // Test to see if the function is inverted. If it is, then the first comparison /
        // setting of high and low are invalid, and need to be reversed. If it is not inverted,
        // then use the output of the probing function() call to set the new parameters for
        // high/low.
        double direction;
        if ((fnx < target && fnx2 > fnx) || (fnx > target && fnx2 < fnx))
        {
            direction = 1; // The function() is not inverted
            if (fnx2 < target) // fnx2 is valid
            {
                low = x2;
                high = high2;
            }
            else
            {
                low = low2;
                high = x2;
            }
        }
        else // inverted... fnx2 is invalid
        {
            direction = -1;
            if (fnx < target) // the opposite of what we did up top
                high = x;
            else
                low = x;
        }

        // Now proceed with a loop now that we know -direction-
        double dTarget = target * direction;
        double lastX = double.NaN;
        for (int i = 0; i < 10000; i++)
        {
            x = (high + low) / 2;
            if (x == lastX) // ran out of precision- this is as 
                return x; // good as it gets with a double precision number
            lastX = x;

            fnx = function( x ) * direction;
            if (fnx < dTarget)
                low = x;
            else
                high = x;
        }

        // should never happen
        throw new InvalidProgramException( "This should never happen, as the precision of a double should make the loop " +
            "terminate within 56-57 iterations" );
    }
}
