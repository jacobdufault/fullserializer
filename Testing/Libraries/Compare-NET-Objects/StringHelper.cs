using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KellermanSoftware.CompareNetObjects
{
    /// <summary>
    /// Methods for manipulating strings
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Insert spaces into a string 
        /// </summary>
        /// <example>
        /// OrderDetails = Order Details
        /// 10Net30 = 10 Net 30
        /// FTPHost = FTP Host
        /// </example> 
        /// <param name="input"></param>
        /// <returns></returns>
        public static string InsertSpaces(string input)
        {
            bool isSpace = false;
            bool isUpperOrNumber = false;
            bool isLower = false;
            bool isLastUpper = true;
            bool isNextCharLower = false;

            if (String.IsNullOrEmpty(input))
                return string.Empty;

            StringBuilder sb = new StringBuilder(input.Length + (int)(input.Length / 2));

            //Replace underline with spaces
            input = input.Replace("_", " ");
            input = input.Replace("-", " ");
            input = input.Replace("  ", " ");

            //Trim any spaces
            input = input.Trim();

            char[] chars = input.ToCharArray();

            sb.Append(chars[0]);

            for (int i = 1; i < chars.Length; i++)
            {
                isUpperOrNumber = (chars[i] >= 'A' && chars[i] <= 'Z') || (chars[i] >= '0' && chars[i] <= '9');
                isNextCharLower = i < chars.Length - 1 && (chars[i + 1] >= 'a' && chars[i + 1] <= 'z');
                isSpace = chars[i] == ' ';
                isLower = (chars[i] >= 'a' && chars[i] <= 'z');

                //There was a space already added
                if (isSpace)
                {
                }
                //Look for upper case characters that have lower case characters before
                //Or upper case characters where the next character is lower
                else if ((isUpperOrNumber && isLastUpper == false)
                    || (isUpperOrNumber && isNextCharLower && isLastUpper == true))
                {
                    sb.Append(' ');
                    isLastUpper = true;
                }
                else if (isLower)
                {
                    isLastUpper = false;
                }

                sb.Append(chars[i]);

            }

            //Replace double spaces
            sb.Replace("  ", " ");

            return sb.ToString();
        }
    }
}
