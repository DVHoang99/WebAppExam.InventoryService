
using Microsoft.Extensions.DependencyInjection;
using KafkaFlow;
using WebAppExam.InventoryService.API;
using WebAppExam.InventoryService.API.Services;
using WebAppExam.InventoryService.API.Services.Grpc;
using WebAppExam.InventoryService.Application;
using WebAppExam.InventoryService.Infrastructure;
using Serilog;
using Hangfire;
using Hangfire.States;
using WebAppExam.InventoryService.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration));

// Add services from each layer
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.MapGrpcService<InventoryGrpcService>();
app.MapGrpcService<WarehouseGrpcService>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Setup Hangfire Dashboard (Monitoring only)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() }
});

var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

app.MapControllers();

app.Run();
