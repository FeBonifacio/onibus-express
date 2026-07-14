namespace OnibusExpress.Application.Common;

/// <summary>Resource not found (maps to HTTP 404 in Phase 3).</summary>
public sealed class NotFoundException : Exception
{
    public string ErrorCode => "NOT_FOUND";
    public string Resource { get; }

    public NotFoundException(string resource, object key)
        : base($"{resource} '{key}' nao encontrado.") => Resource = resource;
}
