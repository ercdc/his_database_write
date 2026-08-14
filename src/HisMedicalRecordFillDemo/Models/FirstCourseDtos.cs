namespace HisMedicalRecordFillDemo.Models;

public sealed record GenerateFirstCourseRequest(string? PatientId, string? VisitId);

public sealed record GenerateFirstCourseResponse(bool Success, string OutputPath, string Message);

public sealed record ErrorResponse(bool Success, string Error);

public sealed record GeneratedXmlResult(string OutputPath, string Confirmation);
