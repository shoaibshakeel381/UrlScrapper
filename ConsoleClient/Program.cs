using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scrapper;

namespace ConsoleClient
{
    class Program
    {
        private string outputFile = "output.txt";

        static void Main()
        {
            var app = new Program();
            app.Start();

            Console.ReadKey();
        }

        private void Start()
        {
            var arguments = ParseCommandLineParameters();
            if (arguments == null)
                return;

            var ignore = GetIgnoreSitesList(arguments.IgnoreUrlFile);
            
            // Parse and Filter-out unnecessary links
            var manipulator = new Manipulator(arguments.Url, ignore, arguments.MaxDepth, arguments.Verbose);
            var uniqeLinks = manipulator.Parse();

            // Print Links
            PrintOutResults(uniqeLinks, arguments.Verbose);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private CommandLineArg ParseCommandLineParameters()
        {
            var arguments = new CommandLineArg();

            Console.Write("Provide URL to explore: ");
            arguments.Url = Console.ReadLine();
            if (arguments.Url == null || arguments.Url.Trim().Length == 0)
            {
                throw new InvalidOperationException();
            }

            Console.Write("Provide maximum depth to explore: ");
            arguments.MaxDepth = int.Parse(Console.ReadLine() ?? "1");

            Console.Write("Ignore Sites List file (skip to use default): ");
            arguments.IgnoreUrlFile = Console.ReadLine();
            if (arguments.IgnoreUrlFile != null && arguments.IgnoreUrlFile.Trim().Length == 0)
            {
                arguments.IgnoreUrlFile = "IgnoreSitesList.txt";
            }

            Console.Write("Verbose Mode (y/n): ");
            arguments.Verbose = Console.ReadKey().Key.Equals(ConsoleKey.Y);

            return arguments;
        }

        /// <summary>
        /// Read list of sites from ignore
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private IEnumerable<string> GetIgnoreSitesList(string filePath)
        {
            if (filePath == null)
                return new List<string>();

            var ignore = new List<string>();
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (var sr = new StreamReader(filePath))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("//") || line.Trim().Length == 0) continue;
                        ignore.Add(line.Trim());
                    }
                }
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            return ignore;
        }

        /// <summary>
        /// Print out results to output file
        /// </summary>
        /// <param name="links"></param>
        /// <param name="isVerbose"></param>
        private void PrintOutResults(IEnumerable<string> links, bool isVerbose)
        {
            try
            {
                Console.WriteLine($"\n\nFound Sites: ({links.Count()})");
                using (var sw = new StreamWriter(outputFile))
                {
                    foreach (var link in links)
                    {
                        sw.WriteLine(link);

                        if (isVerbose)
                        {
                            Console.WriteLine(link);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Output could not be written out.");
                Console.WriteLine(e.Message);
            }
        }
    }
}
