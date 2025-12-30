using Arda9Tenant.Api.Models;

namespace Arda9Tenant.Api.Application.Folders.Commands.CreateFolder;

public class CreateFolderResponse
{
    public FolderModel? Folder { get; set; }
    public string Message { get; set; } = string.Empty;
}