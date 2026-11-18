using Microsoft.AspNetCore.Mvc;
using StudentGpa.API.Common;
using StudentGpa.Application.DTOs;
using StudentGpa.Application.Interfaces;

namespace StudentGpa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    // GET: api/enrollments/students/5
    [HttpGet("students/{studentId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByStudent(
        int studentId,
        CancellationToken ct)
    {
        var result = await _enrollmentService.GetByStudentAsync(
            studentId,
            ct);

        return result.ToActionResult();
    }

    // POST: api/enrollments
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateEnrollmentRequest request,
        CancellationToken ct)
    {
        var result = await _enrollmentService.CreateAsync(request, ct);

        if (result.IsFailure)
            return result.ToActionResult();

        return StatusCode(
            StatusCodes.Status201Created,
            result.Value);
    }

    // PUT: api/enrollments/10/grade
    [HttpPut("{enrollmentId:int}/grade")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateGrade(
        int enrollmentId,
        [FromBody] UpdateEnrollmentGradeRequest request,
        CancellationToken ct)
    {
        var result = await _enrollmentService.UpdateGradeAsync(
            enrollmentId,
            request,
            ct);

        return result.ToActionResult();
    }

    // DELETE: api/enrollments/10
    [HttpDelete("{enrollmentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int enrollmentId,
        CancellationToken ct)
    {
        var result = await _enrollmentService.DeleteAsync(
            enrollmentId,
            ct);

        return result.ToActionResult();
    }
}
