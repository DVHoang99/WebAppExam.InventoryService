using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// 1. Lấy cấu hình từ appsettings
var mongoSettings = builder.Configuration.GetSection("MongoDbSettings");
var connectionString = mongoSettings["ConnectionString"];
var databaseName = mongoSettings["DatabaseName"];

// 2. Đăng ký IMongoClient (Singleton là chuẩn nhất cho Client)
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));

// 3. Đăng ký IMongoDatabase (Lấy từ Client đã đăng ký ở trên)
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(databaseName);
});

builder.Services.AddMediatR(cfg =>
{
    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
    cfg.RegisterServicesFromAssemblies(assemblies);
});
builder.Services.AddScoped<IWareHouseRepository, WareHouseRepository>();

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
