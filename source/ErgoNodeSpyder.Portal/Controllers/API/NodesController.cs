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
        private readonly INodeInfoRepository nodeInfoRepository;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<NodesController> logger;

        public NodesController(INodeInfoRepository nodeInfoRepository, IHttpContextAccessor httpContextAccessor, ILogger<NodesController> logger)
        {
            this.nodeInfoRepository = nodeInfoRepository;
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
        }
    }
}
