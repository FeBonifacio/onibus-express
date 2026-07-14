namespace OnibusExpress.Domain.Exceptions;

public sealed class InvalidDocumentException : DomainException
{
    public override string ErrorCode => "DOCUMENT_INVALID";
    public string Input { get; }
    public DocumentType Type { get; }

    public InvalidDocumentException(string input, DocumentType type)
        : base($"Documento invalido: '{input}'.")
    {
        Input = input;
        Type = type;
    }
}
