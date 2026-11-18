using Microsoft.AspNetCore.Mvc;
using StudentGpa.API.Common;
using StudentGpa.Application.Common;
using StudentGpa.Application.DTOs;
using StudentGpa.Application.Interfaces;

namespace StudentGpa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    // GET: api/courses?page=1&pageSize=20&search=CS
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] ListQuery query,
        CancellationToken ct)
    {
        var result = await _courseService.GetPagedAsync(query, ct);

        return result.ToActionResult();
    }

    // GET: api/courses/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken ct)
    {
        var result = await _courseService.GetByIdAsync(id, ct);

        return result.ToActionResult();
    }

    // POST: api/courses
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] SaveCourseRequest request,
        CancellationToken ct)
    {
        var result = await _courseService.CreateAsync(request, ct);

        if (result.IsFailure)
            return result.ToActionResult();

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.Id },
            result.Value);
    }

    // PUT: api/courses/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] SaveCourseRequest request,
        CancellationToken ct)
    {
        var result = await _courseService.UpdateAsync(id, request, ct);

        return result.ToActionResult();
    }

    // DELETE: api/courses/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken ct)
    {
        var result = await _courseService.DeleteAsync(id, ct);

        return result.ToActionResult();
    }
}
