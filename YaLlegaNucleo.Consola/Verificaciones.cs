using YaLlegaNucleo.Modelo;
using YaLlegaNucleo.Modelo.Excepciones;
using YaLlegaNucleo.Negocio;

namespace YaLlegaNucleo.Consola;

// Verificaciones de cada escenario del enunciado.
// Cada una arma su propio nucleo y su propia carpeta, asi no se mezclan entre si.
internal static class Verificaciones
{
    private static int _bien;
    private static int _mal;

    public static void Correr()
    {
        _bien = 0;
        _mal = 0;

        // Arranco de cero para que se puedan correr varias veces seguidas.
        if (Directory.Exists("Verificaciones"))
        {
            Directory.Delete("Verificaciones", true);
        }

        Console.WriteLine();
        Console.WriteLine("=== Verificaciones ===");

        Console.WriteLine();
        Console.WriteLine("-- El cobro del envio --");
        ConDemandaBaja();
        ConDemandaMedia();
        ConDemandaAlta();
        ConPromocion();
        CambiarLaDemandaDuranteLaOrden();
        ElOperadorPoneYPoneSacarPromocion();

        Console.WriteLine();
        Console.WriteLine("-- El cache de cartas --");
        LaPrimeraVezBajaDelServidor();
        NoGuardaMasDeCinco();
        LaMasViejaSeVuelveABajar();

        Console.WriteLine();
        Console.WriteLine("-- La revision de la orden --");
        ArticuloQueNoExiste();
        ClienteDadoDeBaja();
        PlanQueNoAlcanza();
        PlanQueAlcanza();
        PlanPlusYPlanPremium();

        Console.WriteLine();
        Console.WriteLine("-- Otras validaciones --");
        UnidadesQueNoTienenSentido();
        SacarAlgoQueNoEsta();
        OrdenCerrada();

        Console.WriteLine();
        Console.WriteLine("-- Lo que se guarda --");
        UnaLineaPorOrden();
        LaLineaTieneTodosLosDatos();
        SeGuardaDeDondeVinoLaCarta();
        SeGuardanLosIntentosRechazados();

        Console.WriteLine();
        Console.WriteLine("-- Los reportes --");
        LosTresListadosConLaMismaRutina();
        ReporteConColumnasPropias();

        Console.WriteLine();
        Console.WriteLine("Resultado: " + _bien + " bien, " + _mal + " mal");
    }

    // ---------------- El cobro del envio ----------------

    private static void ConDemandaBaja()
    {
        Nucleo nucleo = NucleoNuevo("cobro_bajo");

        nucleo.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Baja);

        Comprobar("Con demanda baja se cobra la tarifa base",
            nucleo.Ordenes.OrdenActual().Envio.Cobrar() == Ejemplos.CobroConDemandaBaja);
    }

    private static void ConDemandaMedia()
    {
        Nucleo nucleo = NucleoNuevo("cobro_medio");

        nucleo.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Media);

        Comprobar("Con demanda media se suma el 25 %",
            nucleo.Ordenes.OrdenActual().Envio.Cobrar() == Ejemplos.CobroConDemandaMedia);
    }

    private static void ConDemandaAlta()
    {
        Nucleo nucleo = NucleoNuevo("cobro_alto");

        nucleo.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Alta);

        Comprobar("Con demanda alta se suma el 60 %",
            nucleo.Ordenes.OrdenActual().Envio.Cobrar() == Ejemplos.CobroConDemandaAlta);
    }

    private static void ConPromocion()
    {
        Nucleo nucleo = NucleoNuevo("cobro_promo");

        nucleo.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Alta);
        nucleo.Ordenes.PonerPromocion();

        Comprobar("En promocion el envio no se cobra aunque la demanda sea alta",
            nucleo.Ordenes.OrdenActual().Envio.Cobrar() == 0);
    }

    private static void CambiarLaDemandaDuranteLaOrden()
    {
        Nucleo nucleo = NucleoNuevo("cobro_cambio");

        nucleo.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Baja);
        nucleo.Ordenes.Sumar(Ejemplos.Empanada, 1);

        // La zona se puso complicada y el sistema lo detecta.
        nucleo.Ordenes.AvisarDemanda(NivelDeDemanda.Alta);

        Comprobar("El envio se recalcula durante la orden",
            nucleo.Ordenes.OrdenActual().Envio.Cobrar() == Ejemplos.CobroConDemandaAlta);

        Comprobar("Quedaron anotadas las dos tarifas",
            nucleo.Ordenes.OrdenActual().Envio.Aplicadas.Count == 2);

        Comprobar("La orden arranco con la tarifa de demanda baja",
            nucleo.Ordenes.OrdenActual().Envio.Aplicadas[0].Contains("demanda Baja"));

        Comprobar("Y termino con la de demanda alta",
            nucleo.Ordenes.OrdenActual().Envio.Aplicadas[1].Contains("demanda Alta"));

        Comprobar("El importe total es articulos mas envio",
            nucleo.Ordenes.OrdenActual().ImporteTotal() == 1200 + Ejemplos.CobroConDemandaAlta);
    }

    private static void ElOperadorPoneYPoneSacarPromocion()
    {
        Nucleo nucleo = NucleoNuevo("cobro_boton");

        nucleo.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Media);

        Comprobar("Arranca cobrando por demanda",
            nucleo.Ordenes.OrdenActual().Envio.Cobrar() == Ejemplos.CobroConDemandaMedia);

        nucleo.Ordenes.PonerPromocion();

        Comprobar("El operador pone el modo promocion",
            nucleo.Ordenes.OrdenActual().Envio.Cobrar() == 0);

        nucleo.Ordenes.SacarPromocion();

        Comprobar("Y lo vuelve a sacar con el mismo boton",
            nucleo.Ordenes.OrdenActual().Envio.Cobrar() == Ejemplos.CobroConDemandaMedia);
    }

    // ---------------- El cache de cartas ----------------

    private static void LaPrimeraVezBajaDelServidor()
    {
        Nucleo nucleo = NucleoNuevo("cache_uno");

        nucleo.Ordenes.PedirCarta(Ejemplos.Entradas);

        Comprobar("La primera carta baja del servidor",
            nucleo.Ordenes.CartaServidaDesde == "Servidor");

        nucleo.Ordenes.PedirCarta(Ejemplos.Entradas);

        Comprobar("La segunda vez sale del dispositivo",
            nucleo.Ordenes.CartaServidaDesde == "Dispositivo");

        Comprobar("El servidor se consulto una sola vez", nucleo.ConsultasAlServidor == 1);

        Comprobar("Y el cache sirvio una vez", nucleo.VecesQueSirvioElCache == 1);
    }

    private static void NoGuardaMasDeCinco()
    {
        Nucleo nucleo = NucleoNuevo("cache_tope");

        for (int carta = 1; carta <= 6; carta++)
        {
            nucleo.Ordenes.PedirCarta(carta);
        }

        Comprobar("Se pidieron las 6 cartas al servidor", nucleo.ConsultasAlServidor == 6);

        Comprobar("El cache guarda 5 como maximo", nucleo.CartasGuardadas == 5);

        Comprobar("El tope del cache es 5", nucleo.CuantasGuarda == 5);
    }

    private static void LaMasViejaSeVuelveABajar()
    {
        Nucleo nucleo = NucleoNuevo("cache_descarte");

        for (int carta = 1; carta <= 6; carta++)
        {
            nucleo.Ordenes.PedirCarta(carta);
        }

        // La carta 1 fue la primera, asi que ya no esta guardada.
        nucleo.Ordenes.PedirCarta(Ejemplos.Entradas);

        Comprobar("La carta mas vieja tuvo que bajarse otra vez", nucleo.ConsultasAlServidor == 7);

        Comprobar("Y volvio a salir del servidor", nucleo.Ordenes.CartaServidaDesde == "Servidor");
    }

    // ---------------- La revision de la orden ----------------

    private static void ArticuloQueNoExiste()
    {
        Nucleo nucleo = NucleoNuevo("revision_inexistente");

        nucleo.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Baja);

        Rechaza<ArticuloDesconocidoException>("Un articulo que no figura en la carta se rechaza",
            () => nucleo.Ordenes.Sumar(999, 1));

        Comprobar("Y queda contado como intento rechazado",
            nucleo.Ordenes.IntentosRechazados == 1);
    }

    private static void ClienteDadoDeBaja()
    {
        Nucleo nucleo = NucleoNuevo("revision_baja");

        nucleo.Ordenes.NuevaOrden(1, Sofia(), NivelDeDemanda.Baja);

        Rechaza<AccesoNoPermitidoException>("Un cliente dado de baja no puede pedir",
            () => nucleo.Ordenes.Sumar(Ejemplos.Empanada, 1));
    }

    private static void PlanQueNoAlcanza()
    {
        Nucleo nucleo = NucleoNuevo("revision_plan_corto");

        // Martin tiene plan clasico y el ojo de bife es premium.
        nucleo.Ordenes.NuevaOrden(1, Martin(), NivelDeDemanda.Baja);

        Rechaza<AccesoNoPermitidoException>("El plan clasico no llega a un articulo premium",
            () => nucleo.Ordenes.Sumar(Ejemplos.OjoDeBife, 1));

        Rechaza<AccesoNoPermitidoException>("Ni a uno del plan plus",
            () => nucleo.Ordenes.Sumar(Ejemplos.Paella, 1));

        Comprobar("Los dos intentos quedaron contados",
            nucleo.Ordenes.IntentosRechazados == 2);
    }

    private static void PlanQueAlcanza()
    {
        Nucleo nucleo = NucleoNuevo("revision_plan_ok");

        nucleo.Ordenes.NuevaOrden(1, Martin(), NivelDeDemanda.Baja);
        nucleo.Ordenes.Sumar(Ejemplos.Empanada, 2);

        Comprobar("El plan clasico si puede pedir un articulo clasico",
            nucleo.Ordenes.OrdenActual().Lineas.Count == 1);

        Comprobar("Y no se conto ningun rechazo", nucleo.Ordenes.IntentosRechazados == 0);
    }

    private static void PlanPlusYPlanPremium()
    {
        Nucleo nucleo = NucleoNuevo("revision_planes");

        Cliente diego = new Cliente("Diego", Plan.Plus);

        nucleo.Ordenes.NuevaOrden(1, diego, NivelDeDemanda.Baja);
        nucleo.Ordenes.Sumar(Ejemplos.Paella, 1);

        Comprobar("El plan plus llega a un articulo del plan plus",
            nucleo.Ordenes.OrdenActual().Lineas.Count == 1);

        Rechaza<AccesoNoPermitidoException>("Pero no a uno premium",
            () => nucleo.Ordenes.Sumar(Ejemplos.Salmon, 1));

        Nucleo otro = NucleoNuevo("revision_premium");

        otro.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Baja);
        otro.Ordenes.Sumar(Ejemplos.Salmon, 1);

        Comprobar("El plan premium llega a un articulo premium",
            otro.Ordenes.OrdenActual().Lineas.Count == 1);
    }

    // ---------------- Otras validaciones ----------------

    private static void UnidadesQueNoTienenSentido()
    {
        Nucleo nucleo = NucleoNuevo("unidades");

        nucleo.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Baja);

        Rechaza<CantidadFueraDeRangoException>("No se aceptan cero unidades",
            () => nucleo.Ordenes.Sumar(Ejemplos.Empanada, 0));

        Rechaza<CantidadFueraDeRangoException>("Tampoco unidades negativas",
            () => nucleo.Ordenes.Sumar(Ejemplos.Empanada, -2));
    }

    private static void SacarAlgoQueNoEsta()
    {
        Nucleo nucleo = NucleoNuevo("sacar_inexistente");

        nucleo.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Baja);

        Rechaza<ArticuloNoEstaEnLaOrdenException>("Sacar un articulo que no esta en la orden da error",
            () => nucleo.Ordenes.Sacar(Ejemplos.Tiramisu));
    }

    private static void OrdenCerrada()
    {
        Nucleo nucleo = NucleoNuevo("orden_cerrada");

        nucleo.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Baja);
        nucleo.Ordenes.Sumar(Ejemplos.Empanada, 1);
        nucleo.Ordenes.CerrarYGuardar();

        Comprobar("La orden queda cerrada", !nucleo.Ordenes.OrdenActual().Abierta);

        Rechaza<OrdenYaCerradaException>("Una orden cerrada no acepta mas articulos",
            () => nucleo.Ordenes.Sumar(Ejemplos.Limonada, 1));
    }

    // ---------------- Lo que se guarda ----------------

    private static void UnaLineaPorOrden()
    {
        Nucleo nucleo = NucleoNuevo("guardado_lineas");

        nucleo.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Baja);
        nucleo.Ordenes.Sumar(Ejemplos.Empanada, 1);
        nucleo.Ordenes.CerrarYGuardar();

        Comprobar("Se guardo una linea por la orden", nucleo.Ordenes.LeerArchivo().Count == 1);
    }

    private static void LaLineaTieneTodosLosDatos()
    {
        Nucleo nucleo = NucleoNuevo("guardado_datos");

        nucleo.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Baja);
        nucleo.Ordenes.PedirCarta(Ejemplos.Principales);
        nucleo.Ordenes.Sumar(Ejemplos.Milanesa, 2);
        nucleo.Ordenes.Sumar(Ejemplos.Agua, 1);
        nucleo.Ordenes.CerrarYGuardar();

        string linea = nucleo.Ordenes.LeerArchivo()[0];

        bool tieneCliente = linea.Contains("cliente: Lucia");
        bool tieneArticulos = linea.Contains("articulos: 2x Milanesa napolitana, 1x Agua con gas");
        bool tieneTarifas = linea.Contains("tarifas:") && linea.Contains("demanda Baja");
        bool tieneCarta = linea.Contains("carta: Servidor");
        bool tieneRechazados = linea.Contains("rechazados: 0");

        Comprobar("La linea guarda cliente, articulos, tarifas, carta y rechazados",
            tieneCliente && tieneArticulos && tieneTarifas && tieneCarta && tieneRechazados);
    }

    private static void SeGuardaDeDondeVinoLaCarta()
    {
        Nucleo nucleo = NucleoNuevo("guardado_origen");

        nucleo.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Baja);
        nucleo.Ordenes.PedirCarta(Ejemplos.Entradas);
        nucleo.Ordenes.Sumar(Ejemplos.Empanada, 1);

        // La consola vuelve a abrir la misma carta y esta vez ya esta guardada.
        nucleo.Ordenes.PedirCarta(Ejemplos.Entradas);

        nucleo.Ordenes.CerrarYGuardar();

        Comprobar("Se guarda que la carta vino del dispositivo",
            nucleo.Ordenes.LeerArchivo()[0].Contains("carta: Dispositivo"));
    }

    private static void SeGuardanLosIntentosRechazados()
    {
        Nucleo nucleo = NucleoNuevo("guardado_rechazos");

        nucleo.Ordenes.NuevaOrden(1, Martin(), NivelDeDemanda.Baja);

        IgnorarElRechazo(() => nucleo.Ordenes.Sumar(Ejemplos.OjoDeBife, 1));
        IgnorarElRechazo(() => nucleo.Ordenes.Sumar(Ejemplos.Paella, 1));
        IgnorarElRechazo(() => nucleo.Ordenes.Sumar(999, 1));

        nucleo.Ordenes.Sumar(Ejemplos.Empanada, 1);
        nucleo.Ordenes.CerrarYGuardar();

        Comprobar("Los tres intentos rechazados quedan guardados",
            nucleo.Ordenes.LeerArchivo()[0].Contains("rechazados: 3"));
    }

    // ---------------- Los reportes ----------------

    private static void LosTresListadosConLaMismaRutina()
    {
        Nucleo nucleo = NucleoNuevo("reportes_todos");

        nucleo.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Baja);
        nucleo.Ordenes.Sumar(Ejemplos.Empanada, 1);
        nucleo.Ordenes.CerrarYGuardar();

        string deArticulos = nucleo.Reportes.Escribir(nucleo.Ordenes.Guardadas(), "ordenes.txt");
        string deClientes = nucleo.Reportes.Escribir(Ejemplos.ArmarClientes(), "clientes.txt");
        string deCartas = nucleo.Reportes.Escribir(Ejemplos.ArmarCatalogo().Cartas, "cartas.txt");

        Comprobar("Los tres listados salieron con la misma rutina",
            File.Exists(deArticulos) && File.Exists(deClientes) && File.Exists(deCartas));

        string encabezado = File.ReadLines(deClientes).First();

        Comprobar("El encabezado sale del nombre de las propiedades",
            encabezado.Contains("Nombre") && encabezado.Contains("Plan") && encabezado.Contains("Habilitado"));

        Comprobar("Y el de ordenes incluye la lista de tarifas",
            File.ReadLines(deArticulos).First().Contains("TarifasDeEnvio"));
    }

    private static void ReporteConColumnasPropias()
    {
        Nucleo nucleo = NucleoNuevo("reportes_propio");

        nucleo.Ordenes.NuevaOrden(1, Lucia(), NivelDeDemanda.Baja);
        nucleo.Ordenes.Sumar(Ejemplos.Empanada, 1);
        nucleo.Ordenes.CerrarYGuardar();

        string ruta = nucleo.Reportes.EscribirOrdenes(nucleo.Ordenes.Guardadas());

        Comprobar("El reporte a medida usa su propio encabezado",
            File.ReadLines(ruta).First()
                == "FECHA; CLIENTE; ARTICULOS; TARIFAS DE ENVIO; CARTA SERVIDA DESDE; RECHAZADOS");
    }

    // ---------------- Ayudas ----------------

    // Armo un nucleo nuevo con su propia carpeta, asi cada verificacion es independiente.
    private static Nucleo NucleoNuevo(string nombre)
    {
        string carpeta = Path.Combine("Verificaciones", nombre);

        return ArmadorDelSistema.Armar(
            Ejemplos.ArmarCatalogo(),
            Path.Combine(carpeta, "Ordenes"),
            Path.Combine(carpeta, "Reportes"),
            5);
    }

    private static Cliente Lucia()
    {
        return Ejemplos.ArmarClientes()[0];
    }

    private static Cliente Martin()
    {
        return Ejemplos.ArmarClientes()[1];
    }

    private static Cliente Sofia()
    {
        return Ejemplos.ArmarClientes()[2];
    }

    // Cuando esperamos que el sistema rechace algo, no hacemos nada con el error.
    private static void IgnorarElRechazo(Action accion)
    {
        try
        {
            accion();
        }
        catch (AccesoNoPermitidoException)
        {
        }
        catch (ArticuloDesconocidoException)
        {
        }
    }

    // Anoto si la verificacion salio bien o mal.
    private static void Comprobar(string queSeProbo, bool salioBien)
    {
        if (salioBien)
        {
            _bien++;
            Console.WriteLine("bien    " + queSeProbo);
        }
        else
        {
            _mal++;
            Console.WriteLine("MAL     " + queSeProbo);
        }
    }

    // Para las verificaciones donde esperamos que el sistema rechace.
    private static void Rechaza<T>(string queSeProbo, Action accion) where T : Exception
    {
        try
        {
            accion();

            Comprobar(queSeProbo, false);
        }
        catch (T)
        {
            Comprobar(queSeProbo, true);
        }
    }
}
