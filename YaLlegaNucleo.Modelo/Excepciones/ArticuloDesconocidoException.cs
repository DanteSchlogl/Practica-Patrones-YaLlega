namespace YaLlegaNucleo.Modelo.Excepciones;

// Se lanza si se pide un articulo que no figura en ninguna carta.
public class ArticuloDesconocidoException : Exception
{
    public ArticuloDesconocidoException(int codigo)
        : base("El articulo " + codigo + " no figura en el catalogo")
    {
        Codigo = codigo;
    }

    public int Codigo { get; private set; }
}
