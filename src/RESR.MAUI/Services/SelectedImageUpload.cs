namespace RESR.MAUI.Services;

public sealed record SelectedImageUpload(
    string FileName,
    string ContentType,
    byte[] Content,
    long Size);
