namespace Curriculos.Api.Exceptions;

public class EmailDuplicadoException : Exception
{
    public EmailDuplicadoException() : base("Já existe um candidato com este e-mail.")
    {
    }
}
