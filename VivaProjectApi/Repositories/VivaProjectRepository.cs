using Dapper;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using static VivaProjectApi.Services.VivaProjectService;

namespace VivaProjectApi.Repositories
{
    public class VivaProjectRepository
    {
        private readonly IMemoryCache _cache;
        private readonly string _connectionString = "Server=localhost;Database=master;Trusted_Connection=True;";

        public VivaProjectRepository(IMemoryCache cache)
        {
            _cache = cache;
        }


        #region Cache Memory
        public void StoreDataInCache(string key,List<RestCountriesModel> data)
        { 
            _cache.Set(key , data, TimeSpan.FromMinutes(10));
        }
        public List<RestCountriesModel> GetDataFromCache(string key)
        {
            if (_cache.TryGetValue(key, out List<RestCountriesModel> data))
            {
                return data;
            }

            return null;
        }
        #endregion
        #region Database
        public async void InsertDataAsync(List<RestCountriesModel> data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                foreach (var row in data)
                {
                    await connection.OpenAsync();
                    var capitals = string.Join(",", row.capital);
                    var borders = string.Join(",", row.borders);
                    string countryName = row.name.common.ToString();
                    string sqlQuery = @"INSERT INTO VIVA_COUNTRIES (CountryName, Borders, Capitals, CountryId) 
                                VALUES (@CountryName, @Borders, @Capitals, @CountryId)";
                    await connection.ExecuteAsync(sqlQuery, new
                    {
                        CountryId = row.cca2,
                        CountryName = countryName,
                        Borders = borders,
                        Capitals = capitals
                    });
                }
            }
        }
        public List<RestCountriesModel> GetDataAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.OpenAsync();
                string sqlQuery = @"SELECT * FROM VIVA_COUNTRIES";
                return connection.ExecuteAsync(sqlQuery);
            }
        }
        #endregion
    }
}
