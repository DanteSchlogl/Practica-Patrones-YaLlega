namespace YaLlegaNucleo.Modelo.Cartas;

// El servidor de verdad, el que tiene todas las cartas.
public class RepositorioRemotoDeCartas : IOrigenDeCartas
{
    private readonly CatalogoDeCartas _catalogo;

    public RepositorioRemotoDeCartas(CatalogoDeCartas catalogo)
    {
        if (catalogo == null) throw new ArgumentNullException(nameof(catalogo));

        _catalogo = catalogo;
    }

    // Cuento las veces que me pidieron algo, para que se note cuando el cache lo evita.
    public int Consultas { get; private set; }

    public string UltimoOrigen { get { return "Servidor"; } }

    public Carta? DameCarta(int numero)
    {
        Consultas++;

        return _catalogo.DameCarta(numero);
    }
}
