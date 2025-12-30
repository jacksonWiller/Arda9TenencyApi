using Arda9Tenant.Api.Models;

namespace Arda9Tenant.Api.Application.Files.Commands.UpdateFile;

public class UpdateFileResponse
{
    public FileMetadataModel? File { get; set; }
    public string Message { get; set; } = string.Empty;
}