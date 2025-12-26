using Arda9Template.Api.Models;

namespace Arda9Template.Api.Application.Files.Commands.UpdateFile;

public class UpdateFileResponse
{
    public FileMetadataModel? File { get; set; }
    public string Message { get; set; } = string.Empty;
}