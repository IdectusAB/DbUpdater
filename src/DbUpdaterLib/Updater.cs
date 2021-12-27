// <copyright file="Updater.cs" company="IdectusAB">
// Copyright (c) IdectusAB. All rights reserved.
// </copyright>
namespace DbUpdaterLib
{
    using System;
    using System.IO;
    using MySql.Data.MySqlClient;

    /// <summary>
    /// Main class that does all the work.
    /// </summary>
    public class Updater
    {
        private const string Version = "v0.0.1";
        private readonly bool isDebug = false;
        private string scriptPath;
        private string dbConnectionString;
        private MySqlConnection databaseConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="Updater"/> class.
        /// Main class for this lib.
        /// </summary>
        public Updater()
        {
            Console.WriteLine($"DbUpdater {Version}");
#if DEBUG
            this.isDebug = true;
#endif
        }

        /// <summary>
        /// Init the updater with info supplied in args.
        /// </summary>
        /// <param name="args">args with path[0] and connectionString[1].</param>
        /// <returns>True if all good, false if not.</returns>
        public bool Init(string[] args)
        {
            if (this.isDebug)
            {
                Console.WriteLine("DbUpdaterLib.Updater.Init()");
            }

            // Check that args are not null
            if (args == null)
            {
                Console.WriteLine("Error: Args must be supplied");
                WriteUsage();

                return false;
            }

            // Check that we have exactly 2 args supplied
            if (args.Length != 2)
            {
                Console.WriteLine($"Error: Invalid number of args ({args.Length}).");
                if (this.isDebug)
                {
                    Console.WriteLine($"DbUpdaterLib.Updater.Init() - args: ({args}).");
                }

                WriteUsage();
                return false;
            }

            // Set path to scripts and database connection string
            this.scriptPath = args[0];
            this.dbConnectionString = args[1];

            // Return true if everything checks out
            return true;
        }

        /// <summary>
        /// Run the actual update process by parsing all SQL files and running them on the database.
        /// </summary>
        public void Update()
        {
            if (this.isDebug)
            {
                Console.WriteLine("DbUpdaterLib.Updater.Update()");
            }

            // Find all script files in specified folder
            Console.WriteLine($"Loading scripts...");
            FileInfo[] scriptFiles = Utils.FindSqlFiles(this.scriptPath);
            if (scriptFiles == null)
            {
                Console.WriteLine($"Error: Folder [{this.scriptPath}] does not exist.");
                return;
            }

            if (scriptFiles.Length == 0)
            {
                Console.WriteLine("Error: No *.sql files found in folder.");
                return;
            }

            try
            {
                // Create and open the database connection
                this.databaseConnection = new MySqlConnection(this.dbConnectionString);
                this.databaseConnection.Open();

                // Run all scripts on the database
                Console.WriteLine($"Runnings scripts...");
                foreach (var scriptFile in scriptFiles)
                {
                    Console.Write($"Current file: {scriptFile.FullName}");
                    if (!Utils.RunScript(this.databaseConnection, scriptFile.FullName))
                    {
                        break;
                    }

                    Console.Write(Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.Write($"Exception: {ex.Message}");
            }
            finally
            {
                this.databaseConnection.Close();
            }
        }

        /// <summary>
        /// Cleanup stuff. Not sure if this is necessary, but keeping it anyways.
        /// </summary>
        public void Cleanup()
        {
            if (this.isDebug)
            {
                Console.WriteLine("DbUpdaterLib.Updater.Cleanup()");
            }

            // This should not be necessary.
            if (this.databaseConnection != null)
            {
                this.databaseConnection.Close();
            }
        }

        /// <summary>
        /// Simple ping method used by default test.
        /// </summary>
        /// <returns>Always returns string "Pong".</returns>
        public string Ping()
        {
            if (this.isDebug)
            {
                Console.WriteLine("DbUpdaterLib.Updater.Ping()");
            }

            return "Pong";
        }

        /// <summary>
        /// Writes a short usage message.
        /// </summary>
        private static void WriteUsage()
        {
            Console.WriteLine("----------------");
            Console.WriteLine("Usage:");
            Console.WriteLine("DbUpdater [folderPath] [connectionString]");
            Console.WriteLine("Example:");
            Console.WriteLine("DbUpdater \"c:\\myscripts\" \"Server=127.0.0.1;Port=3306;Database=dbName;Uid=dbUser;Pwd=strongPassword;\"");
            Console.WriteLine("----------------");
        }
    }
}
