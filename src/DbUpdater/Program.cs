// <copyright file="Program.cs" company="IdectusAB">
// Copyright (c) IdectusAB. All rights reserved.
// </copyright>

namespace DbUpdater
{
    /// <summary>
    /// Main entry point for the solution.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            DbUpdaterLib.Updater updater = new ();

            if (updater.Init(args))
            {
                updater.Update();
            }

            updater.Cleanup();
        }
    }
}
