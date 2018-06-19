using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fclp;
using Scrapper;

namespace ConsoleClient
{
    class Program
    {
        private string outputFile = "output.txt";

        static void Main(string[] args)
        {
            var app = new Program();
            app.Start(args);

            Console.ReadKey();
        }

        private void Start(string[] args)
        {
            var arguments = ParseCommandLineParameters(args);
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
        /// <param name="args"></param>
        /// <returns></returns>
        private CommandLineArg ParseCommandLineParameters(string[] args)
        {
            var argumentParser = new FluentCommandLineParser<CommandLineArg>();

            // Url Argument
            argumentParser.Setup(a => a.Url)
                .As('u', "url")
                .Required()
                .UseForOrphanArguments()
                .WithDescription("Url to explore links from");

            // Maximum depth Argument
            argumentParser.Setup(a => a.MaxDepth)
                .As('d', "max-depth")
                .Required()
                .WithDescription("Maximum depth");

            // Ignore Url File Location Argument
            argumentParser.Setup(a => a.IgnoreUrlFile)
                .As('i', "ignore-file")
                .SetDefault("IgnoreSitesList.txt")
                .WithDescription("Ignore Url File Location");

            // Verbose Argument
            argumentParser.Setup(a => a.Verbose)
                .As('v', "verbose")
                .SetDefault(false)
                .WithDescription("Verbose");

            var result = argumentParser.Parse(args);
            if (result.HasErrors)
            {
                Console.WriteLine("Invalid input parameters.");
                return null;
            }

            return argumentParser.Object;
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
