namespace YaLlegaNucleo.Modelo;

// Una carta del local: la lista de articulos de una seccion.
// Es lo que baja del servidor remoto y lo que el cache guarda en el dispositivo.
public class Carta
{
    private readonly List<Articulo> _articulos = new List<Articulo>();

    public Carta(int numero, string titulo)
    {
        Numero = numero;
        Titulo = titulo;
    }

    public int Numero { get; private set; }

    public string Titulo { get; private set; }

    public IReadOnlyList<Articulo> Articulos { get { return _articulos; } }

    public int CuantosArticulos { get { return _articulos.Count; } }

    public Carta Sumar(Articulo articulo)
    {
        if (articulo == null) throw new ArgumentNullException(nameof(articulo));

        _articulos.Add(articulo);

        return this;
    }
}
