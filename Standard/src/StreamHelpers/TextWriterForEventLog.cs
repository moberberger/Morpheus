#if _KERNEL32_OK_

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;



namespace Morpheus
{
    /// <summary>
    /// This class allows an applicaiton to forward all strings sent for Console.Out to the EventLog instead.
    /// </summary>
    public class CTextWriterForEventLog : TextWriter
    {
        /// <summary>
        /// The EventLog object that will be written to
        /// </summary>
        private EventLog m_eventLog;

        /// <summary>
        /// This is the "working" string that the Write(char) method works on
        /// </summary>
        private StringBuilder m_workingString = new StringBuilder();


        /// <summary>
        /// Must construct this class with the EventLog that will be written to
        /// </summary>
        /// <param name="_eventLog">The EventLog to write strings to</param>
        public CTextWriterForEventLog( EventLog _eventLog )
        {
            m_eventLog = _eventLog;
        }

        /// <summary>
        /// Don't know what this is really good for, but whatever
        /// </summary>
        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }

        /// <summary>
        /// All other TextWriter "Write" functions eventually call this one.
        /// </summary>
        /// <param name="_nextChar"></param>
        public override void Write( char _nextChar )
        {
            m_workingString.Append( _nextChar );

            int len = m_workingString.Length;
            int checkLen = CoreNewLine.Length;

            if (len >= checkLen)
            {
                for (int i = 0; i < checkLen; i++)
                {
                    if (CoreNewLine[i] != m_workingString[len - checkLen + i])
                        return; // The current working string does NOT end in the CoreNewLine characters
                }

                // Since the loop completed without the "return" occuring, we know the current working string needs to be put into the event log
                m_workingString.Length -= checkLen; // The actual string does NOT include the \r\n because its going to the event log

                EventLogEntryType logType = EventLogEntryType.Information;
                foreach (string logTypeName in Enum.GetNames( typeof( EventLogEntryType ) ))
                {
                    if (m_workingString.Length >= logTypeName.Length)
                    {
                        string s = m_workingString.ToString( 0, logTypeName.Length );
                        if (0 == string.Compare( logTypeName, s, true ))
                        {
                            logType = (EventLogEntryType) Enum.Parse( typeof( EventLogEntryType ), s, true );
                        }
                    }
                }

                m_eventLog.WriteEntry( m_workingString.ToString(), logType );
                m_workingString.Length = 0;
            }
        }
    }
}

#endif