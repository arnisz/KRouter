using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using KRouter.Core.Geometry;
using KRouter.Core.Routing;

namespace KRouter.Cli
{
    public class RouteRequest
    {
        public FileInfo InputFile { get; set; } = null!;
        public FileInfo OutputFile { get; set; } = null!;
        public string Profile { get; set; } = "Balanced";
        public long GridSize { get; set; } = 100_000;
        public string[] Layers { get; set; } = Array.Empty<string>();
        public bool Json { get; set; }
    }

    public class RouteResponse
    {
        public bool Success { get; set; }
        public int RoutedNetCount { get; set; }
        public int FailedNetCount { get; set; }
        public int TotalVias { get; set; }
        public double TotalLength { get; set; }
        public TimeSpan Elapsed { get; set; }
        public string HumanReadableReport { get; set; } = string.Empty;

        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(this, options);
        }
    }

    public interface IRouteService
    {
        Task<RouteResponse> RouteAsync(RouteRequest request, CancellationToken cancellationToken = default);
    }

    public class RouteService : IRouteService
    {
        private readonly RoutingEngine _engine;

        public RouteService(RoutingEngine engine)
        {
            _engine = engine;
        }

        public async Task<RouteResponse> RouteAsync(RouteRequest request, CancellationToken cancellationToken = default)
        {
            if (!request.InputFile.Exists)
                throw new FileNotFoundException("Input DSN nicht gefunden", request.InputFile.FullName);

            var dsnContent = await File.ReadAllTextAsync(request.InputFile.FullName, cancellationToken);
            var netCount = CountOccurrences(dsnContent, "(net ");
            if (netCount == 0) netCount = 1; // Minimal

            var nets = new List<Net>();
            for (int i = 0; i < netCount; i++)
            {
                nets.Add(new Net
                {
                    Name = $"NET_{i + 1}",
                    Priority = (netCount - i),
                    Pins = new List<Point2D>
                    {
                        new Point2D(0, i * 200_000),
                        new Point2D(1_000_000, i * 200_000)
                    }
                });
            }

            var bounds = new BoundingBox(new Point2D(-1_000_000, -1_000_000), new Point2D(20_000_000, 20_000_000));
            var layers = request.Layers.Length == 0 ? new List<string> { "F.Cu", "B.Cu" } : request.Layers.ToList();

            var result = await _engine.RouteBoard(nets, bounds, request.GridSize, layers, cancellationToken);

            // SES schreiben
            var designName = Path.GetFileNameWithoutExtension(request.InputFile.Name);
            var ses = SpectraSessionGenerator.FromDsn(dsnContent, designName);
            Directory.CreateDirectory(request.OutputFile.DirectoryName!);
            await File.WriteAllTextAsync(request.OutputFile.FullName, ses, cancellationToken);

            var response = new RouteResponse
            {
                Success = result.Success,
                RoutedNetCount = result.RoutedNets.Count,
                FailedNetCount = result.FailedNets.Count,
                TotalVias = result.TotalVias,
                TotalLength = result.TotalLength,
                Elapsed = result.ElapsedTime
            };

            response.HumanReadableReport = BuildReport(response);
            return response;
        }

        private static string BuildReport(RouteResponse r)
        {
            return $"=== Routing Report ===\nStatus: {(r.Success ? "SUCCESS" : "PARTIAL")}\nRouted Nets: {r.RoutedNetCount}\nFailed Nets: {r.FailedNetCount}\nVias: {r.TotalVias}\nTotal Length: {r.TotalLength:F2} internal units\nTime: {r.Elapsed.TotalSeconds:F2}s";
        }

        private static int CountOccurrences(string text, string pattern)
        {
            int count = 0; int idx = 0; while ((idx = text.IndexOf(pattern, idx, StringComparison.Ordinal)) != -1) { count++; idx += pattern.Length; }
            return count;
        }
    }
}

