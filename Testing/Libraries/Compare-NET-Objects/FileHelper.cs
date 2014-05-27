using System;
using System.IO;

namespace KellermanSoftware.CompareNetObjects
{
    /// <summary>
    /// Helper methods for files and directories
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Get the current directory of the executing assembly
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDirectory()
        {
            return PathSlash(AppDomain.CurrentDomain.BaseDirectory);
        }

        /// <summary>
        /// Ensure the passed string ends with a directory separator character unless the string is blank.
        /// </summary>
        /// <param name="path">The string to append the backslash to.</param>
        /// <returns>String with a "/" on the end</returns>
        public static String PathSlash(string path)
        {
            string separator = Convert.ToString(Path.DirectorySeparatorChar);

            if (path.Length == 0)
                return path;
            else if (path.EndsWith(separator))
                return path;
            else
                return path + separator;
        }
    }
}
