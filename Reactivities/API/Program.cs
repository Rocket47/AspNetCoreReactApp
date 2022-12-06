using Persistence;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Application.Activities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(prop =>
{
    prop.AddPolicy("CorsPolicy", policy => {
        policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
    });
});

builder.Services.AddMediatR(typeof(List.Handler).Assembly);

var clientSecret = builder.Configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
builder.Services.AddDbContext<DataContext>(opt =>
{
    opt.UseSqlite(clientSecret);
} 
);

var app = builder.Build();

using var scope  = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedData(context);
}
catch (Exception ex) 
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured duting migration");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
