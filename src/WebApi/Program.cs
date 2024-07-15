using Microsoft.EntityFrameworkCore;
using WebApi.Endpoints;
using WebApi.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<LibraryDbContext>(options => options.UseInMemoryDatabase("Library"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseBookEndpoints();

app.Run();

public partial class Program;
