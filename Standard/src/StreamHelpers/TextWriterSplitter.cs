using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Morpheus
{
    /// <summary>
    /// A simple wrapper class around some number of other textwriters that will make sure all output that goes to one writer will
    /// go to all writers
    /// </summary>
    /// <remarks>
    /// 
    /// var outFile = File.CreateText( @"c:\t.txt" );
    /// 
    /// Console.SetOut( new CTextWriterSplitter( Console.Out, outFile ) );
    ///
    /// </remarks>
    public class TextWriterSplitter : TextWriter
    {
        /// <summary>
        /// The <see cref="TextWriter"/> list that receives all output
        /// </summary>
        private readonly IEnumerable<TextWriter> m_writers;

        /// <summary>
        /// Flag to make sure this is not Disposed multiple times
        /// </summary>
        private bool m_disposed = false;


        /// <summary>
        /// A new splitter class to send output to whatever TextWriters are specified in the constructor
        /// </summary>
        /// <param name="_writers">Any number of TextWriters, all of which will receive output</param>
        public TextWriterSplitter( params TextWriter[] _writers )
        {
            m_writers = _writers?.ToList() ?? throw new ArgumentNullException( string.Format( "Must specify non-null list of TextWriters" ) );
        }

        /// <summary>
        /// Make sure things like Files get flushed and closed. One of those rare instances when you need a finalizer.
        /// </summary>
        ~TextWriterSplitter()
        {
            Dispose( true );
        }

        /// <summary>
        /// Use the first textwriter's encoding unless there are no text writers; then use default but it doesn't really matter
        /// </summary>
        public override Encoding Encoding => m_writers?.FirstOrDefault().Encoding ?? Encoding.Default;

        /// <summary>
        /// The only method that must be implemented- all other methods in the bsae implementation use this at the end
        /// </summary>
        /// <param name="_char">A character to write</param>
        public override void Write( char _char )
        {
            foreach (var writer in m_writers)
                writer.Write( _char );
        }

        /// <summary>
        /// Call this if you do not want this TextWriter to dispose your other text writers.
        /// </summary>
        public void DontDispose() => m_disposed = true;

        /// <summary>
        /// This is called by the base class's Dispose() method, which implements IDisposable.
        /// Make sure that all writers attached to this thing get disposed when it gets disposed.
        /// </summary>
        /// <param name="_disposing">TRUE- Called by IDisposable.Dispose  FALSE- Called by finalizer.
        /// See <see cref="System.IO.TextWriter.Dispose(bool)"/></param>
        protected override void Dispose( bool _disposing )
        {
            if (!m_disposed)
            {
                m_disposed = true;
                try
                {
                    base.Dispose( _disposing ); // should actually do nothing according to Reflector
                }
                catch { };

                foreach (var writer in m_writers)
                {
                    try
                    {
                        writer.Dispose();
                    }
                    catch { }
                }
            }
        }
    }
}