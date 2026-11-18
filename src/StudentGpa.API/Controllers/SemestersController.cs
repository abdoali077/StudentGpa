using Microsoft.AspNetCore.Mvc;
using StudentGpa.API.Common;
using StudentGpa.Application.DTOs;
using StudentGpa.Application.Interfaces;

namespace StudentGpa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;

    public SemestersController(ISemesterService semesterService)
    {
        _semesterService = semesterService;
    }

    // GET: api/semesters
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _semesterService.GetAllAsync(ct);

        return result.ToActionResult();
    }

    // GET: api/semesters/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken ct)
    {
        var result = await _semesterService.GetByIdAsync(id, ct);

        return result.ToActionResult();
    }

    // POST: api/semesters
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] SaveSemesterRequest request,
        CancellationToken ct)
    {
        var result = await _semesterService.CreateAsync(request, ct);

        if (result.IsFailure)
            return result.ToActionResult();

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.Id },
            result.Value);
    }

    // PUT: api/semesters/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] SaveSemesterRequest request,
        CancellationToken ct)
    {
        var result = await _semesterService.UpdateAsync(id, request, ct);

        return result.ToActionResult();
    }

    // DELETE: api/semesters/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken ct)
    {
        var result = await _semesterService.DeleteAsync(id, ct);

        return result.ToActionResult();
    }
}
