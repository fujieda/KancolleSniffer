using System;
using System.IO;
#if LOG4NET
using log4net;
#endif

namespace TrotiNet
{
    internal class Log
    {
        /// <summary>
        /// Create a class logger
        /// </summary>
        public static ILog Get()
        {
#if LOG4NET
            return log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#else
            return new ILog();
#endif
        }
    }
#if !LOG4NET
    internal class ILog
    {
        public void Info(string s) {}
        public void Debug(string s) {}
        public void Error(Exception e) {}
    }
#endif
}
