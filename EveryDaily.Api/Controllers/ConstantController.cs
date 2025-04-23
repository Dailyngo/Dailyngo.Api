using EveryDaily.Application.Services.ControllerCommands.Constants.Querie;
using EveryDaily.Application.Services.ControllerCommands.Constants.Query;
using EveryDaily.Core.ControllerBases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EveryDaily.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConstantController : CustomControllerBase
    {
        private readonly IMediator _mediator;

        public ConstantController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("departments/{facultyid}")]
        public async Task<IActionResult> GetDepartments(Guid facultyid)
        {
            var result = await _mediator.Send(new GetDepartmentQuery() { FacultyId = facultyid});
            return CreateActionResultInstance(result);
        }

        [HttpGet("faculties/{universityid}")]
        public async Task<IActionResult> GetFaculties(Guid universityid)
        {
            var result = await _mediator.Send(new GetFacultyQuery() { UniversityId = universityid });
            return CreateActionResultInstance(result);
        }
        [HttpGet("universities")]
        public async Task<IActionResult> GetUniversities()
        {
            var result = await _mediator.Send(new GetUniversityQuery());
            return CreateActionResultInstance(result);
        }

    }
}
