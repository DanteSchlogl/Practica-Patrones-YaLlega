using YaLlegaNucleo.Datos.Excepciones;
using YaLlegaNucleo.Modelo;

namespace YaLlegaNucleo.Datos;

// Guarda las ordenes en un archivo de texto, una linea por orden.
public class ArchivoDeOrdenes : IArchivoDeOrdenes
{
    private const string NombreDelArchivo = "ordenes.log";

    private readonly TextoDeOrden _texto = new TextoDeOrden();

    // Las ordenes que se guardaron en esta corrida, para poder listarlas.
    private readonly List<OrdenRegistrada> _guardadas = new List<OrdenRegistrada>();

    // Para que dos escrituras al mismo tiempo no rompan el archivo.
    private readonly object _turno = new object();

    public ArchivoDeOrdenes(string carpeta)
    {
        Carpeta = string.IsNullOrWhiteSpace(carpeta) ? "Ordenes" : carpeta;
    }

    public string Carpeta { get; private set; }

    public string Ruta { get { return Path.Combine(Carpeta, NombreDelArchivo); } }

    public IReadOnlyList<OrdenRegistrada> Guardadas { get { return _guardadas; } }

    public void Guardar(OrdenRegistrada orden)
    {
        if (orden == null) throw new ArgumentNullException(nameof(orden));

        string linea = _texto.Armar(orden) + Environment.NewLine;

        try
        {
            lock (_turno)
            {
                Directory.CreateDirectory(Carpeta);
                File.AppendAllText(Ruta, linea);
            }

            _guardadas.Add(orden);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            throw new ErrorDeArchivoException("No pude guardar la orden en " + Path.GetFullPath(Ruta), Ruta, ex);
        }
    }

    public IReadOnlyList<string> LeerLineas()
    {
        try
        {
            if (!File.Exists(Ruta)) return new List<string>();

            return File.ReadAllLines(Ruta);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            throw new ErrorDeArchivoException("No pude leer el archivo de ordenes", Ruta, ex);
        }
    }
}
