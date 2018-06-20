using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scrapper;

namespace ConsoleClient
{
    class Program
    {
        private const string OutputFile = "output.txt";
        private const string InputFile = "input.txt";

        static void Main()
        {
            var app = new Program();
            app.Start();

            Console.ReadKey();
        }

        private void Start()
        {
            var arguments = ParseInput(InputFile);
            if (arguments == null)
                return;
            
            // Parse and Filter-out unnecessary links
            var manipulator = new Manipulator(arguments);
            var uniqeLinks = manipulator.Parse();

            // Print Links
            PrintOutResults(uniqeLinks, arguments.Verbose);
        }

        private DataModel ParseInput(string inputFilePath)
        {
            var arguments = new DataModel();

            try
            {
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
                            var ignoreUrlFile = line.Substring(8);
                            if (string.IsNullOrEmpty(ignoreUrlFile))
                            {
                                ignoreUrlFile = "IgnoreSitesList.txt";
                            }
                            arguments.IgnoreUrls = GetIgnoreSitesList(ignoreUrlFile);
                        }
                        else if (line.StartsWith("-verbose"))
                        {
                            arguments.Verbose = true;
                        }
                        else if (line.StartsWith("-parseSameDomain"))
                        {
                            arguments.ParseSameDomainLinks = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            
            return arguments;
        }

        /// <summary>
        /// Read list of sites to ignore from file
        /// </summary>
        private IEnumerable<string> GetIgnoreSitesList(string filePath)
        {
            if (filePath == null)
                return new List<string>();

            var ignore = new List<string>();
            try
            {
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
                Console.WriteLine("The ignore sites list file could not be read:");
                Console.WriteLine(e.Message);
            }

            return ignore;
        }

        /// <summary>
        /// Print out results to output file
        /// </summary>
        private void PrintOutResults(IEnumerable<string> links, bool isVerbose)
        {
            try
            {
                Console.WriteLine($"\n\nFound Sites: ({links.Count()})");
                using (var sw = new StreamWriter(OutputFile))
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
