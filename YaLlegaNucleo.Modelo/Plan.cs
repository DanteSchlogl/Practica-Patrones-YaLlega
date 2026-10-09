namespace YaLlegaNucleo.Modelo;

// Los planes de suscripcion que ofrece la plataforma.
// Los arme como objetos y no como enum porque cada plan tiene un nombre y un nivel,
// y asi el dia de manana se puede agregar uno nuevo sin tocar las comparaciones.
public class Plan
{
    public static readonly Plan Clasico = new Plan("Clasico", 1);
    public static readonly Plan Plus = new Plan("Plus", 2);
    public static readonly Plan Premium = new Plan("Premium", 3);

    private Plan(string nombre, int nivel)
    {
        Nombre = nombre;
        Nivel = nivel;
    }

    public string Nombre { get; private set; }

    public int Nivel { get; private set; }

    public override string ToString()
    {
        return Nombre;
    }
}
