using System;
using System.Diagnostics;

namespace ProjectMew.Logger
{
    /// <summary>
    /// Logging interface
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Log file name
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Checks whether the log level contains the specified flag.
        /// </summary>
        /// <param name="type">The <see cref="TraceLevel" /> value to check.</param>
        bool MayWriteType(TraceLevel type);

        /// <summary>
        /// Writes an informative string to the log and to the console.
        /// </summary>
        /// <param name="message">The message to be written.</param>
        void ConsoleInfo(string message);

        /// <summary>
        /// Writes an informative string to the log and to the console.
        /// </summary>
        /// <param name="format">The format of the message to be written.</param>
        /// <param name="args">The format arguments.</param>
        void ConsoleInfo(string format, params object[] args);

        /// <summary>
        /// Writes an message string to the console.
        /// </summary>
        /// <param name="message">The message to be written.</param>
        void ConsoleMessage(string message);

        /// <summary>
        /// Writes an message string to the console.
        /// </summary>
        /// <param name="format">The format of the message to be written.</param>
        /// <param name="args">The format arguments.</param>
        void ConsoleMessage(string format, params object[] args);

        /// <summary>
        /// Writes an error message to the log and to the console.
        /// </summary>
        /// <param name="message">The message to be written.</param>
        void ConsoleError(string message);

        /// <summary>
        /// Writes an error message to the log and to the console.
        /// </summary>
        /// <param name="format">The format of the message to be written.</param>
        /// <param name="args">The format arguments.</param>
        void ConsoleError(string format, params object[] args);

        /// <summary>
        /// Writes a warning to the log.
        /// </summary>
        /// <param name="message">The message to be written.</param>
        void Warn(string message);

        /// <summary>
        /// Writes a warning to the log.
        /// </summary>
        /// <param name="format">The format of the message to be written.</param>
        /// <param name="args">The format arguments.</param>
        void Warn(string format, params object[] args);

        /// <summary>
        /// Writes an error to the log.
        /// </summary>
        /// <param name="message">The message to be written.</param>
        void Error(string message);

        /// <summary>
        /// Writes an error to the log.
        /// </summary>
        /// <param name="format">The format of the message to be written.</param>
        /// <param name="args">The format arguments.</param>
        void Error(string format, params object[] args);

        /// <summary>
        /// Writes an informative string to the log.
        /// </summary>
        /// <param name="message">The message to be written.</param>
        void Info(string message);

        /// <summary>
        /// Writes an informative string to the log.
        /// </summary>
        /// <param name="format">The format of the message to be written.</param>
        /// <param name="args">The format arguments.</param>
        void Info(string format, params object[] args);

        /// <summary>
        /// Writes data to the log.
        /// </summary>
        /// <param name="message">The message to be written.</param>
        void Data(string message);

        /// <summary>
        /// Writes data to the log.
        /// </summary>
        /// <param name="format">The format of the message to be written.</param>
        /// <param name="args">The format arguments.</param>
        void Data(string format, params object[] args);

        /// <summary>
        /// Writes a message to the log
        /// </summary>
        /// <param name="message">Message to write</param>
        /// <param name="level">LogLevel assosciated with the message</param>
        void Write(string message, TraceLevel level);

        /// <summary>
        /// Writes a debug string to the log file. Only works if the DEBUG preprocessor conditional is set.
        /// </summary>
        /// <param name="message">The message to be written.</param>
        void Debug(string message);

        /// <summary>
        /// Writes a debug string to the log file. Only works if the DEBUG preprocessor conditional is set.
        /// </summary>
        /// <param name="format">The format of the message to be written.</param>
        /// <param name="args">The format arguments.</param>
        void Debug(string format, params object[] args);

        /// <summary>
        /// Dispose the Log
        /// </summary>
        void Dispose();
    }
}