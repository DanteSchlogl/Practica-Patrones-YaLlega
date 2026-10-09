namespace YaLlegaNucleo.Modelo.Excepciones;

// Se lanza cuando el sistema no permite hacer el pedido.
public class AccesoNoPermitidoException : Exception
{
    public AccesoNoPermitidoException(string motivo)
        : base(motivo)
    {
    }
}
