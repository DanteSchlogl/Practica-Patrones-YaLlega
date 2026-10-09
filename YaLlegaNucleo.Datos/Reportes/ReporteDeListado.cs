using YaLlegaNucleo.Datos.Excepciones;

namespace YaLlegaNucleo.Datos.Reportes;

// Template Method: aca esta LA rutina de exportacion.
// El esqueleto (encabezado, renglones y pie) queda escrito una sola vez, y cada
// reporte dice nada mas que columnas quiere.
public abstract class ReporteDeListado<T>
{
    // La unica rutina. Trabaja con cualquier clase porque no nombra a ninguna.
    public void Escribir(IEnumerable<T> lista, string rutaArchivo)
    {
        List<T> renglones = new List<T>(lista);

        try
        {
            string? carpeta = Path.GetDirectoryName(Path.GetFullPath(rutaArchivo));

            if (!string.IsNullOrEmpty(carpeta)) Directory.CreateDirectory(carpeta);

            List<string> texto = new List<string>();

            texto.Add(Encabezado());

            foreach (T renglon in renglones)
            {
                texto.Add(Renglon(renglon));
            }

            texto.Add("Cantidad de registros; " + renglones.Count);

            File.WriteAllLines(rutaArchivo, texto);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            throw new ErrorDeArchivoException("No pude escribir el reporte en " + Path.GetFullPath(rutaArchivo), rutaArchivo, ex);
        }
    }

    // Los dos pasos que cambian segun el tipo de listado.
    protected abstract string Encabezado();

    protected abstract string Renglon(T dato);
}
