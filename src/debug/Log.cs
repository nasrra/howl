using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Howl.Debug;

public static class Log
{
    /// <summary>
    ///     Enables and disables debug logging.
    /// </summary>
    public static bool Suppress = false;

    public const string InfoTag = "[Info]";
    public const string WarnTag = "[Warn]";
    public const string ErrorTag = "[Error]";
    public const string FallbackTag = "[Undefined]";

    /// <summary>
    ///     Writes a line to both the console and debugger.
    /// </summary>
    /// <param name="logType">the type of log.</param>
    /// <param name="msg">the message to accompany the log.</param>
    public static void WriteLine(LogType logType, string msg)
    {
        if(Suppress == true)
        {
            return;
        }


        msg = $"{GetLogTypeTag(logType)} {msg}";
        
        Console.WriteLine(msg);
        System.Diagnostics.Debug.WriteLine(msg);
    }

    /// <summary>
    ///     Gets the tagline for a log type.
    /// </summary>
    /// <param name="logType">the log type.</param>
    /// <returns>the tagline.</returns>
    public static string GetLogTypeTag(LogType logType)
    {
        switch (logType)
        {
            case LogType.Info:
                return InfoTag;
            case LogType.Warn:
                return WarnTag;
            case LogType.Error:
                return ErrorTag;
            default:
                return FallbackTag;
        }
    }

    /// <summary>
    ///     Logs a method call.
    /// </summary>
    /// <param name="logType">the type of log.</param>
    /// <param name="stackDepth">how far up the stack frame the printed method call string should be.</param>
    /// <param name="msg">the message to accompany the log.</param>
    /// <param name="filePath"></param>
    /// <param name="methodName"></param>
    /// <param name="lineNumber"></param>
    public static void MethodCall(LogType logType, int stackDepth, string msg, [CallerFilePath] string filePath = "",[CallerMemberName] string methodName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        var stackTrace = new StackTrace();

        string stack = "";

        for(int i = stackDepth; i > 0; i--)
        {
            if (i != stackDepth)
            {
                stack+=".";
            }
            var method = stackTrace.GetFrame(i).GetMethod();
            stack += $"{method.DeclaringType?.Name ?? "Unknown"}";            
        }

        WriteLine(logType, $"{stack}.{methodName}():line {lineNumber}, {msg}");
    }

    /// <summary>
    ///     Logs a method call.
    /// </summary>
    /// <param name="logType">the type of log.</param>
    /// <param name="msg">the message to accompany the log.</param>
    /// <param name="filePath"></param>
    /// <param name="methodName"></param>
    /// <param name="lineNumber"></param>
    public static void MethodCall(LogType logType, string msg, [CallerFilePath] string filePath = "",[CallerMemberName] string methodName = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        string className = System.IO.Path.GetFileNameWithoutExtension(filePath);
        WriteLine(logType, $"{className}.{methodName}():line {lineNumber}, {msg}");
    }
}