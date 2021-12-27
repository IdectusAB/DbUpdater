// <copyright file="UpdaterTests.cs" company="IdectusAB">
// Copyright (c) IdectusAB. All rights reserved.
// </copyright>
namespace DbUpdaterLibTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.IO;

    /// <summary>
    /// Just a few tests for the DbUpdaterLib.
    /// </summary>
    [TestClass]
    public class UpdaterTests
    {
        /// <summary>
        /// Make sure that we can talk to the lib by calling the Ping() method and expecting the string "Pong" to be returned.
        /// </summary>
        [TestMethod]
        public void PingLib()
        {
            DbUpdaterLib.Updater updater = new ();
            string actualValue = updater.Ping();
            Assert.AreEqual("Pong", actualValue);
        }

        /// <summary>
        /// Tests if a spcific line in the script will be ignored or not.
        /// </summary>
        [TestMethod]
        public void IgnoreLine()
        {
            string mySqlCommentLine = "# This is a comment   SELECT * FROM Table;";
            Assert.IsTrue(DbUpdaterLib.Utils.MySqlIgnoreLine(mySqlCommentLine));
            string mySqlCommentLine2 = "-- This is a another comment   SELECT * FROM Table;";
            Assert.IsTrue(DbUpdaterLib.Utils.MySqlIgnoreLine(mySqlCommentLine2));
            string mySqlCommandLine = "SELECT * FROM Table; # This is a comment, but after a command";
            Assert.IsFalse(DbUpdaterLib.Utils.MySqlIgnoreLine(mySqlCommandLine));
            string mySqlEmptyLine = string.Empty;
            Assert.IsTrue(DbUpdaterLib.Utils.MySqlIgnoreLine(mySqlEmptyLine));
        }

        /// <summary>
        /// Tests if the current script line is changing delimiter.
        /// </summary>
        [TestMethod]
        public void IsDelimiterChange()
        {
            string mySqlDelimiterChangeLine = "DELIMITER $$";
            Assert.IsTrue(DbUpdaterLib.Utils.MySqlIsDelimiterChange(mySqlDelimiterChangeLine));
            string mySqlCommandLine = "SELECT * FROM Table; # This is a comment, but after a command";
            Assert.IsFalse(DbUpdaterLib.Utils.MySqlIsDelimiterChange(mySqlCommandLine));
        }

        /// <summary>
        /// Tests if the new delimiter is extracted correctly from a script line.
        /// </summary>
        [TestMethod]
        public void ExtractDelimiter()
        {
            string mySqlDelimiterChangeLine = "DELIMITER $$";
            Assert.AreEqual("$$", DbUpdaterLib.Utils.MySqlExtractDelimiter(mySqlDelimiterChangeLine));
            string mySqlDelimiterChangeLine2 = "DELIMITER ;";
            Assert.AreEqual(";", DbUpdaterLib.Utils.MySqlExtractDelimiter(mySqlDelimiterChangeLine2));
            string mySqlDelimiterChangeLine3 = "   DELIMITER    $$    ";
            Assert.AreEqual("$$", DbUpdaterLib.Utils.MySqlExtractDelimiter(mySqlDelimiterChangeLine3));
        }

        /// <summary>
        /// Tests if the script files are found and are in the correct order based on filename and subfolders.
        /// </summary>
        [TestMethod]
        [DeploymentItem(@"DbUpdaterLibTests\TestScripts")]
        public void FindTestScripts()
        {
            // Check if folder exists (should copy folder from test project)
            var scriptFolder = "TestScripts";
            DirectoryInfo objDirectoryInfo = new (scriptFolder);
            Assert.IsTrue(objDirectoryInfo.Exists);

            // Grab file info from folder
            FileInfo[] scriptFiles = DbUpdaterLib.Utils.FindSqlFiles(scriptFolder);
            Assert.AreEqual(4, scriptFiles.Length);

            // Make sure they are in the correct order
            Assert.AreEqual("01_tables.sql", scriptFiles[0].Name);
            Assert.AreEqual("02_views.sql", scriptFiles[1].Name);
            Assert.AreEqual("03_procedures.sql", scriptFiles[2].Name);
            Assert.AreEqual("01_data.sql", scriptFiles[3].Name); // This is in a subfolder so it should be last
        }

        /// <summary>
        /// Strips a multiline comment inside a single line.
        /// </summary>
        [TestMethod]
        public void StripComments()
        {
            string theLine = "This /* comment */ is correct!";
            Assert.AreEqual("This  is correct!", DbUpdaterLib.Utils.StripCommentsFromLine(theLine));
            string theLine2 = "This /* comment *//* another comment*/ is correct!";
            Assert.AreEqual("This  is correct!", DbUpdaterLib.Utils.StripCommentsFromLine(theLine2));
        }
    }
}
