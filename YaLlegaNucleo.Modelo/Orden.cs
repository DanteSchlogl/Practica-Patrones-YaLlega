using YaLlegaNucleo.Modelo.Excepciones;

namespace YaLlegaNucleo.Modelo;

// La orden que va armando el cliente.
// El costo de envio lo lleva aparte la CalculadoraDeEnvio, asi la orden se ocupa
// solo de los articulos.
public class Orden
{
    private readonly List<LineaDeOrden> _lineas = new List<LineaDeOrden>();

    public Orden(int numero, Cliente cliente, CalculadoraDeEnvio envio)
    {
        if (cliente == null) throw new ArgumentNullException(nameof(cliente));
        if (envio == null) throw new ArgumentNullException(nameof(envio));

        Numero = numero;
        Cliente = cliente;
        Envio = envio;
        Abierta = true;
    }

    public int Numero { get; private set; }

    public Cliente Cliente { get; private set; }

    public CalculadoraDeEnvio Envio { get; private set; }

    public bool Abierta { get; private set; }

    public IReadOnlyList<LineaDeOrden> Lineas { get { return _lineas; } }

    // Sumo un articulo. Si ya estaba en la orden, le agrego unidades.
    public void Sumar(Articulo articulo, int unidades)
    {
        if (articulo == null) throw new ArgumentNullException(nameof(articulo));
        if (unidades <= 0) throw new CantidadFueraDeRangoException(unidades);

        ControlarQueEsteAbierta();

        LineaDeOrden? linea = Buscar(articulo);

        if (linea == null)
        {
            _lineas.Add(new LineaDeOrden(articulo, unidades));
        }
        else
        {
            linea.Unidades = linea.Unidades + unidades;
        }
    }

    public void Sacar(Articulo articulo)
    {
        if (articulo == null) throw new ArgumentNullException(nameof(articulo));

        ControlarQueEsteAbierta();

        LineaDeOrden? linea = Buscar(articulo);

        if (linea == null) throw new ArticuloNoEstaEnLaOrdenException(articulo.Detalle);

        _lineas.Remove(linea);
    }

    public decimal ImporteDeArticulos()
    {
        decimal total = 0;

        foreach (LineaDeOrden linea in _lineas)
        {
            total = total + linea.Importe;
        }

        return total;
    }

    public decimal ImporteTotal()
    {
        return ImporteDeArticulos() + Envio.Cobrar();
    }

    // Cuando el cliente termina, la orden se cierra y no se toca mas.
    public void Cerrar()
    {
        Abierta = false;
    }

    // Me dice si el articulo ya esta en la orden.
    public bool Tiene(Articulo articulo)
    {
        return Buscar(articulo) != null;
    }

    private void ControlarQueEsteAbierta()
    {
        if (!Abierta) throw new OrdenYaCerradaException(Numero);
    }

    private LineaDeOrden? Buscar(Articulo articulo)
    {
        foreach (LineaDeOrden linea in _lineas)
        {
            if (linea.Articulo.Codigo == articulo.Codigo) return linea;
        }

        return null;
    }
}
