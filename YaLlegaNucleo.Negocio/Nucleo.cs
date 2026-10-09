using YaLlegaNucleo.Modelo.Cartas;

namespace YaLlegaNucleo.Negocio;

// Junta todo lo que armo el ArmadorDelSistema.
// La consola usa esto y ve los contadores del cache sin tener que crear objetos de
// las otras capas.
public class Nucleo
{
    private readonly RepositorioRemotoDeCartas _remoto;
    private readonly CacheDeCartas _cache;

    public Nucleo(ServicioDeOrdenes ordenes, ServicioDeReportes reportes,
        RepositorioRemotoDeCartas remoto, CacheDeCartas cache)
    {
        Ordenes = ordenes;
        Reportes = reportes;
        _remoto = remoto;
        _cache = cache;
    }

    public ServicioDeOrdenes Ordenes { get; private set; }

    public ServicioDeReportes Reportes { get; private set; }

    // Contadores para ver como esta trabajando el cache.
    public int ConsultasAlServidor { get { return _remoto.Consultas; } }

    public int VecesQueSirvioElCache { get { return _cache.VecesQueSirvioElCache; } }

    public int CartasGuardadas { get { return _cache.CartasGuardadas; } }

    public int CuantasGuarda { get { return _cache.CuantasGuarda; } }
}
