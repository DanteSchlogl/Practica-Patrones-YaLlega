namespace YaLlegaNucleo.Modelo.Revision;

// Un eslabon de la cadena de revision.
public interface IRevisorDeOrden
{
    // Revisa y, si esta todo bien, le pasa la revision al revisor siguiente.
    void Revisar(RevisionDeOrden revision);

    // Engancha el revisor siguiente y lo devuelve, para poder enganchar varios seguidos.
    IRevisorDeOrden Enganchar(IRevisorDeOrden siguiente);
}
