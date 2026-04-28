using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using PCRemote.Service;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "HH:mm:ss ";
});

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "PCRemoteService";
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();