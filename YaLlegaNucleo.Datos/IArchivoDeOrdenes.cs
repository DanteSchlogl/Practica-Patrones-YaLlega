using YaLlegaNucleo.Modelo;

namespace YaLlegaNucleo.Datos;

// Lo que necesita el negocio para guardar las ordenes.
// No dice como ni donde se guardan, eso lo decide la clase que lo implementa.
public interface IArchivoDeOrdenes
{
    string Carpeta { get; }

    string Ruta { get; }

    IReadOnlyList<OrdenRegistrada> Guardadas { get; }

    void Guardar(OrdenRegistrada orden);

    IReadOnlyList<string> LeerLineas();
}
