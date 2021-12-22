// <copyright file="UpdaterTests.cs" company="IdectusAB">
// Copyright (c) IdectusAB. All rights reserved.
// </copyright>

namespace DbUpdaterLibTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
