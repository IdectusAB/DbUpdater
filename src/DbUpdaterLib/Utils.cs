// <copyright file="Utils.cs" company="IdectusAB">
// Copyright (c) IdectusAB. All rights reserved.
// </copyright>
namespace DbUpdaterLib
{
    using System;
    using System.IO;
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
            string currentCommand = string.Empty;
            foreach (string line in lines)
            {
                if (Utils.MySqlIgnoreLine(line))
                {
                    continue;
                }

                if (Utils.MySqlIsDelimiterChange(line))
                {
                    currentDelimiter = Utils.MySqlExtractDelimiter(line);
                    continue;
                }

                currentCommand += line + Environment.NewLine;

                if (currentCommand.Contains(currentDelimiter))
                {
                    if (!currentDelimiter.Equals(";"))
                    {
                        currentCommand = currentCommand.Replace(currentDelimiter, string.Empty);
                    }

                    if (!RunMySqlCommand(connection, currentCommand))
                    {
                        return false;
                    }

                    currentCommand = string.Empty;
                    Console.Write(".");
                }
            }

            return true;
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
    }
}
