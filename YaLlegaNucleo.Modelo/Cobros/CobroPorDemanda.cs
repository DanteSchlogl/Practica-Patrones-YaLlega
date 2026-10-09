namespace YaLlegaNucleo.Modelo.Cobros;

// Modo normal: el envio sale de como venga la demanda en la zona.
public class CobroPorDemanda : IModalidadDeCobro
{
    private const decimal RecargoParaDemandaMedia = 1.25m;
    private const decimal RecargoParaDemandaAlta = 1.60m;

    public string Nombre { get { return "Cobro por demanda"; } }

    public decimal Calcular(decimal tarifaBase, NivelDeDemanda demanda)
    {
        if (demanda == NivelDeDemanda.Media) return tarifaBase * RecargoParaDemandaMedia;
        if (demanda == NivelDeDemanda.Alta) return tarifaBase * RecargoParaDemandaAlta;

        return tarifaBase;
    }
}
