using EveryDaily.Application.Services.ControllerCommands.Rank.Commands;
using EveryDaily.Application.Services.ControllerCommands.Rank.Queries;
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
    public class RankController(IMediator mediator) : CustomControllerBase
    {
        /// <summary>
        /// Kullanıcının login işlemine göre rank güncellenmesini başlatır.
        /// </summary>
        [HttpPost("login-rank")]
        public async Task<IActionResult> UpdateRankOnLogin()
        {
            var result = await mediator.Send(new LoginRankCommand());
            return CreateActionResultInstance(result);
        }

        /// <summary>
        /// Kullanıcının mevcut sezon rank'ı, toplam XP'si, son kazandığı XP ve neden kazandığı bilgisini döndürür.
        /// </summary>
        [HttpGet("current-rank")]
        public async Task<IActionResult> GetCurrentRank([FromQuery] GetUserRankQuery query)
        {
            var result = await mediator.Send(query);
            return CreateActionResultInstance(result);
        }
    }
}
