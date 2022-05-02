using System;
using System.Drawing;

namespace Morpheus;
using static System.Math;


public struct ColorHsva
{
    public double h = 0, s = 0, v;
    public byte a;

    public ColorHsva( Color rgb )
    {
        a = rgb.A;
        double r = rgb.R / 255.0;
        double g = rgb.G / 255.0;
        double b = rgb.B / 255.0;

        double cMin = Min( Min( r, g ), b );
        double cMax = Max( Max( r, g ), b );
        double delta = cMax - cMin;

        if (cMax != 0)
            s = delta / cMax;

        h = 0;
        if (s != 0)
        {
            if (r == cMax)
                h = (g - b) / delta;
            else if (g == cMax)
                h = 2 + (b - r) / delta;
            else if (b == cMax)
                h = 4 + (r - g) / delta;

            h = h / 6 + 1;
            h -= Floor( h );
        }

        v = cMax;
    }

    public static implicit operator Color( ColorHsva hsv )
    {
        double r, g, b;
        if (hsv.s == 0)
        {
            r = g = b = hsv.v;
        }
        else
        {
            int i;
            double f, p, q, t;

            var h = hsv.h - Math.Floor( hsv.h );
            h *= 6;
            i = (int)Truncate( h );
            f = h - i;

            p = hsv.v * (1.0 - hsv.s);
            q = hsv.v * (1.0 - (hsv.s * f));
            t = hsv.v * (1.0 - (hsv.s * (1.0 - f)));

            switch (i)
            {
                case 0:
                    r = hsv.v;
                    g = t;
                    b = p;
                    break;

                case 1:
                    r = q;
                    g = hsv.v;
                    b = p;
                    break;

                case 2:
                    r = p;
                    g = hsv.v;
                    b = t;
                    break;

                case 3:
                    r = p;
                    g = q;
                    b = hsv.v;
                    break;

                case 4:
                    r = t;
                    g = p;
                    b = hsv.v;
                    break;

                default:
                    r = hsv.v;
                    g = p;
                    b = q;
                    break;
            }
        }

        return Color.FromArgb( (byte)(r * 255), (byte)(g * 255), (byte)(b * 255), hsv.a );
    }
}
