using System;
using System.Collections.Generic;
using System.Linq;

namespace Morpheus.ProbabilityGeneratorNS
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Chromosome : IComparable<Chromosome>, IComparable
    {
        /// <summary>
        /// Allows C/Asm to effect polymorphism
        /// </summary>
        protected readonly int Version = 0;

        /// <summary>
        /// Construct in subclass
        /// </summary>
        /// <param name="version"></param>
        protected Chromosome( int version ) => Version = version;

        /// <summary>
        /// The best deviation found by the algorithm prior to fix-up. Just because there's a
        /// non-zero Deviation does not mean that the probabilities are valid.
        /// </summary>
        public double Deviation { get; set; } = double.NaN;

        /// <summary>
        /// Copy the data from another Output object
        /// </summary>
        /// <param name="other"></param>
        public virtual void CopyFrom( Chromosome other )
        {
            if (other == null)
                throw new ArgumentNullException( "other" );
            if (Version != other.Version )
                throw new ArgumentException( $"The chromosome versions are different: this={Version}, other={other.Version}" );

            Deviation = other.Deviation;
        }

        /// <summary>
        /// The deviation is what determines order
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo( Chromosome other ) => Math.Sign( Deviation - other.Deviation );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        int IComparable.CompareTo( object obj ) => CompareTo( (obj as Chromosome) ?? throw new ArgumentException( $"Invalid type: {obj.GetType()}" ) );

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"dev: {Deviation:N6}";

    }

}
