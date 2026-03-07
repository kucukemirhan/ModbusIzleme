using ModbusIzleme.Hubs;
using ModbusIzleme.Interfaces;
using ModbusIzleme.Models;
using ModbusIzleme.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Servis Kayıtları ---
builder.Services.AddControllers();

// Strongly-typed Modbus ayarlari (Options Pattern)
builder.Services.Configure<ModbusAyarlari>(
    builder.Configuration.GetSection("ModbusAyarlari"));

// SignalR
builder.Services.AddSignalR();

// Modbus okuyucu (Singleton: tek ortak TCP baglantisi)
builder.Services.AddSingleton<IModbusOkuyucu, ModbusTCP>();

// Son veri deposu (Singleton: BackgroundService yazar, Controller okur)
builder.Services.AddSingleton<ICihazVerisiDeposu, CihazVerisiDeposu>();

// Arka plan okuma servisi
builder.Services.AddHostedService<ModbusOkuyucuServis>();

// --- Uygulama Yapılandırma ---
var app = builder.Build();

// wwwroot → index.html statik sunucu
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<CihazHub>("/cihazHub");

app.Run();
