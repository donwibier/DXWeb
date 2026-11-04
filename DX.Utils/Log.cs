using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


////////////////////////////////////////////////////////////////////////////////////////////////////
// namespace: DX.Utils
//
// summary:	.
////////////////////////////////////////////////////////////////////////////////////////////////////

namespace DX.Utils
{
    public class Log
    {
        public enum LogType { None = 0, Warning = 5, Error = 10 };

        [ThreadStatic]
        private static StringBuilder threadLogger;

        // Upper bound (in chars) of the per-thread log buffer. When exceeded, the oldest half is
        // discarded so long-lived (pool) threads cannot grow the buffer without limit.
        private const int MaxThreadLogLength = 1024 * 1024;


        public static void Exception(Exception err)
        {
            Exception(err, LogType.Error);            
        }
        public static void Exception(Exception err, LogType persistentLogType)
        {
            Write(err.Message, true, persistentLogType, err);
        }
        public static void Write(string logText)
        {
            Write(logText, true, LogType.None, typeof(Log));
        }

        public static void Write(string logText, bool traceWrite)
        {
            Write(logText, traceWrite, LogType.None, typeof(Log));
        }

        public static void Write(string logText, bool traceWrite, LogType persistentLogType, object persistentLogObject)
        {
            try
            {
                string line = String.Format("{0: dd-MM-yyyy HH:mm:ss}: {1}", DateTime.Now, logText);
                if (threadLogger == null)
                    threadLogger = new StringBuilder();
                threadLogger.AppendLine(line);
                TrimThreadLog();
                if (traceWrite)
                    Trace.WriteLine(line);

                Type logType = (persistentLogObject != null)? persistentLogObject.GetType() : typeof(Log);

                switch (persistentLogType)
                {
                    case LogType.Warning:
                        DoLog(logType, persistentLogObject, LogType.Warning, logText);
                        break;
                    case LogType.Error:
                        DoLog(logType, persistentLogObject, LogType.Error, logText);
                        break;
                    default:
                        break;
                }

            }
            catch (Exception err)
            {
                DoLogException(err);
            }
        }

        public static string GetLog()
        {
            try
            {
                if (threadLogger != null)
                    return threadLogger.ToString();
            }
            catch (Exception err)
            {
                DoLogException(err);
            }
            return String.Empty;
        }

        public static void ClearLog()
        {
            try
            {
                threadLogger = new StringBuilder();
            }
            catch (Exception err)
            {
                DoLogException(err);
            }
        }

        private static void TrimThreadLog()
        {
            if (threadLogger.Length <= MaxThreadLogLength)
                return;
            // Keep roughly the newest half, starting at a line boundary when possible.
            int start = threadLogger.Length - MaxThreadLogLength / 2;
            string tail = threadLogger.ToString(start, threadLogger.Length - start);
            int nl = tail.IndexOf('\n');
            if (nl >= 0 && nl < tail.Length - 1)
                tail = tail.Substring(nl + 1);
            threadLogger = new StringBuilder(tail, MaxThreadLogLength);
        }

        private static void DoLogException(Exception err)
        {
            DoLog(typeof(Log), err, LogType.Error, String.Empty);
        }

        private static void DoLog(Type logType, object currentObject, LogType loggingType, string logText)
        {
            //private static readonly ILog logObject = LogManager.GetLogger(typeof(Utils));           
            //LogManager.GetLogger(currentObject.GetType());
            Trace.Write(String.Format("[{0}/{1}] {2} {3}", logType.Name, currentObject is Exception ? ((Exception)currentObject).Message : currentObject, loggingType, logText));
        }
    }

}
