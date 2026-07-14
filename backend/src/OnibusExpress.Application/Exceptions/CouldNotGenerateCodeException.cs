namespace OnibusExpress.Application.Exceptions;

/// <summary>Ran out of attempts to generate a unique code (maps to HTTP 503).</summary>
public sealed class CouldNotGenerateCodeException : Exception, IHasErrorCode
{
    public string ErrorCode => "COULD_NOT_GENERATE_CODE";

    public CouldNotGenerateCodeException(int attempts)
        : base($"Nao foi possivel gerar codigo unico apos {attempts} tentativas.") { }
}
