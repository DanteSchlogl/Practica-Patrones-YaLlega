using YaLlegaNucleo.Datos;
using YaLlegaNucleo.Modelo;
using YaLlegaNucleo.Modelo.Cartas;
using YaLlegaNucleo.Modelo.Revision;

namespace YaLlegaNucleo.Negocio;

// Arma todo el nucleo y lo deja conectado.
// La consola le pide el sistema ya armado y no conoce ninguna clase de las otras capas.
public static class ArmadorDelSistema
{
    public static Nucleo Armar(CatalogoDeCartas catalogo, string carpetaOrdenes, string carpetaReportes, int cuantasCartasGuardar)
    {
        // El cache se pone delante del repositorio remoto.
        RepositorioRemotoDeCartas remoto = new RepositorioRemotoDeCartas(catalogo);
        CacheDeCartas cache = new CacheDeCartas(remoto, cuantasCartasGuardar);

        // La cadena de revision: primero existe, despues habilitado, despues el plan.
        RevisorDeArticulo primero = new RevisorDeArticulo(catalogo);

        primero.Enganchar(new RevisorDeCliente()).Enganchar(new RevisorDePlan());

        ArchivoDeOrdenes archivo = new ArchivoDeOrdenes(carpetaOrdenes);

        ServicioDeOrdenes ordenes = new ServicioDeOrdenes(primero, cache, archivo);
        ServicioDeReportes reportes = new ServicioDeReportes(carpetaReportes);

        return new Nucleo(ordenes, reportes, remoto, cache);
    }
}
