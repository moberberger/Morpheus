using System;
using System.Collections.Generic;
using System.Linq;

namespace Morpheus
{
    public static class ArrayVectorDouble
    {
        public static void tNormalize( this double[] v )
        {
            var mag = v.Magnitude();
            for (int i = 0; i < v.Length; i++)
                v[i] /= mag;
        }

        public static void ChangeToProbabilities( this double[] v )
        {
            double sum = 0;
            for (int i = 0; i < v.Length; i++) 
                sum += v[i];
            for (int i = 0; i < v.Length; i++)
                v[i] /= sum;
        }

        public static double Magnitude( this double[] v ) => Math.Sqrt( v.MagnitudeSquared() );

        public static double MagnitudeSquared( this double[] v )
        {
            double sum = 0;

            for (int i = 0; i < v.Length; i++)
                sum += v[i] * v[i];

            return sum;
        }

        public static double DotProduct( this double[] v1, double[] v2 )
        {
            double sum = 0;

            int smallerLength = Math.Min( v1.Length, v2.Length );
            for (int i = 0; i < smallerLength; i++)
                sum += v1[i] * v2[i];

            return sum;
        }
    }
}
