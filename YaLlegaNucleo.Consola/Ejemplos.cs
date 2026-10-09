using YaLlegaNucleo.Modelo;

namespace YaLlegaNucleo.Consola;

// Los datos de ejemplo con los que se prueba el nucleo.
internal static class Ejemplos
{
    // Numeros de las cartas.
    public const int Entradas = 1;
    public const int Principales = 2;
    public const int CartaPlus = 3;
    public const int CartaPremium = 4;
    public const int Bebidas = 5;
    public const int PostresYPromos = 6;

    // Codigos de los articulos.
    public const int Empanada = 1;
    public const int Guacamole = 2;
    public const int Provoleta = 3;
    public const int Milanesa = 4;
    public const int Ravioles = 5;
    public const int Paella = 6;
    public const int Bondiola = 7;
    public const int OjoDeBife = 8;
    public const int Salmon = 9;
    public const int Agua = 10;
    public const int Limonada = 11;
    public const int Cerveza = 12;
    public const int Tiramisu = 13;
    public const int ComboFamiliar = 14;
    public const int ComboOficina = 15;

    // Cuentas que usan las verificaciones.
    public const decimal TarifaBase = 600;
    public const decimal CobroConDemandaBaja = 600;
    public const decimal CobroConDemandaMedia = 750;
    public const decimal CobroConDemandaAlta = 960;
    public const decimal PrecioDeLaMilanesa = 9800;

    // Armo el catalogo: 6 cartas con articulos de distintos niveles.
    public static CatalogoDeCartas ArmarCatalogo()
    {
        Carta entradas = new Carta(Entradas, "Entradas");
        entradas.Sumar(new Articulo(Empanada, "Empanada de carne", 1200, 1));
        entradas.Sumar(new Articulo(Guacamole, "Guacamole con nachos", 3400, 1));
        entradas.Sumar(new Articulo(Provoleta, "Provoleta a la parrilla", 4200, 1));

        Carta principales = new Carta(Principales, "Platos principales");
        principales.Sumar(new Articulo(Milanesa, "Milanesa napolitana", 9800, 1));
        principales.Sumar(new Articulo(Ravioles, "Ravioles caseros", 7600, 1));

        Carta plus = new Carta(CartaPlus, "Carta Plus");
        plus.Sumar(new Articulo(Paella, "Paella para dos", 14500, 2));
        plus.Sumar(new Articulo(Bondiola, "Bondiola a la cerveza", 11200, 2));

        Carta premium = new Carta(CartaPremium, "Carta Premium");
        premium.Sumar(new Articulo(OjoDeBife, "Ojo de bife", 19800, 3));
        premium.Sumar(new Articulo(Salmon, "Salmon rosado", 17600, 3));

        Carta bebidas = new Carta(Bebidas, "Bebidas");
        bebidas.Sumar(new Articulo(Agua, "Agua con gas", 1600, 1));
        bebidas.Sumar(new Articulo(Limonada, "Limonada casera", 2400, 1));
        bebidas.Sumar(new Articulo(Cerveza, "Cerveza rubia", 3900, 1));

        Carta postres = new Carta(PostresYPromos, "Postres y promos");
        postres.Sumar(new Articulo(Tiramisu, "Tiramisu", 4300, 2));
        postres.Sumar(new Articulo(ComboFamiliar, "Combo familiar", 16500, 2));
        postres.Sumar(new Articulo(ComboOficina, "Combo oficina", 9800, 2));

        CatalogoDeCartas catalogo = new CatalogoDeCartas();

        catalogo.Sumar(entradas);
        catalogo.Sumar(principales);
        catalogo.Sumar(plus);
        catalogo.Sumar(premium);
        catalogo.Sumar(bebidas);
        catalogo.Sumar(postres);

        return catalogo;
    }

    // Los clientes de prueba. Sofia esta dada de baja.
    public static List<Cliente> ArmarClientes()
    {
        List<Cliente> clientes = new List<Cliente>();

        clientes.Add(new Cliente("Lucia", Plan.Premium));
        clientes.Add(new Cliente("Martin", Plan.Clasico));

        Cliente sofia = new Cliente("Sofia", Plan.Plus);
        sofia.Habilitado = false;

        clientes.Add(sofia);

        return clientes;
    }
}
