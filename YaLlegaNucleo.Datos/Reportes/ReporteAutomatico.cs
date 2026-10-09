using System.Reflection;

namespace YaLlegaNucleo.Datos.Reportes;

// Implementacion que sirve para CUALQUIER clase, incluso las que se agreguen mas
// adelante, porque saca las columnas con reflexion. No hay que escribir nada nuevo.
public class ReporteAutomatico<T> : ReporteDeListado<T>
{
    private const string Separador = "; ";

    protected override string Encabezado()
    {
        List<string> columnas = new List<string>();

        foreach (PropertyInfo dato in typeof(T).GetProperties())
        {
            columnas.Add(dato.Name);
        }

        return string.Join(Separador, columnas);
    }

    protected override string Renglon(T elemento)
    {
        List<string> valores = new List<string>();

        foreach (PropertyInfo dato in typeof(T).GetProperties())
        {
            valores.Add(ComoTexto(dato.GetValue(elemento)));
        }

        return string.Join(Separador, valores);
    }

    // Las listas las junto con flechas y el resto lo paso a texto.
    private static string ComoTexto(object? valor)
    {
        if (valor == null) return "";

        if (valor is IEnumerable<string> lista) return string.Join(" >> ", lista);

        return valor.ToString() ?? "";
    }
}
