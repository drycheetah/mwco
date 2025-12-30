using MWCO.Server;
using MWCO.Shared;

Console.WriteLine("=================================");
Console.WriteLine("  MWCO Server - My Winter Car Online");
Console.WriteLine("=================================");
Console.WriteLine();

// Parse command line args
int port = NetworkConfig.DefaultPort;
if (args.Length > 0 && int.TryParse(args[0], out int parsedPort))
{
    port = parsedPort;
}

var server = new UdpServer(port);

// Handle Ctrl+C gracefully
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\n[MWCO Server] Shutting down...");
    server.Stop();
};

try
{
    await server.StartAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"[MWCO Server] Fatal error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
