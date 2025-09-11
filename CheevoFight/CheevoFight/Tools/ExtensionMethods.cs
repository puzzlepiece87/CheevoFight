using System.Data;
using System.IO;

namespace CheevoFight.Tools
{
    internal static class ExtensionMethods
    {
        public static string Left(this string original, int length)
        {
            if (length >= original.Length)
            {
                return original;
            }
            else
            {
                return original.Substring(0, length);
            }
        }


        public static string Mid(this string original, int startPosition, int length)
        {
            if (startPosition >= original.Length)
            {
                throw new ArgumentException(string.Join(Environment.NewLine,
                    "startPosition argument exceeds length of string.",
                    "Method executed: Mid(" + original + ", " + startPosition.ToString() + ", " + length.ToString() + ")"
                ), nameof(startPosition));
            }

            if (length >= original.Length - startPosition)
            {
                return original.Substring(startPosition, original.Length - startPosition);
            }
            else
            {
                return original.Substring(startPosition, length);
            }
        }


        public static string Right(this string original, int length)
        {
            if (length >= original.Length)
            {
                return original;
            }
            else
            {
                return original.Substring(original.Length - length);
            }
        }


        public static List<int> GetAllIndicesOfSubstring(this string stringToSearch, string substring)
        {
            if (string.IsNullOrEmpty(substring))
            {
                throw new ArgumentException("substring argument of GetAllIndicesOfSubstring() may not be null or empty", nameof(substring));
            }

            var indices = new List<int>();

            var i = -1;
            do
            {
                i = stringToSearch.IndexOf(substring, i + 1);

                if (i != -1)
                {
                    indices.Add(i);
                }
            } while (i != -1);

            return indices;
        }


        public static void WriteToCSV(this DataTable dataTable, string cSVPath)
        {
            using var streamWriter = new StreamWriter(cSVPath);

            var headers = new List<string>();
            foreach (DataColumn column in dataTable.Columns)
            {
                headers.Add(column.ColumnName);
            }
            streamWriter.WriteLine(string.Join("|", headers));

            foreach (DataRow row in dataTable.Rows)
            {
                streamWriter.WriteLine(string.Join("|", row.ItemArray));
            }
        }
    }
}