using YaLlegaNucleo.Datos;
using YaLlegaNucleo.Modelo;
using YaLlegaNucleo.Modelo.Cartas;
using YaLlegaNucleo.Modelo.Cobros;
using YaLlegaNucleo.Modelo.Excepciones;
using YaLlegaNucleo.Modelo.Revision;

namespace YaLlegaNucleo.Negocio;

// Es la puerta de entrada para la consola.
// Arma la orden, revisa que se pueda pedir, consigue las cartas y guarda la orden.
public class ServicioDeOrdenes
{
    public const decimal TarifaBaseDeEnvio = 600;

    private readonly IRevisorDeOrden _primerRevisor;
    private readonly IOrigenDeCartas _cartas;
    private readonly IArchivoDeOrdenes _archivo;

    private Orden? _orden;

    public ServicioDeOrdenes(IRevisorDeOrden primerRevisor, IOrigenDeCartas cartas, IArchivoDeOrdenes archivo)
    {
        if (primerRevisor == null) throw new ArgumentNullException(nameof(primerRevisor));
        if (cartas == null) throw new ArgumentNullException(nameof(cartas));
        if (archivo == null) throw new ArgumentNullException(nameof(archivo));

        _primerRevisor = primerRevisor;
        _cartas = cartas;
        _archivo = archivo;
    }

    public int IntentosRechazados { get; private set; }

    public string CartaServidaDesde { get; private set; } = "(todavia no se pidio ninguna carta)";

    public bool HayOrden { get { return _orden != null; } }

    // La consola pide la carta igual que siempre: no sabe de donde sale.
    public Carta? PedirCarta(int numero)
    {
        Carta? carta = _cartas.DameCarta(numero);

        if (carta != null) CartaServidaDesde = _cartas.UltimoOrigen;

        return carta;
    }

    // Arranca una orden nueva.
    public void NuevaOrden(int numero, Cliente cliente, NivelDeDemanda demanda)
    {
        CalculadoraDeEnvio envio = new CalculadoraDeEnvio(TarifaBaseDeEnvio, new CobroPorDemanda(), demanda);

        _orden = new Orden(numero, cliente, envio);

        IntentosRechazados = 0;
        CartaServidaDesde = "(todavia no se pidio ninguna carta)";
    }

    // Suma un articulo. Primero pasa por la cadena de revision.
    public void Sumar(int codigoDeArticulo, int unidades)
    {
        Orden orden = OrdenActual();

        RevisionDeOrden revision = new RevisionDeOrden(orden.Cliente, codigoDeArticulo);

        try
        {
            _primerRevisor.Revisar(revision);
        }
        catch (Exception ex) when (ex is AccesoNoPermitidoException || ex is ArticuloDesconocidoException)
        {
            // Lo cuento, porque despues hay que guardar los intentos rechazados.
            IntentosRechazados++;

            throw;
        }

        Articulo? articulo = revision.Articulo;

        if (articulo == null) throw new AccesoNoPermitidoException("No se pudo identificar el articulo");

        orden.Sumar(articulo, unidades);
    }

    public void Sacar(int codigoDeArticulo)
    {
        Orden orden = OrdenActual();

        Articulo? articulo = null;

        foreach (LineaDeOrden linea in orden.Lineas)
        {
            if (linea.Articulo.Codigo == codigoDeArticulo) articulo = linea.Articulo;
        }

        if (articulo == null) throw new ArticuloNoEstaEnLaOrdenException("codigo " + codigoDeArticulo);

        orden.Sacar(articulo);
    }

    // El sistema mide la demanda de la zona y avisa.
    public void AvisarDemanda(NivelDeDemanda demanda)
    {
        OrdenActual().Envio.CambiarDemanda(demanda);
    }

    // El operador del local aprieta el boton de promocion.
    public void PonerPromocion()
    {
        OrdenActual().Envio.CambiarModalidad(new CobroPromocional());
    }

    // Y lo vuelve a sacar.
    public void SacarPromocion()
    {
        OrdenActual().Envio.CambiarModalidad(new CobroPorDemanda());
    }

    // Cuando el cliente termina, la orden se cierra y se guarda.
    public OrdenRegistrada CerrarYGuardar()
    {
        Orden orden = OrdenActual();

        orden.Cerrar();

        OrdenRegistrada registrada = new OrdenRegistrada();

        registrada.Fecha = DateTime.Now;
        registrada.Cliente = orden.Cliente.Nombre;
        registrada.Articulos = ArmarLosArticulos(orden);
        registrada.CartaServidaDesde = CartaServidaDesde;
        registrada.IntentosRechazados = IntentosRechazados;

        foreach (string tarifa in orden.Envio.Aplicadas)
        {
            registrada.TarifasDeEnvio.Add(tarifa);
        }

        _archivo.Guardar(registrada);

        return registrada;
    }

    // La orden que se esta armando ahora.
    public Orden OrdenActual()
    {
        if (_orden == null) throw new InvalidOperationException("Todavia no hay una orden abierta");

        return _orden;
    }

    public string RutaDelArchivo() { return _archivo.Ruta; }

    public IReadOnlyList<string> LeerArchivo() { return _archivo.LeerLineas(); }

    public IReadOnlyList<OrdenRegistrada> Guardadas() { return _archivo.Guardadas; }

    // Paso los articulos a un solo texto, para guardarlo.
    private static string ArmarLosArticulos(Orden orden)
    {
        List<string> partes = new List<string>();

        foreach (LineaDeOrden linea in orden.Lineas)
        {
            partes.Add(linea.Unidades + "x " + linea.Articulo.Detalle);
        }

        if (partes.Count == 0) return "(sin articulos)";

        return string.Join(", ", partes);
    }
}
