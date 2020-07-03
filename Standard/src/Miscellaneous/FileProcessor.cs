using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Morpheus
{
    /// <summary>
    /// This class allows an application to "process" a file concurrently with reading it. It assumes 
    /// that the application needs to deal with VERY LARGE FILES, otherwise there is no benefit.
    /// </summary>
    public class FileProcessor
    {
        /// <summary>
        /// Delegate matching <see cref="Stream.Write(byte[], int, int)"/> for processing bytes read from the file
        /// </summary>
        /// <param name="_buffer">The buffer containing the data</param>
        /// <param name="_offset">The starting point in the buffer where the data resides. When called by CFileProcessor,
        /// this value is always zero</param>
        /// <param name="_count">The number of bytes found in _buffer. DO NOT USE _buffer.Length to get the byte count!</param>
        public delegate void DFileChunkProcessor( byte[] _buffer, int _offset, int _count );

        /// <summary>
        /// Called by the processor when it sees an exception has been raised during async processing.
        /// </summary>
        /// <param name="_exception">The exception that was thrown- this will be called on a threadpool thread most likely</param>
        public delegate void DExceptionHandler( Exception _exception );

        /// <summary>
        /// The default buffer size for this class
        /// </summary>
        public const int DEFAULT_BUFFER_SIZE = 1024 * 1024 * 16;

        /// <summary>
        /// Used for the "synchronous" method to sync up to the end of the file processing
        /// </summary>
        private readonly ManualResetEvent m_stopped = new ManualResetEvent( false );

        /// <summary>
        /// The buffers used by the object to [0] read data from the file, and [1] process data that has been read
        /// </summary>
        private readonly byte[][] m_buffers = new byte[2][];
        /// <summary>
        /// Indicator as to which of the two buffers is being read into and which is being processed
        /// </summary>
        private int m_currentBufferIndex;
        /// <summary>
        /// The input stream of data for this class to process.
        /// </summary>
        private Stream m_inputStream;

        /// <summary>
        /// Used to make sure that this object is finished with one processing task before another one starts
        /// </summary>
        private int m_isProcessing = 0;


        /// <summary>
        /// The size of the buffer for an object of this class. Each instance gets its own 2 copies of a buffer of this size.
        /// </summary>
        private int m_bufferSize = DEFAULT_BUFFER_SIZE;

        /// <summary>
        /// The size of the buffer for an object of this class. Each instance gets its own 2 copies of a buffer of this size.
        /// The buffer is allocated when it is needed, not when this value is set.
        /// </summary>
        public int InternalBufferSize
        {
            get => m_bufferSize;
            set
            {
                m_bufferSize = value;
                m_buffers[0] = m_buffers[1] = null;
            }
        }

        /// <summary>
        /// The number of bytes that have been read
        /// </summary>
        public long Count { get; private set; }

        /// <summary>
        /// The number of "chunks" (full-or-partial) buffers read (but not necessarily processed)
        /// </summary>
        public int ChunksRead { get; private set; }

        /// <summary>
        /// Based on the <see cref="Stream.Length"/> value, the number of chunks that is expected to be
        /// read from the file
        /// </summary>
        public int EstimatedChunks { get; private set; }

        /// <summary>
        /// An exception thrown within the async completion routine of the file processor.
        /// </summary>
        public Exception AsyncException { get; private set; }

        /// <summary>
        /// Indicates whether the IO completion caused the exception, or if the app-processor caused it.
        /// </summary>
        private bool m_hasIoException;

        /// <summary>
        /// If TRUE, then there WAS an exception and it was caused by the Async IO completion
        /// </summary>
        public bool HasIoException => (AsyncException != null) && m_hasIoException;

        /// <summary>
        /// If TRUE, then there WAS an exception and it was caused by the application's Processing method
        /// </summary>
        public bool HasProcessingException => (AsyncException != null) && !m_hasIoException;


        /// <summary>
        /// Internal helper to access the "current buffer"- the one that has just been "filled up" with data from a stream read
        /// </summary>
        private byte[] CurrentBuffer => m_buffers[m_currentBufferIndex];

        /// <summary>
        /// Internal helper to access the "other buffer"- the one that should be filled with file data asynchronously
        /// </summary>
        private byte[] OtherBuffer => m_buffers[1 - m_currentBufferIndex];

        /// <summary>
        /// Swap the buffers for the next async file completion
        /// </summary>
        private void SwapBuffers() => m_currentBufferIndex = 1 - m_currentBufferIndex;


        /// <summary>
        /// Make sure the finisher disposes whatever this references.
        /// </summary>
        private IDisposable m_disposeOnFinished = null;


        /// <summary>
        /// Called before the first file i/o is started
        /// </summary>
        public event Action OnInitialize;

        /// <summary>
        /// Called after the last "chunk" of data has been read AND processed
        /// </summary>
        public event Action OnFinished;

        /// <summary>
        /// Called for each "chunk" of data
        /// </summary>
        public event DFileChunkProcessor OnProcessBytes;

        /// <summary>
        /// Called if an exception was caught during async file processing
        /// </summary>
        public event DExceptionHandler OnException;


        /// <summary>
        /// Regardless of what method is used to kick off file processing, this method should be one of the first things
        /// called by the method, BEFORE any event handling.
        /// </summary>
        private void InitializeProcessor()
        {
            var isProcessing = Interlocked.Increment( ref m_isProcessing );
            if (isProcessing != 1)
            {
                Interlocked.Decrement( ref m_isProcessing );
                throw new InvalidOperationException( "Not allowed to start a new 'run' of the file processor until all previous runs have completed." );
            }

            Count = 0;
            m_currentBufferIndex = 0;
            m_stopped.Reset();
            ChunksRead = 0;
            EstimatedChunks = (int) ((m_inputStream.Length - 1) / m_bufferSize + 1);
            AsyncException = null;

            if (m_buffers[0] == null || m_buffers[0].Length != m_bufferSize)
            {
                m_buffers[0] = new byte[m_bufferSize];
                m_buffers[1] = new byte[m_bufferSize];
            }
        }

        /// <summary>
        /// Given the name of a file, process that file
        /// </summary>
        /// <param name="_filename">The name of the file to process</param>
        /// <param name="_waitForCompletion">If TRUE, this routine will wait for all file processing to complete before 
        /// returning. It will still use Async I/O.</param>
        /// <returns>the number of bytes found in the stream</returns>
        public long ProcessFile( string _filename, bool _waitForCompletion )
        {
            var fs = new FileStream( _filename, FileMode.Open, FileAccess.Read, FileShare.Read, m_bufferSize, true );
            m_disposeOnFinished = fs;
            return ProcessFile( fs, _waitForCompletion );
        }

        /// <summary>
        /// Given a stream assumed to support asynchronous I/O, process that stream
        /// </summary>
        /// <param name="_inputStream">The stream to process</param>
        /// <param name="_waitForCompletion">If TRUE, this routine will wait for all file processing to complete before 
        /// returning. It will still use Async I/O.</param>
        /// <returns>The number of bytes found in the stream</returns>
        public long ProcessFile( Stream _inputStream, bool _waitForCompletion )
        {
            m_inputStream = _inputStream;

            InitializeProcessor(); // Internal initialization
            Initialize(); // Call derived class's initialization
            OnInitialize?.Invoke();

            m_inputStream.BeginRead( CurrentBuffer, 0, m_bufferSize, AsyncHandler, null );

            if (_waitForCompletion)
                m_stopped.WaitOne();

            return Count;
        }

        /// <summary>
        /// This is the "main" handler, which completes file i/o and handles processing of the bytes.
        /// </summary>
        /// <param name="_result">From the Async I/O library</param>
        private void AsyncHandler( IAsyncResult _result )
        {
            lock (this)
            {
                if (AsyncException != null) // If there is an exception registered, do not handle ANY processing.
                {
                    m_inputStream.EndRead( _result );
                    return;
                }

                var bytesRead = CompleteIo( _result );
                if (bytesRead == 0)
                {
                    FinishProcessing();
                    return;
                }

                ChunksRead++;

                Count += bytesRead;

                if (!StartNextIo())
                {
                    FinishProcessing();
                    return;
                }

                HandleProcessing( bytesRead );

                SwapBuffers();
            }
        }

        /// <summary>
        /// Call the appropriate processors and handle exceptions properly
        /// </summary>
        /// <param name="_bytesRead">The number of bytes to process</param>
        private void HandleProcessing( int _bytesRead )
        {
            try
            {
                ProcessBytes( CurrentBuffer, 0, _bytesRead );
                OnProcessBytes?.Invoke( CurrentBuffer, 0, _bytesRead );
            }
            catch (Exception e)
            {
                AsyncException = e;
                m_hasIoException = false;
                FinishProcessing();
            }
        }

        /// <summary>
        /// Complete the pending async IO and handle exceptions properly
        /// </summary>
        /// <param name="_result">The IAsyncResult identifying the IO request</param>
        /// <returns>The number of bytes read, or 0 if EOF or an exception was thrown</returns>
        private int CompleteIo( IAsyncResult _result )
        {
            int bytesRead;
            try
            {
                bytesRead = m_inputStream.EndRead( _result );
            }
            catch (Exception e)
            {
                bytesRead = 0;
                AsyncException = e;
                m_hasIoException = true;
            }
            return bytesRead;
        }

        /// <summary>
        /// Begin the next Io operation, and handle exceptions properly
        /// </summary>
        /// <returns>TRUE if the IO operation started successfully, or FALSE if a problem was encountered</returns>
        private bool StartNextIo()
        {
            var success = true;
            try
            {
                m_inputStream.BeginRead( OtherBuffer, 0, m_bufferSize, AsyncHandler, null );
            }
            catch (Exception e)
            {
                AsyncException = e;
                m_hasIoException = true;
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Called when the Async handling routine determines that there is no more file to process.
        /// </summary>
        private void FinishProcessing()
        {
            try
            {
                if (AsyncException != null && OnException != null)
                    OnException( AsyncException );

                Finished();
                OnFinished?.Invoke();
            }
            catch { }

            if (m_disposeOnFinished != null)
            {
                m_disposeOnFinished.Dispose();
                m_disposeOnFinished = null;
            }

            m_stopped.Set();
            Interlocked.Decrement( ref m_isProcessing );
        }



        /// <summary>
        /// Can be overridden by inherting class to do initialization stuff. Base does nothing.
        /// </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// Can be overridden by inherting class to do processing stuff. Base does nothing.
        /// </summary>
        protected virtual void ProcessBytes( byte[] _buffer, int _offset, int _count )
        {
        }

        /// <summary>
        /// Can be overridden by inherting class to do finalization stuff. Base does nothing.
        /// </summary>
        protected virtual void Finished()
        {
        }
    }
}
