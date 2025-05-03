using Test_Models;
using Microsoft.Data.SqlClient;
namespace Test_Logic;

public class ExchangeService : IExchangeService
{
    private readonly string _connectionString;

    public ExchangeService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Currency> GetCurrencies()
    {
        var currencies = new List<Currency>();
        string sql = "SELECT Id, Name, Rate FROM Currency";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(sql, connection);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                currencies.Add(new Currency
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1),
                    Rate =(float)reader.GetDouble(2)
                });
            }
            reader.Close();
            return currencies;
        }
    }

    public IEnumerable<Country> GetCountries()
    {
        var countries = new List<Country>();
        string sql = "SELECT Id, Name FROM Country";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(sql, connection);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                //figure out the list and how to do it kinda
                countries.Add(new Country()
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1),
                });
            }

            reader.Close();
            return countries;
        }
    }

    public Currency? GetCurrencyByName(string name)
    {
        string sql = "SELECT * FROM Currency WHERE Name = @Name";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Name", name);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            if (!reader.Read()) return null;
            return new Currency
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                Rate = (float)reader.GetDouble(2)
            };
        }
    }

    public Country? GetCountryByName(string name)
    {
        string sql = "SELECT * FROM Country WHERE Name = @Name";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Name", name);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            if (!reader.Read()) return null;
            return new Country()
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
            };
        }
    }

    public IEnumerable<Currency> GetCurrenciesByCountryName(string countryName)
    {
        var currencies = new List<Currency>();
        const string sql = @"SELECT c.Id, c.Name, c.Rate
                             FROM Currency c
                             JOIN Currency_Country cc ON c.Id = cc.Currency_ID
                             JOIN Country co ON co.Id = cc.Country_ID
                             WHERE co.Name = @Name";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Name", countryName);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                currencies.Add(new Currency
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1),
                    Rate = (float)reader.GetDouble(2),
                });
            }
            return currencies;
        }
    }
    public Currency? GetDeviceById(string id)
    {
        string querystring = "SELECT * FROM Currency WHERE Id = @id";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(querystring, connection);
            command.Parameters.AddWithValue("@Id", id);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            if (!reader.Read()) return null;
            return new Currency()
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                Rate = (float)reader.GetDouble(2)
            };
        }
    }
    public IEnumerable<Country> GetCountriesByCurrencyName(string currencyName)
    {
        var countries = new List<Country>();
        const string sql = @"SELECT co.Id, co.Name
                                 FROM Country co
                                 JOIN Currency_Country cc ON co.Id = cc.Country_ID
                                 JOIN Currency c ON c.Id = cc.Currency_ID
                                 WHERE c.Name = @Name";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Name", currencyName);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                countries.Add(new Country
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1)
                });
            }
        }
        return countries;
    }

        public bool Create(Currency currency)
        {
            ValidateCountries(currency);
            int counter = 1;
            string newId;
            
            while (true)
            {
                newId = $"{"cur"}{counter}";
                if (GetDeviceById(newId) == null)
                    break;
                counter++;
            }
            currency.Id = newId;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                
                connection.Open();
                var existingCurrency = GetCurrencyByName(currency.Name);
                if (existingCurrency == null)
                {
                    string insertSql = "INSERT INTO Currency (Id, Name, Rate) VALUES (@Id, @Name, @Rate)";
                    using (SqlCommand command = new SqlCommand(insertSql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", currency.Id);
                        command.Parameters.AddWithValue("@Name", currency.Name);
                        command.Parameters.AddWithValue("@Rate", currency.Rate);
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    currency.Id = existingCurrency.Id;
                    string updateSql = "UPDATE Currency SET Rate = @Rate WHERE Id = @Id";
                    using (SqlCommand command = new SqlCommand(updateSql, connection))
                    {
                        command.Parameters.AddWithValue("@Rate", currency.Rate);
                        command.Parameters.AddWithValue("@Id", currency.Id);
                        command.ExecuteNonQuery();
                    }

                    string deleteSql = "DELETE FROM Currency_Country WHERE Currency_ID = @CurrencyId";
                    using (SqlCommand command = new SqlCommand(deleteSql, connection))
                    {
                        command.Parameters.AddWithValue("@CurrencyId", currency.Id);
                        command.ExecuteNonQuery();
                    }
                }

                string mapSql = "INSERT INTO Currency_Country (Country_ID, Currency_ID) VALUES (@CountryId, @CurrencyId)";
                foreach (var country in currency.Countries)
                {
                    using (SqlCommand command = new SqlCommand(mapSql, connection))
                    {
                        var existingCountry = GetCountryByName(country);
                        command.Parameters.AddWithValue("@CountryId", existingCountry.Id);
                        command.Parameters.AddWithValue("@CurrencyId", currency.Id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            return true;
        }
        private void ValidateCountries(Currency currency)
        {
            foreach (var country in currency.Countries)
            {
                if (GetCountryByName(country) == null)
                    throw new ArgumentException($"Country '{country}' does not exist.");
            }
        }

    }
