using YaLlegaNucleo.Modelo.Excepciones;

namespace YaLlegaNucleo.Modelo.Revision;

// Tercer revisor: el plan del cliente tiene que alcanzar para ese articulo.
public class RevisorDePlan : RevisorDeOrden
{
    protected override void Controlar(RevisionDeOrden revision)
    {
        Articulo? articulo = revision.Articulo;

        if (articulo == null)
        {
            throw new AccesoNoPermitidoException("No se pudo identificar el articulo");
        }

        if (articulo.NivelNecesario > revision.Cliente.Plan.Nivel)
        {
            throw new AccesoNoPermitidoException(
                "El articulo '" + articulo.Detalle + "' pide el plan " + articulo.PlanNecesario
                + " y el cliente tiene " + revision.Cliente.Plan.Nombre);
        }
    }
}
