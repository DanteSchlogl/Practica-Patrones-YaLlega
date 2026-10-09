using YaLlegaNucleo.Modelo.Excepciones;

namespace YaLlegaNucleo.Modelo.Revision;

// Segundo revisor: el cliente tiene que estar habilitado.
public class RevisorDeCliente : RevisorDeOrden
{
    protected override void Controlar(RevisionDeOrden revision)
    {
        if (!revision.Cliente.Habilitado)
        {
            throw new AccesoNoPermitidoException("El cliente " + revision.Cliente.Nombre + " esta dado de baja");
        }
    }
}
