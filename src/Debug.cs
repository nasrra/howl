using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Howl;
using Howl.Graphics;
using Howl.Math;
using Howl.Vendors;

public static class Debug
{
    /// <summary>
    ///     Enables and disables debug logging.
    /// </summary>
    public static bool SuppressLog = false;

    public static ConsoleColor LogErrorColour = ConsoleColor.Red;
    public static ConsoleColor LogWarningColour = ConsoleColor.Yellow;
    public static ConsoleColor LogStackTraceTagColour = ConsoleColor.Blue;
    public static ConsoleColor LogStackTraceTextColour = ConsoleColor.DarkGray;
    public static ConsoleColor LogInfoColour = ConsoleColor.Cyan;
    public static ConsoleColor LogTextColour = ConsoleColor.Cyan;
    public static ConsoleColor LogTimeStampColour = ConsoleColor.Gray;

    public static string InfoTag = "[Info]";
    public static string WarningTag = "[Warning]";
    public static string ErrorTag = "[Error]";
    public static string StackTraceTag = "[StackTrace]";

    public static void Log(string msg, int stackDepth = 0, int stackStart = 0, [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "", [CallerLineNumber] int lineNumber = 0
    )
    {
        if (SuppressLog)
        {
            return;
        }

        Console.ForegroundColor = LogTextColour;
        Console.Write($"{msg} ");

        Console.ForegroundColor = LogTimeStampColour;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]");

        if(stackDepth > 0)
        {            
            Console.ForegroundColor = LogStackTraceTagColour;
            Console.WriteLine($"{StackTraceTag}");
            Console.ForegroundColor = LogStackTraceTextColour;
            
            var stackTrace = new StackTrace(stackStart);

            for(int i = stackDepth; i > 0 ; i--)
            {                
                var frame = stackTrace.GetFrame(i);

                // Stop if the stack isn't deep enough
                if (frame == null) 
                    break; 
                
                var method = frame.GetMethod();
                
                if(method != null)
                {
                    if(i == 1)
                    {
                        Console.WriteLine($"\t {method.DeclaringType?.Name ?? "Unknown"}.{method.Name}(): line {lineNumber}");                        
                    }
                    else
                    {
                        Console.WriteLine($"\t {method.DeclaringType?.Name ?? "Unknown"}.{method.Name}()");
                    }
                }
            }
        }
    }

    public static void LogInfo(string msg, int stackDepth = 0, [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "", [CallerLineNumber] int lineNumber = 0
    )
    {
        if (SuppressLog)
        {
            return;
        }        

        Console.ForegroundColor = LogInfoColour;
        Console.Write($"{InfoTag} ");
        Log(msg, stackDepth, 1, filePath, methodName, lineNumber);
    }

    public static void LogWarning(string msg, int stackDepth = 0, [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "", [CallerLineNumber] int lineNumber = 0
    )
    {
        if (SuppressLog)
        {
            return;
        }        

        Console.ForegroundColor = LogWarningColour;
        Console.Write($"{WarningTag} ");
        Console.ForegroundColor = LogTextColour;
        Log(msg, stackDepth, 1, filePath, methodName, lineNumber);
    }

    public static void LogError(string msg, int stackDepth = 0, [CallerFilePath] string filePath = "",
        [CallerMemberName] string methodName = "", [CallerLineNumber] int lineNumber = 0
    )
    {
        if (SuppressLog)
        {
            return;
        }        

        Console.ForegroundColor = LogErrorColour;
        Console.Write($"{ErrorTag} ");
        Console.ForegroundColor = LogTextColour;
        Log(msg, stackDepth, 1, filePath, methodName, lineNumber);
    }

    public static void Assert(bool assert, string msg)
    {
        System.Diagnostics.Debug.Assert(assert, msg);
    }

    public static void Panic(string msg = default)
    {
        System.Diagnostics.Debug.Assert(false, msg);        
    }
}