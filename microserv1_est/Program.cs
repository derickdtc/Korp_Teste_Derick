using DotNetEnv;
using Microserv1Est.Data;
using Microserv1Est.DTOs;
using Microserv1Est.Middleware;
using Microserv1Est.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("ESTOQUE_DB_CONNECTION")
    ?? throw new InvalidOperationException("A variável ESTOQUE_DB_CONNECTION deve ser configurada no arquivo .env ou no ambiente.");

builder.Services.AddDbContext<EstoqueDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var message = context.ModelState.Values.SelectMany(value => value.Errors).Select(error => error.ErrorMessage)
            .FirstOrDefault(errorMessage => !string.IsNullOrWhiteSpace(errorMessage)) ?? "Dados inválidos.";
        return new BadRequestObjectResult(new ErrorResponseDto(StatusCodes.Status400BadRequest, message));
    };
});
builder.Services.AddCors(options => options.AddPolicy("AngularDevelopment", policy =>
    policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AngularDevelopment");
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
