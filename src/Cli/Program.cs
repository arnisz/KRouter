using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KRouter.Core.Routing;
using KRouter.Core.Routing.Algorithms;
using KRouter.Core.DRC;
using KRouter.Core.DRC.Models;
using KRouter.Core.Geometry;

namespace KRouter.Cli
{
    /// <summary>
    /// Entry point for the KRouter command-line interface.
    /// Provides minimal routing commands for KiCad integration.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point. Configures available commands and options.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>Exit code.</returns>
        static async Task<int> Main(string[] args)
        {
            // Manuelle Initialisierung der Abhängigkeiten
            var ruleEngine = new RuleEngine();
            ruleEngine.LoadRules(new DesignRules());
            var routingAlgorithm = new AStarRouter();
            var routingEngine = new RoutingEngine(routingAlgorithm, ruleEngine);
            var routeService = new RouteService(routingEngine);

            var rootCommand = new RootCommand("KRouter - KiCad PCB Autorouter");

            // Route command
            var routeCommand = new Command("route", "Route a PCB design");

            var inputOption = new Option<FileInfo>(
                new[] { "--in", "-i" },
                "Input DSN file"
            ) { IsRequired = true };

            var outputOption = new Option<FileInfo>(
                new[] { "--out", "-o" },
                "Output SES file"
            ) { IsRequired = true };

            var profileOption = new Option<string>(
                new[] { "--profile", "-p" },
                getDefaultValue: () => "Balanced",
                "Routing profile (Fast, Balanced, Quality)"
            );

            var gridOption = new Option<long>(
                "--grid",
                getDefaultValue: () => 100_000,
                "Grid size (internal units)"
            );

            var layersOption = new Option<string[]>(
                "--layers",
                getDefaultValue: () => new[] { "F.Cu", "B.Cu" },
                "Layer list"
            );

            var jsonOption = new Option<bool>(
                "--json",
                description: "Machine readable summary to stdout"
            );

            routeCommand.AddOption(inputOption);
            routeCommand.AddOption(outputOption);
            routeCommand.AddOption(profileOption);
            routeCommand.AddOption(gridOption);
            routeCommand.AddOption(layersOption);
            routeCommand.AddOption(jsonOption);

            routeCommand.SetHandler(async (FileInfo input, FileInfo output, string profile, long grid, string[] layers, bool json) =>
            {
                try
                {
                    var result = await routeService.RouteAsync(new RouteRequest
                    {
                        InputFile = input,
                        OutputFile = output,
                        Profile = profile,
                        GridSize = grid,
                        Layers = layers,
                        Json = json
                    });

                    if (json)
                    {
                        Console.WriteLine(result.ToJson());
                    }
                    else
                    {
                        Console.WriteLine(result.HumanReadableReport);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error: {ex.Message}");
                }
            }, inputOption, outputOption, profileOption, gridOption, layersOption, jsonOption);

            rootCommand.AddCommand(routeCommand);

            // Version command
            var versionCommand = new Command("version", "Show version information");
            versionCommand.SetHandler(() =>
            {
                Console.WriteLine("KRouter v1.0.0");
                Console.WriteLine($".NET {Environment.Version} | MIT License");
            });
            rootCommand.AddCommand(versionCommand);

            // Help command
            var helpCommand = new Command("help", "Show help information");
            helpCommand.SetHandler(() =>
            {
                Console.WriteLine("Use --help on any command for details. Example: krouter route --help");
            });
            rootCommand.AddCommand(helpCommand);

            if (args.Length == 0)
            {
                args = new[] { "help" };
            }
            return await rootCommand.InvokeAsync(args);
        }

        /// <summary>
        /// Generates a KiCad-compatible SES file for demonstration.
        /// </summary>
        /// <param name="designName">Design name.</param>
        /// <param name="netCount">Number of nets.</param>
        /// <returns>SES file content.</returns>
        internal static string GenerateKiCadCompatibleSES(string designName, int netCount)
        {
            return $"(session \"{designName}.ses\"\n  (base_design \"{designName}.dsn\")\n  (placement\n    (resolution mm 1000000)\n  )\n  (routing\n    (resolution mm 1000000)\n    (parser\n      (host_cad \"KRouter\")\n      (host_version \"1.0.0\")\n    )\n    (library\n      (padstack \"Via[0-1]_600:300_um\"\n        (shape (circle F.Cu 600 0 0))\n        (shape (circle B.Cu 600 0 0))\n        (attach off)\n      )\n    )\n    (network\n      (net \"GND\"\n        (wire\n          (path F.Cu 250\n            50000 75000\n            80000 75000\n          )\n          (type protect)\n        )\n      )\n      (via \"Via[0-1]_600:300_um\" 100000 75000\n        (net \"VCC\")\n      )\n    )\n  )\n)";
        }
    }
}
