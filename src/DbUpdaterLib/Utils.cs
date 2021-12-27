// <copyright file="Utils.cs" company="IdectusAB">
// Copyright (c) IdectusAB. All rights reserved.
// </copyright>
namespace DbUpdaterLib
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using MySql.Data.MySqlClient;

    /// <summary>
    /// Small class for misc utils.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Helper function when parsing script to identify what lines can be skipped.
        /// </summary>
        /// <param name="line">The script line to check.</param>
        /// <returns>true when line can be skipped, false as default.</returns>
        public static bool MySqlIgnoreLine(string line)
        {
            // Ignore empty lines
            if (string.IsNullOrEmpty(line))
            {
                return true;
            }

            // Line is a MySQL comment
            if (line.StartsWith("#") || line.StartsWith("--"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the current line is a change of delimiter.
        /// </summary>
        /// <param name="line">The script line to check.</param>
        /// <returns>True if delimiter change has been requested.</returns>
        public static bool MySqlIsDelimiterChange(string line)
        {
            // Line is a change of current delimiter
            if (line.TrimStart().ToUpper().StartsWith("DELIMITER "))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Extracts the delimiter from a line.
        /// </summary>
        /// <param name="line">The script line to check.</param>
        /// <returns>The new delimiter.</returns>
        public static string MySqlExtractDelimiter(string line)
        {
            return line.ToUpper().Replace("DELIMITER", string.Empty).Trim();
        }

        /// <summary>
        /// Parses folder and subfolders and grabs all files ending with *.sql.
        /// </summary>
        /// <param name="folder">The folder to start parsing.</param>
        /// <returns>List of files found in folder and subfolders.</returns>
        public static FileInfo[] FindSqlFiles(string folder)
        {
#if DEBUG
            Console.WriteLine($"DbUpdaterLib.Updater.FindSqlFiles({folder})");
#endif

            FileInfo[] scriptFiles = null;
            try
            {
                DirectoryInfo objDirectoryInfo = new (folder);
                scriptFiles = objDirectoryInfo.GetFiles("*.sql", SearchOption.AllDirectories);
            }
            catch (DirectoryNotFoundException dnfex)
            {
#if DEBUG
                Console.WriteLine($"DbUpdaterLib.Updater.FindSqlFiles({folder}) - DirectoryNotFoundException: {dnfex.Message}");
#endif
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"DbUpdaterLib.Updater.FindSqlFiles({folder}) - Exception: {ex.Message}");
#endif
            }

            return scriptFiles;
        }

        /// <summary>
        /// Runs the *.sql script against a database.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="filePath">Path of the file.</param>
        /// <returns>True if no errors occured.</returns>
        public static bool RunScript(MySqlConnection connection, string filePath)
        {
            string[] lines = System.IO.File.ReadAllLines(filePath);
            string currentDelimiter = ";";
            bool isInMultilineComment = false;
            bool isInTextfield = false;
            string currentQuery = string.Empty;
            foreach (string originalLine in lines)
            {
                // Get rid of multiline comments placed on a single line
                string line = Utils.StripCommentsFromLine(originalLine);

                // If the line is a comment or empty, ignore it
                if (Utils.MySqlIgnoreLine(originalLine))
                {
                    continue;
                }

                // Handle multiline comments that spans over multiple lines.
                if (line.Contains("/*"))
                {
                    isInMultilineComment = true;
                    continue;
                }

                if (line.Contains("*/"))
                {
                    isInMultilineComment = false;
                    continue;
                }

                if (isInMultilineComment)
                {
                    continue;
                }

                // Change current delimiter
                if (Utils.MySqlIsDelimiterChange(line))
                {
                    currentDelimiter = Utils.MySqlExtractDelimiter(line);
                    currentQuery += line + Environment.NewLine;
                    continue;
                }

                currentQuery += line + Environment.NewLine;

                // Parse each char in the line and keep track of if we are inside a textfield.
                foreach (char c in line)
                {
                    if (c.Equals('\''))
                    {
                        isInTextfield = !isInTextfield;
                    }
                }

                // Trigger the query if current delimiter found and if we are not within a textstring.
                if (line.EndsWith(currentDelimiter) && !isInTextfield)
                {
                    if (!RunMySqlScript(connection, currentQuery, currentDelimiter))
                    {
                        return false;
                    }

                    currentQuery = string.Empty;
                    isInTextfield = false;
                    Console.Write(".");
                }
            }

            return true;
        }

        /// <summary>
        /// Strips multiline comments from a single line if they exist.
        /// </summary>
        /// <param name="line">The line to check.</param>
        /// <returns>A line free from multiline comments.</returns>
        public static string StripCommentsFromLine(string line)
        {
            if (line.Contains("/*") && line.Contains("*/"))
            {
                var blockComments = @"/\*(.*?)\*/";
                string noComments = Regex.Replace(
                    line,
                    blockComments,
                    me =>
                    {
                        if (me.Value.StartsWith("/*"))
                        {
                            return string.Empty;
                        }

                        return me.Value;
                    },
                    RegexOptions.Singleline);
                return noComments;
            }

            return line;
        }

        /// <summary>
        /// Run individual sql command.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="command">The command.</param>
        /// <returns>True is no error occured.</returns>
        private static bool RunMySqlCommand(MySqlConnection connection, string command)
        {
            try
            {
                var cmd = new MySqlCommand(command, connection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Environment.NewLine}Error:{Environment.NewLine}{command}{Environment.NewLine} failed with exception: {ex.Message}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Run individual sql query as script.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="query">The query.</param>
        /// <param name="delimiter">The current delimiter to use.</param>
        /// <returns>True is no error occured.</returns>
        private static bool RunMySqlScript(MySqlConnection connection, string query, string delimiter)
        {
            try
            {
                var script = new MySqlScript(connection, query);
                script.Delimiter = delimiter;
                script.Execute();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Environment.NewLine}Error:{Environment.NewLine}{query}{Environment.NewLine} failed with exception: {ex.Message}");
                return false;
            }

            return true;
        }
    }
}
