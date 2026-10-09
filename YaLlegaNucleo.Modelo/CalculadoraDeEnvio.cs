using YaLlegaNucleo.Modelo.Cobros;

namespace YaLlegaNucleo.Modelo;

// Se ocupa del costo de envio.
// Tiene puesta una modalidad de cobro y la puede cambiar mientras la orden sigue
// abierta, anotando todas las tarifas que se fueron aplicando.
public class CalculadoraDeEnvio
{
    private readonly List<string> _aplicadas = new List<string>();

    private IModalidadDeCobro _modalidad;

    public CalculadoraDeEnvio(decimal tarifaBase, IModalidadDeCobro modalidadInicial, NivelDeDemanda demandaInicial)
    {
        if (tarifaBase < 0) throw new ArgumentException("La tarifa base no puede ser negativa");
        if (modalidadInicial == null) throw new ArgumentNullException(nameof(modalidadInicial));

        TarifaBase = tarifaBase;
        _modalidad = modalidadInicial;
        Demanda = demandaInicial;

        Anotar();
    }

    public decimal TarifaBase { get; private set; }

    public NivelDeDemanda Demanda { get; private set; }

    public string ModalidadActual { get { return _modalidad.Nombre; } }

    // Todas las tarifas que se aplicaron, en orden.
    public IReadOnlyList<string> Aplicadas { get { return _aplicadas; } }

    // Cuanto hay que cobrar de envio con lo que hay puesto ahora.
    public decimal Cobrar()
    {
        return _modalidad.Calcular(TarifaBase, Demanda);
    }

    // El sistema mide la zona cada cierto tiempo y llama a esto.
    public void CambiarDemanda(NivelDeDemanda demanda)
    {
        Demanda = demanda;
        Anotar();
    }

    // El operador del local cambia el modo a mano.
    public void CambiarModalidad(IModalidadDeCobro modalidad)
    {
        if (modalidad == null) throw new ArgumentNullException(nameof(modalidad));

        _modalidad = modalidad;
        Anotar();
    }

    // Anoto la tarifa que quedo, salvo que sea igual a la ultima anotada.
    private void Anotar()
    {
        string texto = _modalidad.Nombre + " con demanda " + Demanda + " -> $" + Cobrar().ToString("0.00");

        if (_aplicadas.Count > 0 && _aplicadas[_aplicadas.Count - 1] == texto) return;

        _aplicadas.Add(texto);
    }
}
