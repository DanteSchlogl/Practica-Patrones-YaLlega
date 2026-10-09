namespace YaLlegaNucleo.Modelo.Cartas;

// De donde se consiguen las cartas.
// La pantalla de armado usa esta interfaz y no sabe si la carta viene del servidor
// o del dispositivo: de eso se encarga el cache.
public interface IOrigenDeCartas
{
    // Busca una carta. Devuelve null si no existe.
    Carta? DameCarta(int numero);

    // De donde salio la ultima carta que se pidio: "Servidor" o "Dispositivo".
    string UltimoOrigen { get; }
}
