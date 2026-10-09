using YaLlegaNucleo.Modelo;
using YaLlegaNucleo.Modelo.Cartas;
using YaLlegaNucleo.Modelo.Excepciones;
using YaLlegaNucleo.Negocio;

namespace YaLlegaNucleo.Consola;

// Programa principal.
// Primero corre un ejemplo completo, despues las verificaciones y al final
// deja un menu para probar a mano.
internal static class Programa
{
    private const string CarpetaDeOrdenes = "Ordenes";
    private const string CarpetaDeReportes = "Reportes";
    private const int CartasQueSeGuardan = 5;

    private static int Main(string[] args)
    {
        Console.WriteLine("FACULTAD DE TECNOLOGIA INFORMATICA");
        Console.WriteLine("Ingenieria en Sistemas - Practica de parcial");
        Console.WriteLine("YaLlega S.A. - Nucleo de la plataforma de pedidos y entregas");
        Console.WriteLine("Alumno: Dante Schlogl");
        Console.WriteLine();

        CatalogoDeCartas catalogo = Ejemplos.ArmarCatalogo();
        List<Cliente> clientes = Ejemplos.ArmarClientes();

        Nucleo nucleo = ArmadorDelSistema.Armar(catalogo, CarpetaDeOrdenes, CarpetaDeReportes, CartasQueSeGuardan);

        bool sinMenu = false;

        foreach (string argumento in args)
        {
            if (argumento.ToLower() == "--sin-menu") sinMenu = true;
        }

        Ejemplo(nucleo, catalogo, clientes);

        Verificaciones.Correr();

        if (!sinMenu) MostrarMenuDeOpciones(nucleo, catalogo, clientes);

        Console.WriteLine();
        Console.WriteLine("Fin del programa.");

        return 0;
    }

    // Recorro los escenarios del enunciado, uno por uno.
    private static void Ejemplo(Nucleo nucleo, CatalogoDeCartas catalogo, List<Cliente> clientes)
    {
        Cliente lucia = clientes[0];
        Cliente martin = clientes[1];
        Cliente sofia = clientes[2];

        Console.WriteLine("=== Ejemplo ===");

        Console.WriteLine();
        Console.WriteLine("Las cartas del catalogo:");
        MostrarCartas(catalogo);

        Console.WriteLine();
        Console.WriteLine("Lucia (plan premium) abre una orden y la consola pide la carta de entradas.");
        nucleo.Ordenes.NuevaOrden(1, lucia, NivelDeDemanda.Baja);

        Carta? carta = nucleo.Ordenes.PedirCarta(Ejemplos.Entradas);

        if (carta != null) MostrarCarta(carta, nucleo.Ordenes.CartaServidaDesde);

        Console.WriteLine();
        Console.WriteLine("Suma 2 empanadas y 1 agua.");
        nucleo.Ordenes.Sumar(Ejemplos.Empanada, 2);
        nucleo.Ordenes.Sumar(Ejemplos.Agua, 1);
        MostrarOrden(nucleo);

        Console.WriteLine();
        Console.WriteLine("La zona se complica: el sistema mide la demanda y da alta.");
        nucleo.Ordenes.AvisarDemanda(NivelDeDemanda.Alta);
        MostrarOrden(nucleo);

        Console.WriteLine();
        Console.WriteLine("El operador del local aprieta el boton de promocion.");
        nucleo.Ordenes.PonerPromocion();
        MostrarOrden(nucleo);

        Console.WriteLine();
        Console.WriteLine("Lo vuelve a soltar.");
        nucleo.Ordenes.SacarPromocion();
        MostrarOrden(nucleo);

        Console.WriteLine();
        Console.WriteLine("Martin (plan clasico) quiere pedir el ojo de bife, que es premium.");
        nucleo.Ordenes.NuevaOrden(2, martin, NivelDeDemanda.Baja);

        try
        {
            nucleo.Ordenes.Sumar(Ejemplos.OjoDeBife, 1);
        }
        catch (AccesoNoPermitidoException ex)
        {
            Console.WriteLine("  Rechazado: " + ex.Message);
        }

        Console.WriteLine();
        Console.WriteLine("Sofia esta dada de baja, asi que tampoco puede.");
        nucleo.Ordenes.NuevaOrden(3, sofia, NivelDeDemanda.Baja);

        try
        {
            nucleo.Ordenes.Sumar(Ejemplos.Empanada, 1);
        }
        catch (AccesoNoPermitidoException ex)
        {
            Console.WriteLine("  Rechazado: " + ex.Message);
        }

        Console.WriteLine();
        Console.WriteLine("Lucia pide la misma carta otra vez: la consola no lo nota, pero sale del dispositivo.");
        nucleo.Ordenes.NuevaOrden(4, lucia, NivelDeDemanda.Baja);
        nucleo.Ordenes.Sumar(Ejemplos.Empanada, 1);
        nucleo.Ordenes.PedirCarta(Ejemplos.Entradas);

        Console.WriteLine("  La carta salio del: " + nucleo.Ordenes.CartaServidaDesde);
        Console.WriteLine("  Consultas al servidor: " + nucleo.ConsultasAlServidor
            + " | veces que sirvio el cache: " + nucleo.VecesQueSirvioElCache
            + " | cartas guardadas: " + nucleo.CartasGuardadas + " de " + nucleo.CuantasGuarda);

        Console.WriteLine();
        Console.WriteLine("Cierro la orden de Lucia y la guardo.");
        OrdenRegistrada registrada = nucleo.Ordenes.CerrarYGuardar();

        Console.WriteLine("  Cliente: " + registrada.Cliente);
        Console.WriteLine("  Articulos: " + registrada.Articulos);
        Console.WriteLine("  Tarifas de envio: " + string.Join(" >> ", registrada.TarifasDeEnvio));
        Console.WriteLine("  Carta servida desde: " + registrada.CartaServidaDesde);

        Console.WriteLine();
        Console.WriteLine("Marketing escribe los listados con la misma rutina.");
        string rutaOrdenes = nucleo.Reportes.Escribir(nucleo.Ordenes.Guardadas(), "ordenes.txt");
        string rutaClientes = nucleo.Reportes.Escribir(clientes, "clientes.txt");
        string rutaCartas = nucleo.Reportes.Escribir(catalogo.Cartas, "cartas.txt");

        Console.WriteLine("  " + Path.GetFullPath(rutaOrdenes));
        Console.WriteLine("  " + Path.GetFullPath(rutaClientes));
        Console.WriteLine("  " + Path.GetFullPath(rutaCartas));

        Console.WriteLine();
        Console.WriteLine("El encabezado del listado de clientes lo saca solo:");
        Console.WriteLine("  " + File.ReadLines(rutaClientes).First());

        Console.WriteLine();
        Console.WriteLine("Asi quedo el archivo de ordenes:");
        MostrarArchivo(nucleo);
    }

    // Menu para probar a mano.
    private static void MostrarMenuDeOpciones(Nucleo nucleo, CatalogoDeCartas catalogo, List<Cliente> clientes)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Menu ===");
            Console.WriteLine("1) Ver las cartas");
            Console.WriteLine("2) Ver los articulos de una carta");
            Console.WriteLine("3) Abrir una orden");
            Console.WriteLine("4) Sumar un articulo");
            Console.WriteLine("5) Sacar un articulo");
            Console.WriteLine("6) Avisar la demanda");
            Console.WriteLine("7) Poner promocion");
            Console.WriteLine("8) Sacar promocion");
            Console.WriteLine("9) Ver la orden");
            Console.WriteLine("10) Cerrar y guardar");
            Console.WriteLine("11) Ver el archivo");
            Console.WriteLine("12) Escribir los reportes");
            Console.WriteLine("13) Correr las verificaciones");
            Console.WriteLine("0) Salir");
            Console.Write("Opcion: ");

            string? opcion = Console.ReadLine();

            // Si ya no hay entrada (por ejemplo al correrlo por un pipe) salgo sin error.
            if (opcion == null)
            {
                Console.WriteLine();
                Console.WriteLine("No hay mas entrada, salgo.");
                return;
            }

            opcion = opcion.Trim();

            if (opcion == "0") return;

            Console.WriteLine();

            try
            {
                Atender(opcion, nucleo, catalogo, clientes);
            }
            catch (AccesoNoPermitidoException ex)
            {
                Console.WriteLine("No se puede: " + ex.Message);
            }
            catch (ArticuloDesconocidoException ex)
            {
                Console.WriteLine("No se puede: " + ex.Message);
            }
            catch (ArticuloNoEstaEnLaOrdenException ex)
            {
                Console.WriteLine("No se puede: " + ex.Message);
            }
            catch (CantidadFueraDeRangoException ex)
            {
                Console.WriteLine("No se puede: " + ex.Message);
            }
            catch (OrdenYaCerradaException ex)
            {
                Console.WriteLine("No se puede: " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Ojo: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    private static void Atender(string opcion, Nucleo nucleo, CatalogoDeCartas catalogo, List<Cliente> clientes)
    {
        if (opcion == "1")
        {
            MostrarCartas(catalogo);
        }
        else if (opcion == "2")
        {
            Console.Write("Numero de la carta: ");
            int numero = PedirNumero();

            Carta? carta = nucleo.Ordenes.PedirCarta(numero);

            if (carta == null)
            {
                Console.WriteLine("No existe esa carta.");
                return;
            }

            MostrarCarta(carta, nucleo.Ordenes.CartaServidaDesde);
        }
        else if (opcion == "3")
        {
            MostrarClientes(clientes);

            Console.Write("Numero de cliente: ");
            int cual = PedirNumero();

            if (cual < 1 || cual > clientes.Count)
            {
                Console.WriteLine("No existe ese cliente.");
                return;
            }

            Console.Write("Numero de orden: ");
            int numero = PedirNumero();

            nucleo.Ordenes.NuevaOrden(numero, clientes[cual - 1], PedirDemanda());

            Console.WriteLine("Orden abierta para " + clientes[cual - 1].Nombre + ".");
        }
        else if (opcion == "4")
        {
            MostrarArticulos(catalogo);

            Console.Write("Codigo del articulo: ");
            int codigo = PedirNumero();

            Console.Write("Unidades: ");
            int unidades = PedirNumero();

            nucleo.Ordenes.Sumar(codigo, unidades);
            MostrarOrden(nucleo);
        }
        else if (opcion == "5")
        {
            Console.Write("Codigo del articulo a sacar: ");
            nucleo.Ordenes.Sacar(PedirNumero());
            MostrarOrden(nucleo);
        }
        else if (opcion == "6")
        {
            nucleo.Ordenes.AvisarDemanda(PedirDemanda());
            MostrarOrden(nucleo);
        }
        else if (opcion == "7")
        {
            nucleo.Ordenes.PonerPromocion();
            MostrarOrden(nucleo);
        }
        else if (opcion == "8")
        {
            nucleo.Ordenes.SacarPromocion();
            MostrarOrden(nucleo);
        }
        else if (opcion == "9")
        {
            MostrarOrden(nucleo);
        }
        else if (opcion == "10")
        {
            OrdenRegistrada registrada = nucleo.Ordenes.CerrarYGuardar();

            Console.WriteLine("Orden guardada: " + registrada.Cliente + " - " + registrada.Articulos);
        }
        else if (opcion == "11")
        {
            MostrarArchivo(nucleo);
        }
        else if (opcion == "12")
        {
            string ruta = nucleo.Reportes.Escribir(catalogo.TodosLosArticulos(), "articulos.txt");

            Console.WriteLine("Escrito: " + Path.GetFullPath(ruta));
        }
        else if (opcion == "13")
        {
            Verificaciones.Correr();
        }
        else
        {
            Console.WriteLine("Opcion invalida.");
        }
    }

    private static void MostrarCartas(CatalogoDeCartas catalogo)
    {
        foreach (Carta carta in catalogo.Cartas)
        {
            Console.WriteLine("  Carta " + carta.Numero + ") " + carta.Titulo
                + " - " + carta.CuantosArticulos + " articulos");
        }
    }

    private static void MostrarCarta(Carta carta, string origen)
    {
        Console.WriteLine("Carta '" + carta.Titulo + "' (vino del " + origen + "):");

        foreach (Articulo articulo in carta.Articulos)
        {
            Console.WriteLine("  " + articulo.Codigo + ") " + articulo.Detalle
                + " - $" + articulo.Importe.ToString("0.00")
                + " [plan " + articulo.PlanNecesario + "]");
        }
    }

    private static void MostrarArticulos(CatalogoDeCartas catalogo)
    {
        foreach (Articulo articulo in catalogo.TodosLosArticulos())
        {
            Console.WriteLine("  " + articulo.Codigo + ") " + articulo.Detalle
                + " - $" + articulo.Importe.ToString("0.00")
                + " [plan " + articulo.PlanNecesario + "]");
        }
    }

    private static void MostrarClientes(List<Cliente> clientes)
    {
        int numero = 0;

        foreach (Cliente cliente in clientes)
        {
            numero++;

            string estado = cliente.Habilitado ? "habilitado" : "DADO DE BAJA";

            Console.WriteLine("  " + numero + ") " + cliente.Nombre
                + " - plan " + cliente.Plan.Nombre + " (" + estado + ")");
        }
    }

    private static void MostrarOrden(Nucleo nucleo)
    {
        Console.WriteLine();

        if (!nucleo.Ordenes.HayOrden)
        {
            Console.WriteLine("Todavia no hay una orden abierta.");
            return;
        }

        Orden orden = nucleo.Ordenes.OrdenActual();

        Console.WriteLine("Orden " + orden.Numero + " de " + orden.Cliente.Nombre
            + (orden.Abierta ? " (abierta)" : " (cerrada)"));

        if (orden.Lineas.Count == 0)
        {
            Console.WriteLine("  Todavia no hay articulos.");
        }

        foreach (LineaDeOrden linea in orden.Lineas)
        {
            Console.WriteLine("  " + linea.Unidades + "x " + linea.Articulo.Detalle
                + " -> $" + linea.Importe.ToString("0.00"));
        }

        Console.WriteLine("  Importe de articulos: $" + orden.ImporteDeArticulos().ToString("0.00"));
        Console.WriteLine("  Envio (" + orden.Envio.ModalidadActual + ", demanda " + orden.Envio.Demanda
            + "): $" + orden.Envio.Cobrar().ToString("0.00"));
        Console.WriteLine("  IMPORTE TOTAL: $" + orden.ImporteTotal().ToString("0.00"));
        Console.WriteLine("  Rechazos: " + nucleo.Ordenes.IntentosRechazados);

        if (orden.Envio.Aplicadas.Count > 0)
        {
            Console.WriteLine("  Tarifas que se aplicaron: " + string.Join(" >> ", orden.Envio.Aplicadas));
        }
    }

    private static void MostrarArchivo(Nucleo nucleo)
    {
        Console.WriteLine("Archivo: " + Path.GetFullPath(nucleo.Ordenes.RutaDelArchivo()));

        IReadOnlyList<string> lineas = nucleo.Ordenes.LeerArchivo();

        if (lineas.Count == 0)
        {
            Console.WriteLine("Todavia no hay ordenes guardadas.");
            return;
        }

        foreach (string linea in lineas)
        {
            Console.WriteLine(linea);
        }
    }

    private static NivelDeDemanda PedirDemanda()
    {
        Console.Write("Demanda (baja / media / alta): ");

        string? texto = Console.ReadLine();

        if (texto == null) return NivelDeDemanda.Baja;

        string valor = texto.Trim().ToLower();

        if (valor == "media") return NivelDeDemanda.Media;
        if (valor == "alta") return NivelDeDemanda.Alta;

        return NivelDeDemanda.Baja;
    }

    private static int PedirNumero()
    {
        string? texto = Console.ReadLine();

        if (texto == null) return 0;

        int numero;

        if (!int.TryParse(texto.Trim(), out numero)) return 0;

        return numero;
    }
}
