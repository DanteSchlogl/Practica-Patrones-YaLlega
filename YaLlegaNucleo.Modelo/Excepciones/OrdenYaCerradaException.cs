namespace YaLlegaNucleo.Modelo.Excepciones;

// Se lanza si se quiere tocar una orden que ya se cerro.
public class OrdenYaCerradaException : Exception
{
    public OrdenYaCerradaException(int numero)
        : base("La orden " + numero + " ya esta cerrada y no se puede modificar")
    {
        Numero = numero;
    }

    public int Numero { get; private set; }
}
