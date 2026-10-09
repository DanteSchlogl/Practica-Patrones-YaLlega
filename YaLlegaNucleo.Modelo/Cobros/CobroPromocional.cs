namespace YaLlegaNucleo.Modelo.Cobros;

// Modo promocion: no se cobra el envio, sin importar como venga la demanda.
public class CobroPromocional : IModalidadDeCobro
{
    public string Nombre { get { return "Promocion sin cargo"; } }

    public decimal Calcular(decimal tarifaBase, NivelDeDemanda demanda)
    {
        return 0;
    }
}
