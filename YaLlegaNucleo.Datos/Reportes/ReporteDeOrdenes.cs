using YaLlegaNucleo.Modelo;

namespace YaLlegaNucleo.Datos.Reportes;

// Ejemplo de reporte con columnas propias.
// Sirve para mostrar que se puede cambiar el formato sin tocar la rutina de escritura.
public class ReporteDeOrdenes : ReporteDeListado<OrdenRegistrada>
{
    protected override string Encabezado()
    {
        return "FECHA; CLIENTE; ARTICULOS; TARIFAS DE ENVIO; CARTA SERVIDA DESDE; RECHAZADOS";
    }

    protected override string Renglon(OrdenRegistrada dato)
    {
        return dato.Fecha.ToString("dd/MM/yyyy HH:mm:ss") + "; "
            + dato.Cliente + "; "
            + dato.Articulos + "; "
            + string.Join(" >> ", dato.TarifasDeEnvio) + "; "
            + dato.CartaServidaDesde + "; "
            + dato.IntentosRechazados;
    }
}
