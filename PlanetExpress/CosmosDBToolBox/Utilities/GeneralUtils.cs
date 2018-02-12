using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlanetExpress.Utilities
{
    class GeneralUtils
    {
        /// <summary>
        /// Helps support optional arguments for user input.
        /// </summary>
        /// <param name="inString"></param>
        /// <returns> Null if the string is an empty string, otherwise the string. </returns>
        public static string OptionalArgumentHelper(string inString) {
            return inString == "" ? null : inString;
        }

        /// <summary>
        /// Creates a menu header based on an array of headers.
        /// </summary>
        /// <param name="headers"> array containing the names of the column headers. </param>
        /// <returns> a formatted list of the menu headers and their deviding lines. </returns>
        public static List<string[]> CreateMenuHeader(string[] headers) {
            int len = headers.Length;
            string[] lines = new string[len];
            len--;
            while (len >= 0)
            {
                lines[len] = "--------------------";
                len--;
            }
            List<string[]> menuHeader = new List<string[]>();
            menuHeader.Add(headers);
            menuHeader.Add(lines);
            return menuHeader;
        }

        /// <summary>
        /// Pads elements printed to the console.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="padding"></param>
        /// <returns> a formatted string good for pretty printing. </returns>
        public static string PadElementsInLines(List<string[]> lines, int padding = 1)
        {
            int numElements = lines[0].Length;
            int[] maxValues = new int[numElements];
            StringBuilder sb = new StringBuilder();
            bool isFirst = true;

            for (int i = 0; i < numElements; i++)
            {
                maxValues[i] = lines.Max(x => x[i].Length) + padding;
            }
            foreach (var line in lines)
            {
                if (!isFirst)
                {
                    sb.AppendLine();
                }
                isFirst = false;
                for (int i = 0; i < line.Length; i++)
                {
                    string value = line[i];
                    sb.Append(value.PadRight(maxValues[i]));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Prompts for user input based on a question.
        /// </summary>
        /// <param name="question"></param>
        /// <returns> if the input is correct based on user input, the answer is returned. Otherwise recurse.</returns>
        public static string PromptInput(String question)
        {
            Console.WriteLine(question + ": ");
            String answer = Console.ReadLine().ToString();
            Console.WriteLine("You have entered: " + answer + ", is this correct?(Y/N): ");
            String confirmation = Console.ReadLine();
            if (confirmation.ToUpper().Equals("Y"))
            {
                return answer;
            }
            else
            {
                return PromptInput(question);
            }
        }
    }
}
