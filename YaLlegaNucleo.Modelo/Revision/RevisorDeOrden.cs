namespace YaLlegaNucleo.Modelo.Revision;

// Base de la cadena.
// Se encarga de pasarle la revision al revisor siguiente, asi cada revisor escribe
// solamente su propio control.
public abstract class RevisorDeOrden : IRevisorDeOrden
{
    private IRevisorDeOrden? _siguiente;

    public IRevisorDeOrden Enganchar(IRevisorDeOrden siguiente)
    {
        if (siguiente == null) throw new ArgumentNullException(nameof(siguiente));

        _siguiente = siguiente;

        return siguiente;
    }

    public void Revisar(RevisionDeOrden revision)
    {
        if (revision == null) throw new ArgumentNullException(nameof(revision));

        Controlar(revision);

        // Si pase mi control, que siga el que viene.
        if (_siguiente != null) _siguiente.Revisar(revision);
    }

    // El control de cada revisor. Si algo no esta permitido, tiene que tirar excepcion.
    protected abstract void Controlar(RevisionDeOrden revision);
}
