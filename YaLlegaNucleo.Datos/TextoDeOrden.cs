using YaLlegaNucleo.Modelo;

namespace YaLlegaNucleo.Datos;

// Arma el texto de la linea que se guarda por cada orden.
// Uso punto y coma entre los datos, con el nombre del dato adelante.
public class TextoDeOrden
{
    public string Armar(OrdenRegistrada orden)
    {
        return "ORDEN; "
            + orden.Fecha.ToString("dd/MM/yyyy HH:mm:ss")
            + "; cliente: " + orden.Cliente
            + "; articulos: " + orden.Articulos
            + "; tarifas: " + string.Join(" >> ", orden.TarifasDeEnvio)
            + "; carta: " + orden.CartaServidaDesde
            + "; rechazados: " + orden.IntentosRechazados;
    }
}
