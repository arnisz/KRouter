using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace KRouter.Tests.Cli
{
    public class RouteCliIntegrationTest
    {
        [Fact]
        public async Task KRouter_CLI_Routes_KiCad_DSN()
        {
            // Arrange
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var cliProject = Path.Combine(projectRoot, "src", "Cli", "Cli.csproj");
            var dsnPath = Path.Combine(projectRoot, "tests", "TESTUS.dsn");
            var tempDir = Path.Combine(Path.GetTempPath(), "krouter_cli_test", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var sesPath = Path.Combine(tempDir, "out.ses");

            // Build CLI (Release, single file, so der Test auch unabhängig läuft)
            var build = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"publish \"{cliProject}\" -c Release -o \"{tempDir}\" --nologo",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });
            build!.WaitForExit();
            Assert.Equal(0, build.ExitCode);

            var exe = Path.Combine(tempDir, "krouter.exe");
            if (!File.Exists(exe)) exe = Path.Combine(tempDir, "krouter"); // Linux/macOS
            Assert.True(File.Exists(exe), $"CLI-Binary nicht gefunden: {exe}");

            // Act
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = $"route --in \"{dsnPath}\" --out \"{sesPath}\" --profile Fast",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };
            proc.Start();
            string stdout = await proc.StandardOutput.ReadToEndAsync();
            string stderr = await proc.StandardError.ReadToEndAsync();
            proc.WaitForExit(60_000); // 60s Timeout

            // Assert
            Assert.True(proc.ExitCode == 0, $"CLI exit code: {proc.ExitCode}\nSTDOUT:\n{stdout}\nSTDERR:\n{stderr}");
            Assert.True(File.Exists(sesPath), $"SES-File nicht erzeugt: {sesPath}\nSTDOUT:\n{stdout}\nSTDERR:\n{stderr}");
            var sesContent = await File.ReadAllTextAsync(sesPath);
            Assert.Contains("(routing", sesContent);
            Assert.Contains("(network", sesContent);
        }
    }
}

