namespace YaLlegaNucleo.Modelo;

// Un cliente de la plataforma.
public class Cliente
{
    public Cliente(string nombre, Plan plan)
    {
        Nombre = nombre;
        Plan = plan;
        Habilitado = true;
    }

    public string Nombre { get; private set; }

    public Plan Plan { get; private set; }

    // Si esta habilitado puede hacer pedidos.
    public bool Habilitado { get; set; }
}
