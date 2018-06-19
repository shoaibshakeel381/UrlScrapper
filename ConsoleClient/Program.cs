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
        private string inputFile = "input.txt";

        static void Main()
        {
            var app = new Program();
            app.Start();

            Console.ReadKey();
        }

        private void Start()
        {
            var arguments = ParseInput(inputFile);
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
        private CommandLineArg ParseInput(string inputFilePath)
        {
            var arguments = new CommandLineArg();

            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using (var sr = new StreamReader(inputFilePath))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.StartsWith("//") || line.Length == 0)
                            continue;

                        if (line.StartsWith("-url"))
                        {
                            arguments.Url = line.Substring(5);
                            if (string.IsNullOrEmpty(arguments.Url))
                            {
                                throw new InvalidOperationException("Invalid value for URL parameter");
                            }
                        }
                        else if (line.StartsWith("-depth"))
                        {
                            arguments.MaxDepth = int.Parse(line.Substring(7));
                        }
                        else if (line.StartsWith("-ignore"))
                        {
                            arguments.IgnoreUrlFile = line.Substring(8);
                            if (string.IsNullOrEmpty(arguments.IgnoreUrlFile))
                            {
                                arguments.IgnoreUrlFile = "IgnoreSitesList.txt";
                            }
                        }
                        else if (line.StartsWith("-verbose"))
                        {
                            arguments.Verbose = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine(e.Message);
            }
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
                Console.WriteLine("The ignore sites list file could not be read:");
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
                        sw.WriteLine($"0.0.0.0 ${link}");

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
