namespace YaLlegaNucleo.Modelo;

// Una orden que ya se cerro. Esto es lo que se guarda en el archivo.
public class OrdenRegistrada
{
    public DateTime Fecha { get; set; }

    public string Cliente { get; set; } = "";

    public string Articulos { get; set; } = "";

    public List<string> TarifasDeEnvio { get; set; } = new List<string>();

    public string CartaServidaDesde { get; set; } = "";

    public int IntentosRechazados { get; set; }
}
