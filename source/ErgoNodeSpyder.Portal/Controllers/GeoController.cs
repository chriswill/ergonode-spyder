using System.Threading.Tasks;
using ErgoNodeSharp.Data;
using ErgoNodeSharp.Models.Responses;
using ErgoNodeSpyder.Portal.Models.Geo;
using Microsoft.AspNetCore.Mvc;

namespace ErgoNodeSpyder.Portal.Controllers
{
    [Route("geo")]
    public class GeoController : Controller
    {

        private readonly INodeReportingRepository reportingRepository;

        public GeoController(INodeReportingRepository reportingRepository)
        {
            this.reportingRepository = reportingRepository;
        }


        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("country/{countryCode:length(2)}")]
        public async Task<IActionResult> Country(string countryCode)
        {
            CountryViewModel model = new CountryViewModel();

            string countyName = await reportingRepository.GetCountyName(countryCode);

            if (string.IsNullOrEmpty(countyName))
            {
                ErrorMessage errorMessage = new ErrorMessage();
                errorMessage.Status = "400";
                errorMessage.Title = "Invalid request";
                errorMessage.Detail = "Invalid country code supplied";
                ErrorResponse errorResponse = new ErrorResponse(errorMessage);
                return BadRequest(errorResponse);
            }

            model.CountryCode = countryCode;
            model.CountryName = countyName;
            return View(model);
        }
    }
}
