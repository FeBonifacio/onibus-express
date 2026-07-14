namespace OnibusExpress.Domain.Common;

/// <summary>Anything that carries a stable, machine-readable error code (mapped to HTTP by the API).</summary>
public interface IHasErrorCode
{
    string ErrorCode { get; }
}
