using Microsoft.AspNetCore.Mvc;
using StudentGpa.API.Common;
using StudentGpa.Application.Interfaces;

namespace StudentGpa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GpaController : ControllerBase
{
    private readonly IGpaService _gpaService;

    public GpaController(IGpaService gpaService)
    {
        _gpaService = gpaService;
    }

    // GET: api/gpa/students/5/transcript
    [HttpGet("students/{studentId:int}/transcript")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTranscript(
        int studentId,
        CancellationToken ct)
    {
        var result = await _gpaService.GetTranscriptAsync(
            studentId,
            ct);

        return result.ToActionResult();
    }
}
