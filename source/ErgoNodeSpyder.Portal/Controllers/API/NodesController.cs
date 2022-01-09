using System.Linq;
using System.Threading.Tasks;
using ErgoNodeSharp.Data;
using ErgoNodeSharp.Models.Responses;
using ErgoNodeSharp.Models.Responses.NodeSpyder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ErgoNodeSpyder.Portal.Controllers.API
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class NodesController : ControllerBase
    {
        private readonly INodeReportingRepository repository;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<NodesController> logger;

        public NodesController(INodeReportingRepository nodeReportingRepository, IHttpContextAccessor httpContextAccessor, ILogger<NodesController> logger)
        {
            repository = nodeReportingRepository;
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        [Route("node-infos")]
        public async Task<IActionResult> GetNodeInfos()
        {
            JsonApiResponse<NodeInfo> response = new JsonApiResponse<NodeInfo>();
            if (httpContextAccessor.HttpContext != null)
            {
                logger.LogDebug("Received GetNodeInfos request from " + httpContextAccessor.HttpContext.Request.Host);
                response.Links.First = httpContextAccessor.HttpContext.Request.GetEncodedUrl();
                response.Links.Self = httpContextAccessor.HttpContext.Request.GetEncodedUrl();
            }

            response.Data = await repository.GetNodeInfos();
            response.Meta.TotalRecords = response.Data.Count();

            return Ok(response);
        }

        [Route("versions")]
        public async Task<IActionResult> Versions()
        {
            JsonApiResponse<StringValuePair> response = CreateJsonApiResponse();

            response.Data = await repository.GetVersionCount();
            response.Meta.TotalRecords = response.Data.Count();

            return Ok(response);
        }

        [Route("geo/continents")]
        public async Task<IActionResult> Continents()
        {
            JsonApiResponse<GeoSummary> response = CreateJsonGeoApiResponse();

            response.Data = await repository.GetContinentCount();
            response.Meta.TotalRecords = response.Data.Count();

            return Ok(response);
        }

        [Route("geo/countries")]
        public async Task<IActionResult> Countries(int count = 5)
        {
            if (count <= 0)
            {
                ErrorMessage errorMessage = new ErrorMessage();
                errorMessage.Status = "400";
                errorMessage.Title = "Invalid request";
                errorMessage.Detail = "Count parameter must be greater than zero";
                ErrorResponse errorResponse = new ErrorResponse(errorMessage);
                return BadRequest(errorResponse);
            }
            JsonApiResponse<GeoSummary> response = CreateJsonGeoApiResponse();

            response.Data = await repository.GetCountryCount(count);
            response.Meta.TotalRecords = response.Data.Count();

            return Ok(response);
        }

        [Route("geo/regions")]
        public async Task<IActionResult> Regions(string countryCode, int count = 5)
        {
            if (string.IsNullOrEmpty(countryCode) || countryCode.Length != 2)
            {
                ErrorMessage errorMessage = new ErrorMessage();
                errorMessage.Status = "400";
                errorMessage.Title = "Invalid request";
                errorMessage.Detail = "Invalid country code supplied";
                ErrorResponse errorResponse = new ErrorResponse(errorMessage);
                return BadRequest(errorResponse);
            }

            if (count <= 0)
            {
                ErrorMessage errorMessage = new ErrorMessage();
                errorMessage.Status = "400";
                errorMessage.Title = "Invalid request";
                errorMessage.Detail = "Count parameter must be greater than zero";
                ErrorResponse errorResponse = new ErrorResponse(errorMessage);
                return BadRequest(errorResponse);
            }

            JsonApiResponse<GeoSummary> response = CreateJsonGeoApiResponse();

            response.Data = await repository.GetRegionCount(countryCode, count);
            response.Meta.TotalRecords = response.Data.Count();

            return Ok(response);
        }

        [Route("geo/isps")]
        public async Task<IActionResult> Isps(int count = 10)
        {
            if (count <= 0)
            {
                ErrorMessage errorMessage = new ErrorMessage();
                errorMessage.Status = "400";
                errorMessage.Title = "Invalid request";
                errorMessage.Detail = "Count parameter must be greater than zero";
                ErrorResponse errorResponse = new ErrorResponse(errorMessage);
                return BadRequest(errorResponse);
            }

            JsonApiResponse<StringValuePair> response = CreateJsonApiResponse();

            response.Data = await repository.GetIspCount(count);
            response.Meta.TotalRecords = response.Data.Count();

            return Ok(response);
        }

        [Route("state-types")]
        public async Task<IActionResult> StateTypes()
        {
            JsonApiResponse<StringValuePair> response = CreateJsonApiResponse();

            response.Data = await repository.GetStateTypeCount();
            response.Meta.TotalRecords = response.Data.Count();

            return Ok(response);
        }

        [Route("verifying")]
        public async Task<IActionResult> Verifying()
        {
            JsonApiResponse<BoolValuePair> response = new JsonApiResponse<BoolValuePair>();

            if (httpContextAccessor.HttpContext != null)
            {
                logger.LogDebug("Received GetNodeInfos request from " + httpContextAccessor.HttpContext.Request.Host);
                response.Links.First = httpContextAccessor.HttpContext.Request.GetEncodedUrl();
                response.Links.Self = httpContextAccessor.HttpContext.Request.GetEncodedUrl();
            }

            response.Data = await repository.GetVerifyingCount();
            response.Meta.TotalRecords = response.Data.Count();

            return Ok(response);
        }

        [Route("blocks-kept")]
        public async Task<IActionResult> BlocksKept()
        {
            JsonApiResponse<IntValuePair> response = new JsonApiResponse<IntValuePair>();

            if (httpContextAccessor.HttpContext != null)
            {
                logger.LogDebug("Received BlocksKept request from " + httpContextAccessor.HttpContext.Request.Host);
                response.Links.First = httpContextAccessor.HttpContext.Request.GetEncodedUrl();
                response.Links.Self = httpContextAccessor.HttpContext.Request.GetEncodedUrl();
            }

            response.Data = await repository.GetBlocksKeptCount();
            response.Meta.TotalRecords = response.Data.Count();

            return Ok(response);
        }

        [Route("daily-count")]
        public async Task<IActionResult> DailyCount(int count = 10)
        {
            if (count <= 0)
            {
                ErrorMessage errorMessage = new ErrorMessage();
                errorMessage.Status = "400";
                errorMessage.Title = "Invalid request";
                errorMessage.Detail = "Count parameter must be greater than zero";
                ErrorResponse errorResponse = new ErrorResponse(errorMessage);
                return BadRequest(errorResponse);
            }

            JsonApiResponse<StringValuePair> response = CreateJsonApiResponse();

            response.Data = await repository.GetDailyCount(count);
            response.Meta.TotalRecords = response.Data.Count();

            return Ok(response);
        }

        [Route("weekly-count")]
        public async Task<IActionResult> WeeklyCount(int count = 12)
        {
            if (count <= 0)
            {
                ErrorMessage errorMessage = new ErrorMessage();
                errorMessage.Status = "400";
                errorMessage.Title = "Invalid request";
                errorMessage.Detail = "Count parameter must be greater than zero";
                ErrorResponse errorResponse = new ErrorResponse(errorMessage);
                return BadRequest(errorResponse);
            }

            JsonApiResponse<StringValuePair> response = CreateJsonApiResponse();

            response.Data = await repository.GetWeekCount(count);
            response.Meta.TotalRecords = response.Data.Count();

            return Ok(response);
        }

        [Route("monthly-count")]
        public async Task<IActionResult> MonthlyCount(int count = 6)
        {
            if (count <= 0)
            {
                ErrorMessage errorMessage = new ErrorMessage();
                errorMessage.Status = "400";
                errorMessage.Title = "Invalid request";
                errorMessage.Detail = "Count parameter must be greater than zero";
                ErrorResponse errorResponse = new ErrorResponse(errorMessage);
                return BadRequest(errorResponse);
            }

            JsonApiResponse<StringValuePair> response = CreateJsonApiResponse();

            response.Data = await repository.GetMonthCount(count);
            response.Meta.TotalRecords = response.Data.Count();

            return Ok(response);
        }

        private JsonApiResponse<StringValuePair> CreateJsonApiResponse()
        {
            JsonApiResponse<StringValuePair> response = new JsonApiResponse<StringValuePair>();
            if (httpContextAccessor.HttpContext == null) return response;

            logger.LogDebug("Received GetNodeInfos request from " + httpContextAccessor.HttpContext.Request.Host);
            response.Links.First = httpContextAccessor.HttpContext.Request.GetEncodedUrl();
            response.Links.Self = httpContextAccessor.HttpContext.Request.GetEncodedUrl();

            return response;
        }

        private JsonApiResponse<GeoSummary> CreateJsonGeoApiResponse()
        {
            JsonApiResponse<GeoSummary> response = new JsonApiResponse<GeoSummary>();
            if (httpContextAccessor.HttpContext == null) return response;

            logger.LogDebug("Received GetNodeInfos request from " + httpContextAccessor.HttpContext.Request.Host);
            response.Links.First = httpContextAccessor.HttpContext.Request.GetEncodedUrl();
            response.Links.Self = httpContextAccessor.HttpContext.Request.GetEncodedUrl();

            return response;
        }
    }
}
