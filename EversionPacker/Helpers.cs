using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ShinyTools.Helpers
{
    #region Console printing helpers
    public static class Console
    {
        /// <summary>
        /// Write text to the console using colours.
        /// </summary>
        /// <param name="text">The text to write to the console.</param>
        /// <param name="textColour">The colour the font should be when written.</param>
        /// <param name="backgroundColour">The colour the background should be behind the text.</param>
        /// <param name="WriteOnNewLine">If true, will write the text on a new line. If false, will write the text inline.</param>
        public static string WriteColor(string text, ConsoleColor textColour, ConsoleColor backgroundColour, bool WriteOnNewLine)
        {
            System.Console.ForegroundColor = textColour;
            System.Console.BackgroundColor = backgroundColour;
            if (WriteOnNewLine) System.Console.WriteLine(text);
            else System.Console.Write(text);
            System.Console.ResetColor();
            return null;
        }
        public static string WriteColor(string text, ConsoleColor textColour, ConsoleColor backgroundColour) => WriteColor(text, textColour, backgroundColour, false);
        public static string WriteColor(string text, ConsoleColor textColour, bool WriteOnNewLine) => WriteColor(text, textColour, System.Console.BackgroundColor, WriteOnNewLine);
        public static string WriteColor(string text, ConsoleColor textColour) => WriteColor(text, textColour, System.Console.BackgroundColor, false);
        public static string WriteColor(string text, bool WriteOnNewLine) => WriteColor(text, System.Console.ForegroundColor, System.Console.BackgroundColor, WriteOnNewLine);
        public static string WriteColor(string text) => WriteColor(text, System.Console.ForegroundColor, System.Console.BackgroundColor, false);
    }
    #endregion
    #region Application helpers
    public static class Application
    {
        /// <summary>
        /// Displays a message before exiting the program.
        /// </summary>
        /// <param name="result">The text to be displayed when exiting.</param>
        /// <param name="timeout">The time in milliseconds before the program closes.</param>
        /// <param name="exitCode">Environment exit code (for debugging and logging).</param>
        public static void Quit(string result, int timeout, int exitCode)
        {
            System.Console.WriteLine(result);
            Thread.Sleep(timeout);
            Environment.Exit(exitCode);
        }
        public static void Quit(string result, int timeout) => Quit(result, timeout, 0);
        public static void Quit(string result) => Quit(result, 1200, 0);
        public static void Quit() => Quit("", 1200, 0);
    }
    #endregion
    #region I/O helpers
    public static class IO
    {
        public static void Write(byte[] data, string directory, string filename, string extension)
        {
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            var f = File.Create(directory + filename + extension);
            f.Write(data, 0, data.Length);
            f.Close();
        }
    }
    #endregion
    #region Comparison helpers
    public static class Comparers
    {
		public class IndexComparer : IComparer<string>
		{
			Regex _reg = new Regex(@"\Z_(?<index>[\D]).*");
			public int Compare(string first, string second)
			{
				var _1st = _reg.Match(first).Groups["index"].Value;
				var _2nd = _reg.Match(second).Groups["index"].Value;
				return _1st.CompareTo(_2nd);
			}
		}

        //http://www.interact-sw.co.uk/iangblog/2007/12/13/natural-sorting
        /// <summary>
        /// Compares two sequences.
        /// </summary>
        /// <typeparam name="T">Type of item in the sequences.</typeparam>
        /// <remarks>
        /// Compares elements from the two input sequences in turn. If we
        /// run out of list before finding unequal elements, then the shorter
        /// list is deemed to be the lesser list.
        /// </remarks>
        public class EnumerableComparer<T> : IComparer<IEnumerable<T>>
        {
            /// <summary>
            /// Create a sequence comparer using the default comparer for T.
            /// </summary>
            public EnumerableComparer()
            {
                comp = Comparer<T>.Default;
            }

            /// <summary>
            /// Create a sequence comparer, using the specified item comparer
            /// for T.
            /// </summary>
            /// <param name="comparer">Comparer for comparing each pair of
            /// items from the sequences.</param>
            public EnumerableComparer(IComparer<T> comparer)
            {
                comp = comparer;
            }

            /// <summary>
            /// Object used for comparing each element.
            /// </summary>
            private IComparer<T> comp;


            /// <summary>
            /// Compare two sequences of T.
            /// </summary>
            /// <param name="x">First sequence.</param>
            /// <param name="y">Second sequence.</param>
            public int Compare(IEnumerable<T> x, IEnumerable<T> y)
            {
                using (IEnumerator<T> leftIt = x.GetEnumerator())
                using (IEnumerator<T> rightIt = y.GetEnumerator())
                {
                    while (true)
                    {
                        bool left = leftIt.MoveNext();
                        bool right = rightIt.MoveNext();

                        if (!(left || right)) return 0;

                        if (!left) return -1;
                        if (!right) return 1;

                        int itemResult = comp.Compare(leftIt.Current, rightIt.Current);
                        if (itemResult != 0) return itemResult;
                    }
                }
            }
        }
    }
	#endregion
	#region Data parsers
    public static class Parsers
	{
        public static byte[] SwapEndianness(byte[] Data, int BytesPerSwap)
        {
            if (Data != null && Data.Length >= BytesPerSwap)
            {
                //Get bytes of SET file
                byte[] WorkingBuffer = Data;

                //Adds padding if SET isn't correctly sized
                if (WorkingBuffer.Length % BytesPerSwap != 0)
                {
                    if (WorkingBuffer.Length % BytesPerSwap != 0)
                    {
                        Array.Resize(ref WorkingBuffer, WorkingBuffer.Length + (WorkingBuffer.Length % BytesPerSwap));
                    }

                    for (int i = 0; i < WorkingBuffer.Length; i += BytesPerSwap)
                    {
                        //Converts data to ulong and discards unused bytes
                        ulong ReversedData = 0;
                        for (int x = 0; x < BytesPerSwap; x++)
                            ReversedData |= (ulong)WorkingBuffer[i + x] << (8 * x);

                        //Converts ulong into array of bytes
                        var Bytes = BitConverter.GetBytes(ReversedData);
                        //Reverses ulong byte array
                        Array.Reverse(Bytes);
                        //Writes bytes back into array
                        int ArrayIdx = 0;
                        //We do x=8-BPS because the beginning bytes will be the skipped ones (if not 8)
                        for (int x = 0 - BytesPerSwap; x < Bytes.Length; x++)
                        {
                            WorkingBuffer[i + ArrayIdx] = Bytes[x];
                            ArrayIdx++;
                        }
                    }
                }
                return WorkingBuffer;
            }
            else
            {
                return Data;
            }
        }
    }
	#endregion
}
