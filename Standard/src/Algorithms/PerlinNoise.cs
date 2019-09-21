using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// The delegate that is used to reliably generate a pseudo-random value for a given point
    /// in 2d space given a particular seed
    /// </summary>
    /// <param name="_x">The X coord</param>
    /// <param name="_y">The Y coord</param>
    /// <param name="_noiseSeed">The Seed to use for PRNG generation</param>
    /// <returns>
    /// A floating point pseudo random number which will always be identical for the same three
    /// parameter X, Y, and Seed
    /// </returns>
    /// <remarks>
    /// You should probably use <see cref="OpenSimplexNoise"/> instead of this... just sayin'
    /// </remarks>
    public delegate double DPerlinNoise( int _x, int _y, int _noiseSeed );

    /// <summary>
    /// The delegate for a function to interpolate between two values
    /// </summary>
    /// <param name="_1">One of the values to interpolate between</param>
    /// <param name="_2">One of the values to interpolate between</param>
    /// <param name="_fraction">
    /// How far between the two values the function should generate a value for
    /// </param>
    /// <returns>
    /// A value "between" _1 and _2. "Between" doesn't necessarily mean greater than one and
    /// less than the other.
    /// </returns>
    public delegate double DInterpolation( double _1, double _2, double _fraction );

    /// <summary>
    /// Create Perlin Noise for a 2d field.
    /// </summary>
    public class PerlinNoise2d
    {
        /// <summary>
        /// For each Octave, determines how much of that Octave's noise is present in the final
        /// noise value
        /// </summary>
        public double Persistence = 0.25;

        /// <summary>
        /// The number of times noise is generated for a given point. Successive generations of
        /// noise count less towards the overall noise.
        /// </summary>
        public int Octaves = 6;

        /// <summary>
        /// How to generate noise
        /// </summary>
        public DPerlinNoise PerlinNoiseFunction;
        /// <summary>
        /// How to interpolate between two points
        /// </summary>
        public DInterpolation InterpolationFunction;

        /// <summary>
        /// The Seed to use for the Noise
        /// </summary>
        public int NoiseSeed = 0;

        /// <summary>
        /// Width and Height are used to create tiled noise patterns.
        /// </summary>
        public int Width = int.MaxValue;

        /// <summary>
        /// Width and Height are used to create tiled noise patterns.
        /// </summary>
        public int Height = int.MaxValue;


        /// <summary>
        /// Construct with default parameters
        /// </summary>
        public PerlinNoise2d()
        {
            Init();
        }

        /// <summary>
        /// Construct with a Persistence and Octaves value
        /// </summary>
        /// <param name="_persistence">The Persistence to use</param>
        /// <param name="_octaves">The Octaves to use</param>
        public PerlinNoise2d( double _persistence, int _octaves )
        {
            Persistence = _persistence;
            Octaves = _octaves;
            Init();
        }

        /// <summary>
        /// Initialize to default values
        /// </summary>
        private void Init()
        {
            PerlinNoiseFunction = Noise;
            InterpolationFunction = CosineInterpolation;
        }

        /// <summary>
        /// The Default function for noice generation. Uses the Knuth RNG algorithm.
        /// </summary>
        /// <param name="_x">The X coord</param>
        /// <param name="_y">The Y coord</param>
        /// <param name="_noiseSeed">The Seed to use for PRNG generation</param>
        /// <returns>
        /// A floating point pseudo random number which will always be identical for the same
        /// three parameter X, Y, and Seed
        /// </returns>
        public double Noise( int _x, int _y, int _noiseSeed )
        {
            var x = _x % Width;
            if (x < 0)
                x += Width;
            var y = _y % Height;
            if (y < 0)
                y += Height;

            var n = x + y * 57;
            n ^= _noiseSeed;
            n = (n << 13) ^ n;
            return 1.0 - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0;
        }

        /// <summary>
        /// The default interpolation between two values. Uses Cosine interpolation.
        /// </summary>
        /// <param name="_1">One of the values to interpolate between</param>
        /// <param name="_2">One of the values to interpolate between</param>
        /// <param name="_fraction">
        /// How far between the two values the function should generate a value for
        /// </param>
        /// <returns>
        /// A value "between" _1 and _2. "Between" doesn't necessarily mean greater than one and
        /// less than the other, but in this case is is.
        /// </returns>
        public double CosineInterpolation( double _1, double _2, double _fraction )
        {
            var tmp = _fraction * Math.PI;
            var tmp2 = (1 - Math.Cos( tmp )) * 0.5;
            return _1 * (1 - tmp2) + _2 * tmp2;
        }

        /// <summary>
        /// Generate a "smoothed" noise value for integer coordinates (x,y).
        /// </summary>
        /// <remarks>
        /// Smoothed means that the center noise function value counts the most, the N,E,S,W
        /// coords count half as much, and the corners (NW, NE, SW, SE) count one quarter as
        /// much
        /// </remarks>
        /// <param name="_x">One coord to get noise from</param>
        /// <param name="_y">One coord to get noise from</param>
        /// <returns>Smoothed noise at the coordinate</returns>
        public double SmoothedNoise( int _x, int _y )
        {
            var corners = (PerlinNoiseFunction( _x - 1, _y - 1, NoiseSeed ) + PerlinNoiseFunction( _x + 1, _y - 1, NoiseSeed ) + PerlinNoiseFunction( _x - 1, _y + 1, NoiseSeed ) + PerlinNoiseFunction( _x + 1, _y + 1, NoiseSeed )) / 16;
            var sides = (PerlinNoiseFunction( _x - 1, _y, NoiseSeed ) + PerlinNoiseFunction( _x + 1, _y, NoiseSeed ) + PerlinNoiseFunction( _x, _y - 1, NoiseSeed ) + PerlinNoiseFunction( _x, _y + 1, NoiseSeed )) / 8;
            var center = PerlinNoiseFunction( _x, _y, NoiseSeed ) / 4;
            return corners + sides + center;
        }

        /// <summary>
        /// Given smoothed noise at the four integer-aligned points surrounding a floating-point
        /// coordinate, interpolate what the noise would look like somewhere within the "square"
        /// with corners set on integer coordinate boundaries surrounding the (x,y) parameter
        /// values.
        /// </summary>
        /// <param name="_x">One coord to get noise from</param>
        /// <param name="_y">One coord to get noise from</param>
        /// <returns>
        /// Noise at the coordinate interpolated from smoothed noise calculated at four points
        /// surrounding the coordinate
        /// </returns>
        public double InterpolatedNoise( double _x, double _y )
        {
            var iX = (int) _x;
            var iY = (int) _y;
            var fracX = _x - iX;
            var fracY = _y - iY;

            var v1 = SmoothedNoise( iX, iY );
            var v2 = SmoothedNoise( iX + 1, iY );
            var v3 = SmoothedNoise( iX, iY + 1 );
            var v4 = SmoothedNoise( iX + 1, iY + 1 );

            var i1 = InterpolationFunction( v1, v2, fracX );
            var i2 = InterpolationFunction( v3, v4, fracX );

            return InterpolationFunction( i1, i2, fracY );
        }

        /// <summary>
        /// Calculate the perlin noise at a given point based on the Octaves and Persistence
        /// values established.
        /// </summary>
        /// <param name="_x">One coord to get noise from</param>
        /// <param name="_y">One coord to get noise from</param>
        /// <returns>The Perlin Noise at a floating-point coordinate</returns>
        public double PerlinNoise( double _x, double _y )
        {
            double total = 0;

            for (var i = 0; i < Octaves; i++)
            {
                double frequency = 1 << i;
                var amplitude = Math.Pow( Persistence, i );

                total += InterpolatedNoise( _x * frequency, _y * frequency ) * amplitude;
            }

            return total;
        }
    }
}
