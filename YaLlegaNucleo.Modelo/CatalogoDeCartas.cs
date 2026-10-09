namespace YaLlegaNucleo.Modelo;

// Todas las cartas que tiene el local.
public class CatalogoDeCartas
{
    private readonly List<Carta> _cartas = new List<Carta>();

    public IReadOnlyList<Carta> Cartas { get { return _cartas; } }

    public void Sumar(Carta carta)
    {
        if (carta == null) throw new ArgumentNullException(nameof(carta));

        _cartas.Add(carta);
    }

    // Busco una carta por su numero.
    public Carta? DameCarta(int numero)
    {
        foreach (Carta carta in _cartas)
        {
            if (carta.Numero == numero) return carta;
        }

        return null;
    }

    // Busco un articulo en todas las cartas.
    public Articulo? DameArticulo(int codigo)
    {
        foreach (Carta carta in _cartas)
        {
            foreach (Articulo articulo in carta.Articulos)
            {
                if (articulo.Codigo == codigo) return articulo;
            }
        }

        return null;
    }

    // Todos los articulos juntos, para el listado de marketing.
    public IReadOnlyList<Articulo> TodosLosArticulos()
    {
        List<Articulo> todos = new List<Articulo>();

        foreach (Carta carta in _cartas)
        {
            foreach (Articulo articulo in carta.Articulos)
            {
                todos.Add(articulo);
            }
        }

        return todos;
    }
}
