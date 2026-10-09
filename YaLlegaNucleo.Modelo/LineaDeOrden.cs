namespace YaLlegaNucleo.Modelo;

// Una linea de la orden: un articulo y cuantas unidades.
public class LineaDeOrden
{
    public LineaDeOrden(Articulo articulo, int unidades)
    {
        if (articulo == null) throw new ArgumentNullException(nameof(articulo));

        Articulo = articulo;
        Unidades = unidades;
    }

    public Articulo Articulo { get; private set; }

    public int Unidades { get; set; }

    public decimal Importe { get { return Articulo.Importe * Unidades; } }
}
