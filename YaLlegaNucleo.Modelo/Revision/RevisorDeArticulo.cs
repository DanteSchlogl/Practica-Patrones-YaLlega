using YaLlegaNucleo.Modelo.Excepciones;

namespace YaLlegaNucleo.Modelo.Revision;

// Primer revisor: el articulo tiene que existir en el catalogo.
public class RevisorDeArticulo : RevisorDeOrden
{
    private readonly CatalogoDeCartas _catalogo;

    public RevisorDeArticulo(CatalogoDeCartas catalogo)
    {
        if (catalogo == null) throw new ArgumentNullException(nameof(catalogo));

        _catalogo = catalogo;
    }

    protected override void Controlar(RevisionDeOrden revision)
    {
        Articulo? articulo = _catalogo.DameArticulo(revision.CodigoDeArticulo);

        if (articulo == null) throw new ArticuloDesconocidoException(revision.CodigoDeArticulo);

        // Se lo dejo listo a los que vienen.
        revision.Articulo = articulo;
    }
}
