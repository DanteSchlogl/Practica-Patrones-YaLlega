namespace YaLlegaNucleo.Modelo.Cartas;

// Cache de cartas.
// Guarda en el dispositivo las ultimas cartas consultadas para no volver a bajarlas
// del servidor.
// La pantalla le pide las cartas a este objeto igual que al servidor, asi que no se
// entera de donde salieron.
public class CacheDeCartas : IOrigenDeCartas
{
    private readonly IOrigenDeCartas _servidor;
    private readonly int _cuantasGuarda;

    private readonly Dictionary<int, Carta> _guardadas = new Dictionary<int, Carta>();

    // Las cartas ordenadas de la mas vieja a la mas nueva.
    private readonly List<int> _ordenDeUso = new List<int>();

    public CacheDeCartas(IOrigenDeCartas servidor, int cuantasGuarda)
    {
        if (servidor == null) throw new ArgumentNullException(nameof(servidor));
        if (cuantasGuarda <= 0) throw new ArgumentException("El cache tiene que guardar al menos una carta");

        _servidor = servidor;
        _cuantasGuarda = cuantasGuarda;
    }

    public string UltimoOrigen { get; private set; } = "";

    // Cuantas veces se pudo usar algo que ya estaba guardado.
    public int VecesQueSirvioElCache { get; private set; }

    public int CartasGuardadas { get { return _guardadas.Count; } }

    public int CuantasGuarda { get { return _cuantasGuarda; } }

    public Carta? DameCarta(int numero)
    {
        // Si ya la tengo guardada, la devuelvo sin molestar al servidor.
        if (_guardadas.ContainsKey(numero))
        {
            VecesQueSirvioElCache++;
            UltimoOrigen = "Dispositivo";
            PasarAlFinal(numero);

            return _guardadas[numero];
        }

        Carta? carta = _servidor.DameCarta(numero);

        if (carta != null) Guardar(numero, carta);

        UltimoOrigen = "Servidor";

        return carta;
    }

    // Guardo la carta y, si me pase del limite, saco la mas vieja.
    private void Guardar(int numero, Carta carta)
    {
        _guardadas[numero] = carta;
        PasarAlFinal(numero);

        while (_ordenDeUso.Count > _cuantasGuarda)
        {
            int masVieja = _ordenDeUso[0];

            _ordenDeUso.RemoveAt(0);
            _guardadas.Remove(masVieja);
        }
    }

    // Muevo la carta al final de la lista, porque acaba de usarse.
    private void PasarAlFinal(int numero)
    {
        _ordenDeUso.Remove(numero);
        _ordenDeUso.Add(numero);
    }
}
