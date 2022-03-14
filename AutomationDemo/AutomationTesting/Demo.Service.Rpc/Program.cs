using System.IO.Compression;
using Demo.Service.Rpc;
using ProtoBuf.Grpc.Server;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddLogging(configure => configure.AddConsole());

builder.Services.AddCodeFirstGrpc(config =>
{
    config.ResponseCompressionLevel = CompressionLevel.Optimal;
});


var loader = new Loader();

loader.Load(builder.Services,builder.Configuration);


var app = builder.Build();


app.MapGrpcService<UserManagement>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
