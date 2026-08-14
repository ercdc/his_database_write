using HisMedicalRecordFillDemo.Exceptions;
using HisMedicalRecordFillDemo.Models;
using HisMedicalRecordFillDemo.Services;
using Microsoft.AspNetCore.Mvc;

namespace HisMedicalRecordFillDemo.Controllers;

[ApiController]
[Route("api/first-course")]
public sealed class FirstCourseController(FirstCourseGenerationService generationService) : ControllerBase
{
    [HttpPost("generate")]
    [ProducesResponseType<GenerateFirstCourseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Generate([FromBody] GenerateFirstCourseRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PatientId) || string.IsNullOrWhiteSpace(request.VisitId))
            return BadRequest(new ErrorResponse(false, "patientId 和 visitId 均不能为空。"));

        try
        {
            var result = await generationService.GenerateAsync(request.PatientId, request.VisitId, cancellationToken);
            return Ok(new GenerateFirstCourseResponse(true, result.OutputPath, result.Confirmation));
        }
        catch (FixtureNotFoundException exception)
        {
            return NotFound(new ErrorResponse(false, exception.Message));
        }
        catch (Exception exception) when (exception is XmlValidationException or ToolCallingException)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse(false, exception.Message));
        }
    }
}
