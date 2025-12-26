using Arda9Template.Api.Models;

namespace Arda9Template.Api.Application.Folders.Commands.CreateFolder;

public class CreateFolderResponse
{
    public FolderModel? Folder { get; set; }
    public string Message { get; set; } = string.Empty;
}