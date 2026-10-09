namespace YaLlegaNucleo.Modelo;

// Un articulo del catalogo.
public class Articulo
{
    public Articulo(int codigo, string detalle, decimal importe, int nivelNecesario)
    {
        if (importe <= 0) throw new ArgumentException("El importe tiene que ser mayor a cero");
        if (nivelNecesario < 1 || nivelNecesario > 3) throw new ArgumentException("El nivel tiene que ir de 1 a 3");

        Codigo = codigo;
        Detalle = detalle;
        Importe = importe;
        NivelNecesario = nivelNecesario;
    }

    public int Codigo { get; private set; }

    public string Detalle { get; private set; }

    public decimal Importe { get; private set; }

    // Nivel de plan que hace falta para poder pedirlo.
    public int NivelNecesario { get; private set; }

    // El nombre del plan necesario, para mostrar en pantalla y exportar.
    public string PlanNecesario
    {
        get
        {
            if (NivelNecesario >= 3) return Plan.Premium.Nombre;
            if (NivelNecesario == 2) return Plan.Plus.Nombre;

            return Plan.Clasico.Nombre;
        }
    }
}
