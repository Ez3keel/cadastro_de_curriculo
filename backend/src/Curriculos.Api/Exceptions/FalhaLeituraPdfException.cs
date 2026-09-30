namespace Curriculos.Api.Exceptions;

public class FalhaLeituraPdfException : Exception
{
    public FalhaLeituraPdfException() : base("Não foi possível ler o texto do PDF. Preencha os dados manualmente.")
    {
    }
}
