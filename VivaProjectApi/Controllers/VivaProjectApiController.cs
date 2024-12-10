using Microsoft.AspNetCore.Mvc;
using VivaProjectApi.Services;
using static VivaProjectApi.Services.VivaProjectService;

namespace VivaProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VivaProjectApiController : ControllerBase
    {
        private VivaProjectService _service;

        public VivaProjectApiController(VivaProjectService service)
        {
            _service = service;
        }

        [HttpPost(Name = "GetSecondLargestNumber")]
        public int SecondLargestNumber([FromBody]RequestObj args)
        {
            var secondLargestNumber = _service.GetSecondLargestNumber(args.RequestArrayObj);
            return secondLargestNumber;
        }

        [HttpGet(Name = "GetCountries")]
        public Task<List<RestCountriesModel>> GetCountries()
        {
            var countries = _service.GetCountries();
            return countries;
        }
        public class RequestObj
        {
            public IEnumerable<int> RequestArrayObj { get; set; }
        }
    }
}