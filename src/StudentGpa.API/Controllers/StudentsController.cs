using Microsoft.AspNetCore.Mvc;
using StudentGpa.API.Common;
using StudentGpa.Application.DTOs;
using StudentGpa.Application.Common;
using StudentGpa.Application.Interfaces;

namespace StudentGpa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    // GET: api/students?page=1&pageSize=20&search=ahmed
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] ListQuery query,
        CancellationToken ct)
    {
        var result = await _studentService.GetPagedAsync(query, ct);

        return result.ToActionResult();
    }

    // GET: api/students/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken ct)
    {
        var result = await _studentService.GetByIdAsync(id, ct);

        return result.ToActionResult();
    }

    // POST: api/students
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] SaveStudentRequest request,
        CancellationToken ct)
    {
        var result = await _studentService.CreateAsync(request, ct);

        if (result.IsFailure)
            return result.ToActionResult();

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value.Id },
            result.Value);
    }

    // PUT: api/students/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] SaveStudentRequest request,
        CancellationToken ct)
    {
        var result = await _studentService.UpdateAsync(id, request, ct);

        return result.ToActionResult();
    }

    // DELETE: api/students/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken ct)
    {
        var result = await _studentService.DeleteAsync(id, ct);

        return result.ToActionResult();
    }
}
