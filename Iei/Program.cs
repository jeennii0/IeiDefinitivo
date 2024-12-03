using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Iei.Wrappers;
using Iei.Repository;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<IeiContext>(options =>
           options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddScoped<MonumentoService>();

        var app = builder.Build();

        // Si estamos en desarrollo, habilitamos Swagger
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        // Habilitamos los controllers
        app.MapControllers();

        // Ejecutamos la aplicación
        app.Run();
    }
}
