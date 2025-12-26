namespace Arda9Template.Api.Application.Tenants.Queries.GetTenantById;

public class GetTenantByIdResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public int ClientsCount { get; set; }
    public int UsersCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
