namespace HisMedicalRecordFillDemo.Services;

public interface IHisEncounterDataProvider
{
    Task<string> GetRawDataAsync(string patientId, string visitId, CancellationToken cancellationToken);
}
