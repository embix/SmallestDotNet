// -----------------------------------------------------------------------
// <copyright file="program.cs" company="Scott Hanselman, Michael Sarchet and Friends">
//     Copyright © Scott Hanselman, Michael Sarchet and Friends 2012. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace CheckForDotNet45
{
    using System;
    using System.Diagnostics;

    using Microsoft.Win32;

    /// <summary>
    /// Class containing all application logic
    /// </summary>
    public class Program
    {
        #region Fields

        /// <summary>Site URL with release key parameter</summary>
        private const string SiteWithReleaseKey = "http://smallestdotnet.com/?releaseKey={0}";

        /// <summary>Site URL with version parameter</summary>
        private const string SiteWithVersion = "http://smallestdotnet.com/?realversion={0}";

        #endregion Fields

        #region Methods

        /// <summary>
        /// Main entry point for program.
        /// </summary>
        public static void Main()
        {
            try
            {
                Console.Write("Checking .NET version...");

                String releaseKeyText = null;
                int releaseKey = 0;
                string version = null;
                var isNet45OrNewer = IsNet45OrNewer();
                if (isNet45OrNewer)
                {
                    releaseKey = GetDotnetReleaseKeyFromRegistry();
                    releaseKeyText = releaseKey.ToString();
                }
                else
                {
                    version = Environment.Version.ToString();
                }

                string url = string.IsNullOrEmpty(version)
                        ? string.Format(SiteWithReleaseKey, releaseKey)
                        : string.Format(SiteWithVersion, version);
                try
                {
                    var unknown = "unknown";
                    Console.WriteLine();
                    Console.WriteLine($"Your current .NET version is: {version??unknown}");
                    Console.WriteLine($"Your current .NET 4.5+ release key is: {releaseKeyText??unknown}");
                    bool isGuess;
                    var versionGuess = Helpers.GetDotnetVersionFromReleaseKey(releaseKey, out isGuess);
                    var justguessing = " (just guessing)";
                    Console.WriteLine();
                    Console.WriteLine($"                    version: {versionGuess}{(isGuess?justguessing:String.Empty)}");
                    var guess=global::Helpers.GetUpdateInformation("", "", releaseKey);
                    Console.WriteLine(guess.Text);
                    Process.Start(url);
                }
                catch (Exception)
                {
                    Console.WriteLine("\nApplication was unable to launch browser to go to {0}", url);
                    if (version == null)
                    {
                        Console.WriteLine("Please browse to the URL to get version information.");
                    }

                    Console.ReadLine(); // Pause
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred:");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();
                Console.WriteLine("Please log an issue including the error information at https://github.com/shanselman/SmallestDotNet/issues/");
                Console.ReadLine(); // Pause
            }
        }

        /// <summary>
        /// Gets the .Net release key from registry "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\".
        /// </summary>
        /// <returns>Release key or -1 if not found</returns>
        private static int GetDotnetReleaseKeyFromRegistry()
        {
            int releaseKey = -1;

            // Based on http://msdn.microsoft.com/en-us/library/hh925568%28v=vs.110%29.aspx#net_d
            using (
                RegistryKey ndpKey =
                    RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                        .OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
            {
                if (ndpKey != null)
                {
                    object releaseKeyValue = ndpKey.GetValue("Release") ?? "-1";
                    int.TryParse(releaseKeyValue.ToString(), out releaseKey);
                }

                return releaseKey;
            }
        }

        /// <summary>
        /// Determines whether we have .net 4.5 or newer.
        /// </summary>
        /// <returns>true if we have .net 4.5 or newer</returns>
        private static bool IsNet45OrNewer()
        {
            // Class "ReflectionContext" exists in .NET 4.5 .
            return Type.GetType("System.Reflection.ReflectionContext", false) != null;
        }

        #endregion Methods
    }
}