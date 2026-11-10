using CellStore.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<Db>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromHours(1);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSession();
app.MapControllers();

app.Run();