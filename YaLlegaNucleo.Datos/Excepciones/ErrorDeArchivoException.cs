namespace YaLlegaNucleo.Datos.Excepciones;

// Se lanza cuando falla la lectura o el guardado de un archivo, asi las otras capas
// no tienen que saber de IOException ni de UnauthorizedAccessException.
public class ErrorDeArchivoException : Exception
{
    public ErrorDeArchivoException(string mensaje, string ruta, Exception interna)
        : base(mensaje, interna)
    {
        Ruta = ruta;
    }

    public string Ruta { get; private set; }
}
