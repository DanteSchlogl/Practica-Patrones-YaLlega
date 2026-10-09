# Práctica de parcial — Patrones de diseño

**FACULTAD:** Tecnología Informática
**CARRERA:** Ingeniería en Sistemas
**ASIGNATURA:** Trabajo de diploma
**TURNO:** Noche
**MODALIDAD:** Presencial

---

## Ejercicio práctico

La empresa **YaLlega S.A.** necesita desarrollar el **núcleo de su plataforma de pedidos y entregas a
domicilio**.

### El costo de envío de cada pedido se ajusta según la situación

- Con **demanda baja** se aplica la **tarifa normal**.
- Con **demanda media** se aplica la tarifa con un **recargo del 25 %**.
- Con **demanda alta** (hora pico, lluvia) se aplica la tarifa con un **recargo del 60 %**.
- En **modo promoción** el envío es **siempre gratuito**, sin importar la demanda.

El sistema **mide la demanda de la zona cada cierto tiempo** y debe poder **cambiar la forma de
calcular el envío mientras el pedido todavía está abierto** (el cliente sigue agregando productos).
El **operador del local** también puede **forzar un modo manualmente** con sólo presionar un botón.

### El catálogo

El **catálogo de productos está alojado en un servidor remoto**. El sistema debe:

- **impedir pedir productos que no estén incluidos en el plan de suscripción del cliente**
  (clásico, plus o premium), y
- **mantener en el dispositivo los últimos 5 menús consultados** para no volver a descargarlos.

La **pantalla de armado del pedido no debe distinguir** si el menú viene del servidor o del
dispositivo.

### La exportación

El área de **marketing** necesita **exportar cualquier listado del sistema** (productos, clientes,
pedidos) a un archivo de texto con encabezados, mediante **una única rutina de exportación que
funcione con cualquier clase, incluso con las que se agreguen en el futuro**.

Se pide **diseñar y desarrollar un sistema** para dar soporte a esta estructura, **utilizando
patrones de diseño** para tal fin. **Realizar diagrama de clases y código correspondiente.**

### Persistencia

**Persistir** en una base de datos, serialización o **archivo con formato personalizado** cada
pedido:

| Dato | Ejemplo |
|---|---|
| Cliente | `Lucia` |
| Productos | `2x Milanesa napolitana, 1x Agua con gas` |
| Tarifas de envío aplicadas durante el pedido | `... demanda Baja -> $600.00 >> ... demanda Alta -> $960.00` |
| Si el menú se sirvió desde el servidor o desde el dispositivo | `carta: Servidor` |
| Intentos rechazados por plan | `rechazados: 2` |

> **IMPORTANTE:** Utilizar una arquitectura de **3 o más capas**. Los datos deberán estar
> **normalizados** si se utiliza base de datos relacional.
>
> **Realizar casos de prueba para cada uno de los escenarios planteados.**

<br>

**Firma:** ______________________

---

## Lectura del enunciado: de qué párrafo sale cada decisión

| Párrafo del enunciado | Qué hace falta | Con qué se resolvió |
|---|---|---|
| "El costo de envío se ajusta según la situación": normal, +25 %, +60 % y gratis en promoción | Varias formas de calcular lo mismo | **Strategy**: `IModalidadDeCobro` con `CobroPorDemanda` y `CobroPromocional` |
| "Cambiar la forma de calcular el envío **mientras el pedido todavía está abierto**" | Poder cambiar el cálculo en caliente | `CalculadoraDeEnvio.CambiarDemanda()` y `CambiarModalidad()` |
| "El **operador del local** puede forzar un modo manualmente" | Un camino aparte para forzar la modalidad | `ServicioDeOrdenes.PonerPromocion()` y `SacarPromocion()` |
| "El catálogo está alojado en un **servidor remoto**" | Un origen de datos lejano que conviene no consultar de más | **Proxy**: `RepositorioRemotoDeCartas` es el sujeto real |
| "Mantener en el dispositivo los **últimos 5 menús** consultados para no volver a descargarlos" | Guardar una cantidad limitada de lo último usado | `CacheDeCartas` con tope de 5 y descarte de la más vieja |
| "La pantalla **no debe distinguir** si el menú viene del servidor o del dispositivo" | La pantalla no puede cambiar | El cache implementa **la misma interfaz** `IOrigenDeCartas` |
| "Impedir pedir productos que no estén incluidos en el **plan** del cliente" | Varios controles que pueden crecer | **Chain of Responsibility**: existe → habilitado → plan |
| "Exportar **cualquier listado**... mediante **una única rutina**... que funcione con **cualquier clase**, incluso las que se agreguen en el futuro" | Una sola rutina, sin escribir código por tipo | **Template Method** + `ReporteAutomatico<T>` con reflexión |
| "Persistir... **tarifas de envío aplicadas durante el pedido**" | Guardar la evolución, no sólo el valor final | `CalculadoraDeEnvio.Aplicadas` (lista, se anota cada cambio) |
| "**Intentos rechazados por plan**" | Contarlos en algún lado | `ServicioDeOrdenes.IntentosRechazados`, se guarda con la orden |
| "Arquitectura de **3 o más capas**" | Separar responsabilidades | **4 capas**: Consola, Negocio, Datos, Modelo |
| "Los datos deberán estar **normalizados** si se utiliza base de datos relacional" | — | Como se usa **archivo con formato personalizado**, no aplica |
| "Realizar **casos de prueba** para cada escenario" | Probar cada requerimiento | `Verificaciones`: 45 comprobaciones automáticas |
| "Realizar **diagrama de clases**" | Documentar el diseño | `Diagrama/DiagramaDeClases.pdf` |
