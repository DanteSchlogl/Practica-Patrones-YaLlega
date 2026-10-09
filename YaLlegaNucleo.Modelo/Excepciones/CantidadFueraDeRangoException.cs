namespace YaLlegaNucleo.Modelo.Excepciones;

// Se lanza si se pide una cantidad de unidades que no tiene sentido.
public class CantidadFueraDeRangoException : Exception
{
    public CantidadFueraDeRangoException(int unidades)
        : base("Las unidades tienen que ser mayor a cero. Me llego: " + unidades)
    {
        Unidades = unidades;
    }

    public int Unidades { get; private set; }
}
