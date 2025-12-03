using BasketManagementAPI.Filters;
using BasketManagementAPI.Repositories;
using BasketManagementAPI.Services;
using BasketManagementAPI.Shipping;
using FluentValidation;
using FluentValidation.AspNetCore;
using BasketManagementAPI.Validators;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<KeyNotFoundExceptionFilter>();
builder.Services.AddControllers(options =>
    {
        options.Filters.AddService<KeyNotFoundExceptionFilter>();
    })
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<AddItemsRequestValidator>();

builder.Services.AddSingleton<IBasketRepository, SqlBasketRepository>();
builder.Services.AddSingleton<IDiscountDefinitionRepository, SqlDiscountDefinitionRepository>();
builder.Services.AddSingleton<IShippingCostRepository, SqlShippingCostRepository>();
builder.Services.AddSingleton<IShippingPolicy, ShippingPolicy>();
builder.Services.AddScoped<IBasketService, BasketService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();