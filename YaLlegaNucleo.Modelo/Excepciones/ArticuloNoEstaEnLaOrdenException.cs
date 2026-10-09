namespace YaLlegaNucleo.Modelo.Excepciones;

// Se lanza si se quiere sacar de la orden un articulo que no esta.
public class ArticuloNoEstaEnLaOrdenException : Exception
{
    public ArticuloNoEstaEnLaOrdenException(string detalle)
        : base("El articulo '" + detalle + "' no esta en la orden")
    {
        Detalle = detalle;
    }

    public string Detalle { get; private set; }
}
