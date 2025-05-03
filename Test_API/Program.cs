using Test_Logic;
using Test_Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("UniversityDatabase");
builder.Services.AddTransient<IExchangeService, ExchangeService>(
    _ => new ExchangeService(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/currency", (Currency currency, IExchangeService exchangeService) =>
{
    try
    {
        var success = exchangeService.Create(currency);
        return success ? Results.Created($"/api/currency/{currency.Name}", currency) : Results.BadRequest();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});


app.MapGet("/api/search/{term}", (string term, IExchangeService exchangeService) =>
{
    var country = exchangeService.GetCountryByName(term);
    if (country is not null)
    {
        var currencies = exchangeService.GetCurrenciesByCountryName(term);
        var result = new
        {
            Name = country.Name,
            Currencies = currencies.Select(c => new { c.Name, c.Rate })
        };
        return Results.Ok(result);
    }
    var currencyEntity = exchangeService.GetCurrencyByName(term);
    if (currencyEntity is not null)
    {
        var countries = exchangeService.GetCountriesByCurrencyName(term)
            .Select(c => new { c.Id, c.Name });
        return Results.Ok(countries);
    }
    return Results.NotFound($"No country or currency found with name '{term}'.");
});

app.Run();