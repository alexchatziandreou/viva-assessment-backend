using Dapper;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
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
            var cacheValue = DateTime.Now;

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10));

            _cache.Set(key, data, cacheEntryOptions);
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
        public async void InsertData(List<RestCountriesModel> data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                foreach (var row in data)
                {
                    var capitals = string.Join(",", row.Capital);
                    var borders = string.Join(",", row.Borders);
                    string countryName = row.Name?.Common ?? "Unknown";
                    string sqlQuery = @"INSERT INTO VIVA_COUNTRIES_TABLE (cca2,name , borders, capital) 
                                VALUES (@cca2,@name ,@borders, @capitals)";
                    await connection.ExecuteAsync(sqlQuery, new
                    {
                        cca2 = row.Cca2,
                        name = countryName,
                        borders,
                        capitals 
                    });
                }
            }
        }
        public async Task<List<RestCountriesModel>> GetDataAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sqlQuery = @"SELECT * FROM VIVA_COUNTRIES_TABLE";
                var data = await connection.QueryAsync<CountryDbModel>(sqlQuery);
                var mappedData = data.Select(country => new RestCountriesModel
                {
                    Cca2 = country.Cca2,
                    Name = new Name
                    {
                        Common = country.Name
                    },
                    Borders = string.IsNullOrEmpty(country.Borders) ? new List<string>() : country.Borders.Split(',').ToList(),
                    Capital = string.IsNullOrEmpty(country.Capitals) ? new List<string>() : country.Capitals.Split(',').ToList()
                }).ToList();
                return mappedData;
            }
        }


        #endregion
    }
}
