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
        static object consoleLock = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="memberName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        public static void TraceMessage(string message, TraceLevel level = TraceLevel.INFO,
                        [CallerMemberName] string memberName = "",
                        [CallerFilePath] string sourceFilePath = "",
                        [CallerLineNumber] int sourceLineNumber = 0)
        {
            lock(consoleLock)
            {
                Console.WriteLine($"[{sourceFilePath}@{sourceLineNumber}]:{message}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="memberName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="sourceLineNumber"></param>
        public static void TraceMessage(Exception e, TraceLevel level = TraceLevel.DEBUG,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (e == null)
                return;

            lock(consoleLock)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"[{sourceFilePath}@{sourceLineNumber}]:{e.Message}");
                Console.ResetColor();

                if (level == TraceLevel.ERROR || level == TraceLevel.VERBOSE)
                {
                    var ex = e.InnerException;
                    var loopCounter = 1;

                    while(ex != null)
                    {
                        Console.WriteLine($"Inner #{loopCounter++} - {ex.Message}");
                        ex = e.InnerException;
                    }

                    Console.WriteLine(e.StackTrace);
                }  
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum TraceLevel
    {
        /// <summary>
        /// 
        /// </summary>
        INFO,
        /// <summary>
        /// 
        /// </summary>
        DEBUG,
        /// <summary>
        /// 
        /// </summary>
        VERBOSE,
        /// <summary>
        /// 
        /// </summary>
        ERROR
    }
}
