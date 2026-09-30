namespace Curriculos.Api.Exceptions;

public class ArquivoInvalidoException : Exception
{
    public ArquivoInvalidoException() : base("Arquivo inválido. Envie um PDF de até 5 MB.")
    {
    }
}
