using EveryDaily.Core.ControllerBases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EveryDaily.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController(IMediator mediator)
    : CustomControllerBase
    {
        /// <summary>
        /// Kullanıcının bildirimlerini getirir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var result = await mediator.Send(new GetNotificationsQuery { });
            return CreateActionResultInstance(result);
        }
    }
}
