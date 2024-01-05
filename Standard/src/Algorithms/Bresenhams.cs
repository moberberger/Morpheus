using System.Collections;

namespace Morpheus;

public class Bresenhams : IEnumerable<(int, int)>
{
    private int x0, y0, x1, y1, dx, sx, dy, sy, error;

    public Bresenhams( int _x0, int _y0, int _x1, int _y1 )
    {
        x0 = _x0;
        y0 = _y0;
        x1 = _x1;
        y1 = _y1;

        dx = Math.Abs( x1 - x0 );
        sx = x0 < x1 ? 1 : -1;
        dy = -Math.Abs( y1 - y0 );
        sy = y0 < y1 ? 1 : -1;
        error = dx + dy;
    }

    public IEnumerable<(int, int)> Pixels()
    {
        do
        {
            yield return (x0, y0);

            var e2 = 2 * error;
            if (e2 >= dy && x0 != x1)
            {
                error += dy;
                x0 += sx;
            }
            if (e2 <= dx && y0 != y1)
            {
                error += dx;
                y0 += sy;
            }
        } while (x0 != x1 || y0 != y1);
        yield return (x0, y0);
    }


    public IEnumerator<(int, int)> GetEnumerator() => Pixels().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static void Test()
    {
        foreach (var pt in new Bresenhams( 0, 0, 18, 7 ).Pixels())
            Console.WriteLine( pt );
        foreach (var pt in new Bresenhams( 5, 11, 0, 0 ))
            Console.WriteLine( pt );
    }
}
