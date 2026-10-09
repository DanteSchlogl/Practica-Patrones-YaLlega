namespace YaLlegaNucleo.Modelo.Cobros;

// Formas de cobrar el envio.
// Se pueden cambiar mientras la orden sigue abierta, por eso son objetos aparte.
public interface IModalidadDeCobro
{
    string Nombre { get; }

    decimal Calcular(decimal tarifaBase, NivelDeDemanda demanda);
}
