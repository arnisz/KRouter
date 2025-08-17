using System;
using System.IO;
using KRouter.Cli;

// Debug-Tool um zu sehen was tatsächlich generiert wird
var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
var dsnPath = Path.Combine(projectRoot, "samples", "boards", "example.dsn");
var dsn = File.ReadAllText(dsnPath);

Console.WriteLine("=== DSN Content (erste 500 Zeichen) ===");
Console.WriteLine(dsn.Substring(0, Math.Min(500, dsn.Length)));

var ses = SpectraSessionGenerator.FromDsn(dsn, "example");

Console.WriteLine("\n=== Generierte SES Content ===");
Console.WriteLine(ses);
