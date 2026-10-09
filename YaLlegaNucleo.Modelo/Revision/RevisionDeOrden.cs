namespace YaLlegaNucleo.Modelo.Revision;

// Los datos que van pasando de un revisor al otro.
// El primero completa el articulo, asi los que siguen no lo tienen que buscar.
public class RevisionDeOrden
{
    public RevisionDeOrden(Cliente cliente, int codigoDeArticulo)
    {
        if (cliente == null) throw new ArgumentNullException(nameof(cliente));

        Cliente = cliente;
        CodigoDeArticulo = codigoDeArticulo;
    }

    public Cliente Cliente { get; private set; }

    public int CodigoDeArticulo { get; private set; }

    // Lo completa el primer revisor si encuentra el articulo.
    public Articulo? Articulo { get; set; }
}
