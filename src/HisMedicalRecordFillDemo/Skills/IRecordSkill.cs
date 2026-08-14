using HisMedicalRecordFillDemo.Models;

namespace HisMedicalRecordFillDemo.Skills;

public interface IRecordSkill
{
    string Id { get; }

    Task<SkillContext> BuildContextAsync(string hisData, CancellationToken cancellationToken);
}
