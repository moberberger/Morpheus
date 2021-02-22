using System;

namespace Morpheus.Evolution
{
    /// <summary>
    /// Basic chromosome containing the all-important Deviation value. Also provides ordering
    /// based on said Deviation. Applications derive from this.
    /// </summary>
    public abstract class Chromosome : IComparable<Chromosome>, IComparable
    {
        /// <summary>
        /// The deviation found by the provided algorithm for the data found in the chromosome.
        /// </summary>
        public virtual double Deviation { get; set; } = double.NaN;

        /// <summary>
        /// Copy the data from another Output object
        /// </summary>
        /// <param name="other"></param>
        public virtual void CopyTo( Chromosome other )
        {
            if (other == null)
                throw new ArgumentNullException( "other" );

            other.Deviation = Deviation;
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
        public override string ToString() => $"Dev={Deviation:N6}";

    }

}
