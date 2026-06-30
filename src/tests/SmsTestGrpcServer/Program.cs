using SmsTestGrpcServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<SmsTestService>();
app.MapGet("/", () => "gRPC client.");

app.Run();