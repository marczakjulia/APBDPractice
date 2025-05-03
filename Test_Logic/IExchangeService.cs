using Test_Models;

namespace Test_Logic;

public interface IExchangeService
{
    IEnumerable<Country> GetCountries();
    IEnumerable<Currency> GetCurrencies();
    Currency? GetCurrencyByName(string name);
    Country? GetCountryByName(string name);
    IEnumerable<Currency> GetCurrenciesByCountryName(string countryName);
    IEnumerable<Country> GetCountriesByCurrencyName(string currencyName);
    bool Create(Currency currency);
}