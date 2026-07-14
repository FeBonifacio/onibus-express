namespace OnibusExpress.Domain.Enums;

/// <summary>
/// Passenger document type. Only CPF today, modeled as an enum so the domain can
/// scale to other document types later without changing call sites.
/// </summary>
public enum DocumentType
{
    Cpf = 1,
}
