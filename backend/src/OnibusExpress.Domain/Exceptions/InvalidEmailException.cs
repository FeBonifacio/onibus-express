namespace OnibusExpress.Domain.Exceptions;

public sealed class InvalidEmailException : DomainException
{
    public override string ErrorCode => "EMAIL_INVALID";
    public string Input { get; }

    public InvalidEmailException(string input)
        : base($"E-mail invalido: '{input}'.") => Input = input;
}
