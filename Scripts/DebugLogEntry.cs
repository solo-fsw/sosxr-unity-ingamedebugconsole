using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;


// Container for a simple debug entry
namespace IngameDebugConsole
{
    /// <summary>
    /// Pooled container for a single debug log entry. Holds the message string, stack trace, display sprite, and collapse metadata.
    /// Instances are reused via <see cref="DebugLogManager"/>'s internal pool to minimise allocations.
    /// </summary>
    public class DebugLogEntry
    {
        /// <summary>The log message string.</summary>
        public string logString;
        /// <summary>The stack trace string captured with this entry.</summary>
        public string stackTrace;
        private string completeLog;

        // Sprite to show with this entry
        /// <summary>The sprite icon corresponding to the log type (info, warning, error).</summary>
        public Sprite logTypeSpriteRepresentation;

        // Collapsed count
        /// <summary>Number of times this exact entry has been received (used for the collapse-mode count badge).</summary>
        public int count;

        // Index of this entry among all collapsed entries
        /// <summary>Index of this entry within <c>collapsedLogEntries</c>; used for O(1) lookup during collapse updates.</summary>
        public int collapsedIndex;

        private int hashValue;
        private const int HASH_NOT_CALCULATED = -623218;


        /// <summary>Resets this entry with new log data, ready to be displayed or pooled.</summary>
        public void Initialize(string logString, string stackTrace)
        {
            this.logString = logString;
            this.stackTrace = stackTrace;

            completeLog = null;
            count = 1;
            hashValue = HASH_NOT_CALCULATED;
        }


        /// <summary>Nullifies all string references so this entry can be safely returned to the pool.</summary>
        public void Clear()
        {
            logString = null;
            stackTrace = null;
            completeLog = null;
        }


        // Checks if logString or stackTrace contains the search term
        /// <summary>Returns <c>true</c> if <paramref name="searchTerm"/> appears (case-insensitively) in either the log message or stack trace.</summary>
        public bool MatchesSearchTerm(string searchTerm)
        {
            return (logString != null && DebugLogConsole.caseInsensitiveComparer.IndexOf(logString, searchTerm, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0) ||
                   (stackTrace != null && DebugLogConsole.caseInsensitiveComparer.IndexOf(stackTrace, searchTerm, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0);
        }


        // Return a string containing complete information about this debug entry
        public override string ToString()
        {
            if (completeLog == null)
            {
                completeLog = string.Concat(logString, "\n", stackTrace);
            }

            return completeLog;
        }


        // Credit: https://stackoverflow.com/a/19250516/2373034
        /// <summary>Returns a stable hash code based solely on the log content (message + stack trace), used for collapse deduplication.</summary>
        public int GetContentHashCode()
        {
            if (hashValue == HASH_NOT_CALCULATED)
            {
                unchecked
                {
                    hashValue = 17;
                    hashValue = hashValue * 23 + (logString == null ? 0 : logString.GetHashCode());
                    hashValue = hashValue * 23 + (stackTrace == null ? 0 : stackTrace.GetHashCode());
                }
            }

            return hashValue;
        }
    }


    /// <summary>
    /// Lightweight, allocation-friendly struct queued from any thread. Processed by <see cref="DebugLogManager"/> on the main thread.
    /// </summary>
    public struct QueuedDebugLogEntry
    {
        public readonly string logString;
        public readonly string stackTrace;
        public readonly LogType logType;


        /// <summary>Creates a new queued entry with the given log data.</summary>
        public QueuedDebugLogEntry(string logString, string stackTrace, LogType logType)
        {
            this.logString = logString;
            this.stackTrace = stackTrace;
            this.logType = logType;
        }


        // Checks if logString or stackTrace contains the search term
        public bool MatchesSearchTerm(string searchTerm)
        {
            return (logString != null && DebugLogConsole.caseInsensitiveComparer.IndexOf(logString, searchTerm, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0) ||
                   (stackTrace != null && DebugLogConsole.caseInsensitiveComparer.IndexOf(stackTrace, searchTerm, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0);
        }
    }


    /// <summary>
    /// Captures when a log entry was received: wall-clock time, elapsed runtime seconds, and frame count.
    /// Which fields are compiled in is controlled by <c>IDG_OMIT_ELAPSED_TIME</c> and <c>IDG_OMIT_FRAMECOUNT</c> defines.
    /// </summary>
    public struct DebugLogEntryTimestamp
    {
        public readonly DateTime dateTime;
        #if !IDG_OMIT_ELAPSED_TIME
        public readonly float elapsedSeconds;
        #endif
        #if !IDG_OMIT_FRAMECOUNT
        public readonly int frameCount;
        #endif

        #if !IDG_OMIT_ELAPSED_TIME && !IDG_OMIT_FRAMECOUNT
        public DebugLogEntryTimestamp(DateTime dateTime, float elapsedSeconds, int frameCount)
            #elif !IDG_OMIT_ELAPSED_TIME
		public DebugLogEntryTimestamp( System.DateTime dateTime, float elapsedSeconds )
            #elif !IDG_OMIT_FRAMECOUNT
		public DebugLogEntryTimestamp( System.DateTime dateTime, int frameCount )
            #else
		public DebugLogEntryTimestamp( System.DateTime dateTime )
            #endif
        {
            this.dateTime = dateTime;
            #if !IDG_OMIT_ELAPSED_TIME
            this.elapsedSeconds = elapsedSeconds;
            #endif
            #if !IDG_OMIT_FRAMECOUNT
            this.frameCount = frameCount;
            #endif
        }


        /// <summary>Appends the time in <c>[HH:mm:ss]</c> format to <paramref name="sb"/>.</summary>
        public void AppendTime(StringBuilder sb)
        {
            // Add DateTime in format: [HH:mm:ss]
            sb.Append("[");

            var hour = dateTime.Hour;

            if (hour >= 10)
            {
                sb.Append(hour);
            }
            else
            {
                sb.Append("0").Append(hour);
            }

            sb.Append(":");

            var minute = dateTime.Minute;

            if (minute >= 10)
            {
                sb.Append(minute);
            }
            else
            {
                sb.Append("0").Append(minute);
            }

            sb.Append(":");

            var second = dateTime.Second;

            if (second >= 10)
            {
                sb.Append(second);
            }
            else
            {
                sb.Append("0").Append(second);
            }

            sb.Append("]");
        }


        /// <summary>Appends the full timestamp (time, elapsed seconds, and frame count) to <paramref name="sb"/>.</summary>
        public void AppendFullTimestamp(StringBuilder sb)
        {
            AppendTime(sb);

            #if !IDG_OMIT_ELAPSED_TIME && !IDG_OMIT_FRAMECOUNT
            // Append elapsed seconds and frame count in format: [1.0s at #Frame]
            sb.Append("[").Append(elapsedSeconds.ToString("F1")).Append("s at ").Append("#").Append(frameCount).Append("]");
            #elif !IDG_OMIT_ELAPSED_TIME
			// Append elapsed seconds in format: [1.0s]
			sb.Append( "[" ).Append( elapsedSeconds.ToString( "F1" ) ).Append( "s]" );
            #elif !IDG_OMIT_FRAMECOUNT
			// Append frame count in format: [#Frame]
			sb.Append( "[#" ).Append( frameCount ).Append( "]" );
            #endif
        }
    }


    /// <summary>
    /// Equality comparer that treats two <see cref="DebugLogEntry"/> objects as equal when their log string and stack trace match.
    /// Used as the key comparer for the collapse deduplication dictionary.
    /// </summary>
    public class DebugLogEntryContentEqualityComparer : EqualityComparer<DebugLogEntry>
    {
        public override bool Equals(DebugLogEntry x, DebugLogEntry y)
        {
            return x.logString == y.logString && x.stackTrace == y.stackTrace;
        }


        public override int GetHashCode(DebugLogEntry obj)
        {
            return obj.GetContentHashCode();
        }
    }
}