namespace NtbEvent.Infrastructure.Configuration;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string SuperAdminEmail { get; init; } = "superadmin@ntb.gov.np";

    public string SuperAdminPassword { get; init; } = "ChangeMe123!";

    public string SuperAdminFullName { get; init; } = "NTB Super Admin";

    public string SuperAdminDepartment { get; init; } = "System Administration";

    public bool SeedDemoEvents { get; init; } = true;
}
