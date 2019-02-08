using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Betlln
{
    public class ConsoleScreen : IConsoleScreen
    {
        public void UpdateProgress(string message)
        {
            // ReSharper disable once LocalizableElement
            Console.Write("\r{0}", message);
        }

        public void UpdateProgress(string format, string variableWidthValue, params object[] otherValues)
        {
            List<object> args = new List<object>();
            args.Add(variableWidthValue);
            args.AddRange(otherValues);

            string message = string.Format(format, args.ToArray());
            message = message.PadRight(message.Length + variableWidthValue.Length, ' ');

            // ReSharper disable once LocalizableElement
            Console.Write($"\r{message}");
        }

        public void UpdateProgressComplete(string message = null)
        {
            Console.WriteLine(message);
        }

        public void ConfirmExit()
        {
            if (Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.Write(@"Press any key to exit...");
                Console.ReadKey();
            }
        }

        public static readonly ConsoleScreen Default = new ConsoleScreen();
    }
}