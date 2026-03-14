namespace RESR.Core.Controllers.Resources;

public sealed record ResourceFileUpload(
    string OriginalName,
    string MimeType,
    int Size,
    byte[] Content
);
