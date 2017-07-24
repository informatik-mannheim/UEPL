using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ProjectAPI.Helper
{
    /// <summary>
    /// Class to trace messages (caller and other diagnostic vars)
    /// </summary>
    public class Tracer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="memberName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        public static void TraceMessage(string message,
                                [CallerMemberName] string memberName = "",
                                [CallerFilePath] string sourceFilePath = "",
                                [CallerLineNumber] int sourceLineNumber = 0)
        {
            Console.WriteLine($"[{sourceFilePath}@{sourceLineNumber}]:{message}");
        }
    }
}
