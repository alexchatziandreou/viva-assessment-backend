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
            int? highestNum = null;
            int? secondHighestNum = null;

            if (listOfInt != null)
            {
                foreach (var num in listOfInt)
                {
                    if (highestNum == null || num > highestNum)
                    {
                        secondHighestNum = highestNum;d
                        highestNum = num;
                    }
                    else if (secondHighestNum == null || num > secondHighestNum && num < highestNum)
                    {
                        secondHighestNum = num;
                    }
                }
            }
            return secondHighestNum.Value;
        }
        public async Task<List<RestCountriesModel>> GetCountries()
        {
            var apiEndpoint = "https://restcountries.com/v3.1/all?fields=name,capital,borders,cca2";
            //Countries data exists on cache?
            if (_repository.GetDataFromCache("countrieslist")==null)
            {
                //Countries data exists on Databse?
                if (_repository.GetDataAsync()==null)
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
                            if (res.Count() != 0)
                            {
                                //Save them to Cache and Database
                                _repository.InsertDataAsync(res);
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
            }
            return new List<RestCountriesModel>();
        }
        public class CountryNameModel
        {
            public string common { get; set; }
        }
        public class RestCountriesModel
        {
            public string cca2 { get; set; }
            public CountryNameModel name { get; set; }
            public List<string> capital { get; set; }
            public List<string> borders { get; set; }
        }

    }
}
