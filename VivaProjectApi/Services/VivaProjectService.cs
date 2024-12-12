using Newtonsoft.Json;
using System.Net.Http.Json;
using VivaProjectApi.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Immutable;

namespace VivaProjectApi.Services
{
    public class VivaProjectService
    {
        VivaProjectRepository _repository;
        public VivaProjectService(VivaProjectRepository repository) {
            _repository = repository;
        }
        public virtual int GetSecondLargestNumber(IEnumerable<int> listOfInt)
        {
            int highestNum = listOfInt.FirstOrDefault();
            int secondHighestNum = listOfInt.FirstOrDefault();

            if (listOfInt != null)
            {
                foreach (var num in listOfInt)
                {
                    if (num > highestNum)
                    {
                        secondHighestNum = highestNum;
                        highestNum = num;
                    }
                    else if (num > secondHighestNum && num < highestNum)
                    {
                        secondHighestNum = num;
                    }
                }
            }
            return secondHighestNum;
        }
        public async Task<List<RestCountriesModel>> GetCountries()
        {
            var apiEndpoint = "https://restcountries.com/v3.1/all?fields=name,capital,borders,cca2";
            //Countries data exists on cache?
            if (_repository.GetDataFromCache("countrieslist") == null)
            {
                //Countries data exists on Databse?
                var countries = await _repository.GetDataAsync();
                if (countries.Count() == 0)
                {
                    using (var httpClient = new HttpClient())
                    {
                        try
                        {
                            //Make the HTTP call
                            HttpResponseMessage response = await httpClient.GetAsync(apiEndpoint);
                            response.EnsureSuccessStatusCode();
                            string responseBody = await response.Content.ReadAsStringAsync();
                            var res = JsonConvert.DeserializeObject<List<RestCountriesModel>>(responseBody);
                            if (res.ToList().Count() != 0)
                            {
                                //Save them to Cache and Database
                                _repository.InsertData(res);
                                _repository.StoreDataInCache("countrieslist", res);
                            }
                            //Return data to the client
                            return res;
                        }
                        catch(HttpRequestException ex)
                        {
                            //Returns Internal Server Error
                            /*return $"Request error: {ex.Message}";*/
                        }
                    }
                }
                else{
                    _repository.StoreDataInCache("countrieslist",countries);
                    return countries;

                }
            }
            else
            {
                var countries = _repository.GetDataFromCache("countrieslist");
                return countries;

            }
                return new List<RestCountriesModel>();
        }
        public class Name
        {
            public string? Common { get; set; }
            public string? Official { get; set; }
        }

        public class RestCountriesModel
        {
            public Name? Name { get; set; }
            public string? Cca2 { get; set; }
            public List<string>? Capital { get; set; }
            public List<string>? Borders { get; set; }
        }
        public class CountryDbModel
        {
            public string? Cca2 { get; set; }
            public string? Name { get; set; }
            public string? Borders { get; set; }
            public string? Capitals { get; set; }
        }

    }
}
