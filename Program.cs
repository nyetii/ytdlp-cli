﻿using System.Reflection;
using CliFx;

namespace youtube
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Dependency.InitializeAsync();
            
            try
            {
                await new CliApplicationBuilder()
                    .SetExecutableName(Assembly.GetEntryAssembly().Location)
                    .AddCommandsFromThisAssembly()
                    .Build()
                    .RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadKey();
            }
        }
    }
}