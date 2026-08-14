using HisMedicalRecordFillDemo.Exceptions;
using HisMedicalRecordFillDemo.Services;

namespace HisMedicalRecordFillDemo.Tests;

public sealed class HisFixtureProviderTests
{
    [Fact]
    public async Task ReadAsync_存在的样例_返回原始HIS拼接文本()
    {
        var provider = new HisFixtureProvider(new TestHostEnvironment(AppContext.BaseDirectory));

        var result = await provider.ReadAsync("CASE-001", "TEST20260811001", CancellationToken.None);

        Assert.Contains("【病案】", result);
        Assert.Contains("【医嘱】", result);
        Assert.Contains("【费用】", result);
        Assert.Contains("【报告】", result);
    }

    [Fact]
    public async Task ReadAsync_非法标识符_拒绝路径穿越()
    {
        var provider = new HisFixtureProvider(new TestHostEnvironment(AppContext.BaseDirectory));

        await Assert.ThrowsAsync<FixtureNotFoundException>(() => provider.ReadAsync("../CASE-001", "TEST", CancellationToken.None));
    }
}
