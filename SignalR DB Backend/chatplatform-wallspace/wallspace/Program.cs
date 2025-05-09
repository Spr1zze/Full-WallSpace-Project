using wallspace;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSignalR();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        "server=localhost;port=3307;database=chatdb;user=root;password=root",//3307 = Sergey 3306 = Nicolai
        new MySqlServerVersion(new Version(11, 7)) 
    )
);


var app = builder.Build();



app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapHub<ChatHub>("/chathub");


app.Run();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    )
);
