using YaLlegaNucleo.Datos.Reportes;
using YaLlegaNucleo.Modelo;

namespace YaLlegaNucleo.Negocio;

// Exporta los listados a archivos de texto.
// Una sola llamada sirve para cualquier listado, incluso para clases nuevas.
public class ServicioDeReportes
{
    private readonly string _carpeta;

    public ServicioDeReportes(string carpeta)
    {
        _carpeta = string.IsNullOrWhiteSpace(carpeta) ? "Reportes" : carpeta;
    }

    public string Carpeta { get { return _carpeta; } }

    // Una sola rutina para cualquier listado, incluso clases que todavia no existen.
    public string Escribir<T>(IEnumerable<T> lista, string nombreArchivo)
    {
        string ruta = Path.Combine(_carpeta, nombreArchivo);

        new ReporteAutomatico<T>().Escribir(lista, ruta);

        return ruta;
    }

    // Si hace falta un formato propio, se usa un reporte hecho a medida.
    public string EscribirOrdenes(IEnumerable<OrdenRegistrada> lista)
    {
        string ruta = Path.Combine(_carpeta, "ordenes_detalle.txt");

        new ReporteDeOrdenes().Escribir(lista, ruta);

        return ruta;
    }
}
