# YaLlega S.A. — Núcleo de la plataforma de pedidos y entregas

**Práctica de parcial — Patrones de diseño**

| Dato | Valor |
|---|---|
| Facultad | Tecnología Informática |
| Carrera | Ingeniería en Sistemas |
| Asignatura | Trabajo de diploma |
| Instancia | Práctica de parcial — Turno Noche |
| Lenguaje / IDE | C# / Visual Studio |
| Framework | .NET 8 |
| Arquitectura | 4 capas: Consola – Negocio – Datos – Modelo |

📄 **[Enunciado completo](Enunciado.md)** · 📐 **[Diagrama de clases (PDF)](Diagrama/DiagramaDeClases.pdf)**

---

## 1. Los patrones que se usaron

| Patrón | Para qué | Clases |
|---|---|---|
| **Strategy** | El envío se cobra de 4 formas distintas y la forma puede cambiar mientras la orden está abierta | `IModalidadDeCobro`, `CobroPorDemanda`, `CobroPromocional` |
| **Proxy** | El catálogo está en un servidor remoto, pero las últimas 5 cartas quedan en el dispositivo y la consola no debe notarlo | `IOrigenDeCartas`, `RepositorioRemotoDeCartas`, `CacheDeCartas` |
| **Chain of Responsibility** | Impedir pedir artículos fuera del plan del cliente, con controles que pueden crecer | `IRevisorDeOrden`, `RevisorDeOrden` y los 3 revisores |
| **Template Method** | Escribir **cualquier** listado con **una sola rutina**, incluso de clases futuras | `ReporteDeListado<T>`, `ReporteAutomatico<T>`, `ReporteDeOrdenes` |
| *(además)* **Factory Method** | Armar el núcleo completo sin que la consola conozca las otras capas | `ArmadorDelSistema` |

---

## 2. Cómo ejecutar

### Desde Visual Studio

1. Abrir `YaLlegaNucleo.sln`.
2. Verificar que `YaLlegaNucleo.Consola` sea el proyecto de inicio.
3. `F5` (o `Ctrl+F5`).

### Desde la línea de comandos

```bash
dotnet build YaLlegaNucleo.sln -c Release

# Ejemplo + verificaciones + menú
dotnet run --project YaLlegaNucleo.Consola -c Release

# Sólo el ejemplo y las verificaciones, sin menú
dotnet run --project YaLlegaNucleo.Consola -c Release -- --sin-menu
```

**Requisito:** .NET SDK 8.0 o superior.

### Regenerar el PDF del diagrama

```bash
python Diagrama/generar_diagrama.py
# después: abrir Diagrama/diagrama.html y "Imprimir > Guardar como PDF"
```

---

## 3. Arquitectura en 4 capas

```
   Consola      YaLlegaNucleo.Consola    Programa, Verificaciones, Ejemplos
                    |
                    v
   Negocio      YaLlegaNucleo.Negocio    ServicioDeOrdenes, ServicioDeReportes, Nucleo, ArmadorDelSistema
                    |
                    v
   Datos        YaLlegaNucleo.Datos      ArchivoDeOrdenes, TextoDeOrden, Reportes
                    |
                    v
   Modelo       YaLlegaNucleo.Modelo     El negocio: Orden, CalculadoraDeEnvio, Cartas, Revisiones
```

| Capa | Responsabilidad | Depende de |
|---|---|---|
| **Modelo** | El modelo del negocio y los tres patrones. No sabe nada de archivos ni de pantalla. | *(ninguna)* |
| **Datos** | Guardar las órdenes en un archivo y escribir los listados. | Modelo |
| **Negocio** | Las reglas: revisar, calcular el envío, guardar. Es la única puerta de entrada para la consola. | Modelo, Datos |
| **Consola** | El ejemplo, las verificaciones y el menú. | Negocio, Modelo |

Las dependencias van **siempre en un solo sentido** (Consola → Negocio → Datos → Modelo).

---

## 4. Patrón por patrón

### 4.1 Strategy — el costo de envío

**El problema.** El envío se cobra de cuatro formas distintas y la forma puede cambiar **mientras el
cliente sigue armando la orden** (porque cambió la demanda de la zona o porque el operador apretó un
botón). Si las cuatro formas estuvieran escritas con `if` adentro de la orden, cada forma nueva
obligaría a modificar la orden otra vez.

**La solución.** Cada forma de cobrar es un objeto que implementa `IModalidadDeCobro`, y la orden no
calcula nada: se lo pregunta a la calculadora.

```csharp
public interface IModalidadDeCobro
{
    string Nombre { get; }
    decimal Calcular(decimal tarifaBase, NivelDeDemanda demanda);
}
```

| Modalidad | Qué hace |
|---|---|
| `CobroPorDemanda` | Baja → tarifa base. Media → **+25 %**. Alta → **+60 %** |
| `CobroPromocional` | Devuelve **0** siempre |

**Ejemplos con la tarifa base de $600:**

| Situación | Cuenta | Resultado |
|---|---|---|
| Demanda baja | 600 | **$600,00** |
| Demanda media | 600 × 1,25 | **$750,00** |
| Demanda alta (hora pico, lluvia) | 600 × 1,60 | **$960,00** |
| Modo promoción | 0 | **$0,00** |

**La decisión de diseño de esta versión:** el costo de envío **no vive dentro de la orden**, vive en
`CalculadoraDeEnvio`, que la orden tiene como un atributo más. Así la orden se ocupa de los artículos
y la calculadora del envío, y cada una se puede probar por separado.

```csharp
orden.Envio.Cobrar();                          // cuánto se cobra ahora
orden.Envio.CambiarDemanda(NivelDeDemanda.Alta);   // lo llama el sistema cuando mide la zona
orden.Envio.CambiarModalidad(new CobroPromocional());  // lo llama el operador con el botón
```

Cada cambio **queda anotado** en `CalculadoraDeEnvio.Aplicadas`, así después se pueden guardar
**todas** las tarifas que se aplicaron durante la orden, no sólo la última.

### 4.2 Proxy — la caché de las últimas 5 cartas

**El problema.** El catálogo está en un **servidor remoto**, así que cada consulta cuesta. Hay que
evitar volver a bajar lo último que ya se miró, **pero la consola no puede cambiar**: no debe saber
de dónde salió la carta.

**La solución.** `IOrigenDeCartas` es lo único que conoce la pantalla de armado.
`RepositorioRemotoDeCartas` es el sujeto real y `CacheDeCartas` **lo envuelve** con la misma
interfaz.

```
        pantalla de armado
               |
               v
        IOrigenDeCartas  <------------- (la consola sólo conoce esto)
               ^
               |
        CacheDeCartas  --- si no está guardada --->  RepositorioRemotoDeCartas
        (últimas 5 cartas)                              (catálogo completo)
```

| Requerimiento del enunciado | Cómo se resuelve |
|---|---|
| Mantener los **últimos 5** menús consultados | El cache guarda 5; si entra una nueva, **descarta la más vieja** |
| No volver a descargarlos | Si la carta está guardada, ni se llama al servidor |
| La pantalla **no debe distinguir** el origen | El cache tiene la misma interfaz, así que la consola no cambia ni una línea |
| Saber de dónde salió (para persistir) | El cache publica `UltimoOrigen` ("Servidor" o "Dispositivo") |

**Cómo se prueba que funciona:** `RepositorioRemotoDeCartas` cuenta las consultas. Pedir la misma
carta dos veces da **una sola consulta** y un acierto de caché.

### 4.3 Chain of Responsibility — la revisión por plan

**El problema.** Hay que **impedir pedir artículos que no estén en el plan del cliente**, y los
controles pueden crecer. Con todos los `if` en un solo método, cada control nuevo lo modifica.

**La solución.** Cada control es un **revisor** que revisa y se la pasa al siguiente; si algo no está
permitido, **tira excepción y la cadena se corta ahí**.

| Revisor | Qué controla | Si falla |
|---|---|---|
| `RevisorDeArticulo` | Que el artículo figure en alguna carta | `ArticuloDesconocidoException` |
| `RevisorDeCliente` | Que el cliente esté habilitado | `AccesoNoPermitidoException` |
| `RevisorDePlan` | Que el plan del cliente alcance para ese artículo | `AccesoNoPermitidoException` |

Se arma en el armador, y se lee como una frase:

```csharp
RevisorDeArticulo primero = new RevisorDeArticulo(catalogo);

primero.Enganchar(new RevisorDeCliente()).Enganchar(new RevisorDePlan());
```

Cada revisor escribe **solamente su `Controlar`**; de pasarle la revisión al siguiente se encarga
`RevisorDeOrden`. **Si mañana hay que agregar un control** (por ejemplo "el local está abierto"), se
agrega un revisor y se engancha: **no se toca ninguno de los otros**.

### 4.4 Template Method — los reportes genéricos

**El problema.** Marketing quiere exportar **cualquier listado** (artículos, clientes, órdenes) a un
archivo con encabezados, con **una única rutina** que funcione con **cualquier clase, incluso las que
se agreguen en el futuro**. Si cada listado tuviera su propio método, cada clase nueva obligaría a
escribir código nuevo.

**La solución.** `ReporteDeListado<T>.Escribir()` tiene **el esqueleto fijo**, y lo único que cambia
son dos pasos:

```csharp
public abstract class ReporteDeListado<T>
{
    // LA única rutina: encabezado, renglones y pie.
    public void Escribir(IEnumerable<T> lista, string rutaArchivo)
    {
        ...
        texto.Add(Encabezado());            // <-- paso que cambia

        foreach (T renglon in renglones)
            texto.Add(Renglon(renglon));    // <-- paso que cambia

        texto.Add("Cantidad de registros; " + renglones.Count);
        File.WriteAllLines(rutaArchivo, texto);
    }

    protected abstract string Encabezado();
    protected abstract string Renglon(T dato);
}
```

`ReporteAutomatico<T>` resuelve esos dos pasos **con reflexión**: saca los nombres de las propiedades
como encabezado y sus valores como renglones. **Por eso sirve para cualquier clase, sin escribir
nada nuevo.**

```csharp
// La misma llamada para listados de tipos distintos.
nucleo.Reportes.Escribir(nucleo.Ordenes.Guardadas(), "ordenes.txt");
nucleo.Reportes.Escribir(clientes, "clientes.txt");
nucleo.Reportes.Escribir(catalogo.Cartas, "cartas.txt");
```

Resultado real:

```
Nombre; Plan; Habilitado
Lucia; Premium; True
Martin; Clasico; True
Sofia; Plus; False
Cantidad de registros; 3
```

Y `ReporteDeOrdenes` muestra la otra cara del patrón: si se quieren **columnas propias**, se hereda y
se cambian sólo esos dos pasos, **sin tocar la rutina de escritura**.

> **Trade-off para la defensa.** El reporte automático depende de los **nombres de las propiedades**:
> si alguien renombra una, cambia el encabezado. Es el precio de que sirva para cualquier clase, y por
> eso está también `ReporteDeOrdenes` con las columnas escritas a mano. Tener las dos versiones es la
> respuesta correcta: la automática para lo nuevo, la hecha a medida para el formato estable.

---

## 5. Persistencia

Cada orden se guarda en `Ordenes/ordenes.log`, una línea por orden con formato propio:

```
ORDEN; 08/10/2026 20:15:31; cliente: Lucia; articulos: 2x Milanesa napolitana, 1x Agua con gas;
tarifas: Cobro por demanda con demanda Baja -> $600.00 >> Cobro por demanda con demanda Alta -> $960.00;
carta: Dispositivo; rechazados: 2
```

| Lo que pide el enunciado | Dato en el archivo |
|---|---|
| Cliente | `cliente: Lucia` |
| Productos | `articulos: 2x Milanesa napolitana, 1x Agua con gas` |
| **Tarifas de envío aplicadas durante el pedido** | `tarifas: ... demanda Baja -> $600.00 >> ... demanda Alta -> $960.00` |
| Si el menú se sirvió desde el servidor o del dispositivo | `carta: Dispositivo` |
| Intentos rechazados por plan | `rechazados: 2` |

**Manejo de archivos:** la carpeta se crea sola si no existe, se escribe con *append* y un `lock`
evita que dos escrituras al mismo tiempo rompan el archivo. Si algo falla se lanza
`ErrorDeArchivoException` con la ruta absoluta y la excepción original adentro.

---

## 6. Casos de prueba

El programa corre **45 comprobaciones automáticas** y muestra `bien` o `MAL` por cada una. Cada una
arma su propio núcleo en su propia carpeta, así no se mezclan.

| Escenario | Comprobaciones |
|---|---|
| **El cobro del envío** | demanda baja = $600; media = $750; alta = $960; promoción = $0; el envío se recalcula durante la orden y quedan anotadas las dos tarifas; el operador pone y saca la promoción; el total es artículos + envío |
| **El cache de cartas** | la primera vez baja del servidor y la segunda sale del dispositivo; una sola consulta; guarda 5 como máximo; la más vieja se descarta y se vuelve a bajar |
| **La revisión del pedido** | artículo desconocido; cliente dado de baja; clásico no llega a plus ni a premium; clásico sí llega a clásico; plus llega a plus pero no a premium; premium llega a premium; se cuentan los rechazos |
| **Otras validaciones** | cero unidades y unidades negativas; sacar un artículo que no está en la orden; una orden cerrada no acepta más artículos |
| **Lo que se guarda** | una línea por orden; la línea tiene los cinco datos; se guarda si la carta vino del dispositivo; los intentos rechazados quedan guardados |
| **Los reportes** | los tres listados con la misma rutina; el encabezado sale del nombre de las propiedades; el reporte a medida usa sus columnas |

Resultado de la última ejecución: **45 bien, 0 mal**.

---

## 7. Puntos que pide el enunciado

| Lo que pide | Dónde se cumple |
|---|---|
| Usar **patrones de diseño** | Strategy, Proxy, Chain of Responsibility y Template Method |
| **Cambiar la forma de calcular el envío** mientras la orden está abierta | `CalculadoraDeEnvio.CambiarDemanda()` y `CambiarModalidad()` |
| El **operador** puede forzar un modo con un botón | `ServicioDeOrdenes.PonerPromocion()` y `SacarPromocion()` |
| **Impedir** pedir artículos fuera del plan | Cadena de 3 revisores |
| Mantener las **últimas 5 cartas** en el dispositivo | `CacheDeCartas` con descarte de la más vieja |
| La pantalla **no debe distinguir** el origen | El cache implementa la misma interfaz `IOrigenDeCartas` |
| **Una única rutina** para cualquier listado | `ReporteDeListado<T>.Escribir()` + `ReporteAutomatico<T>` con reflexión |
| **Persistir** cada orden con los cinco datos | `ArchivoDeOrdenes` con formato propio |
| **Arquitectura de 3 o más capas** | 4 capas |
| **Normalización** de datos | No aplica: se usa **archivo con formato personalizado**, no base de datos relacional |
| **Casos de prueba** de cada escenario | 45 comprobaciones automáticas |
| **Diagrama de clases** | `Diagrama/DiagramaDeClases.pdf` |
| Compila sin errores **ni advertencias** | `dotnet build YaLlegaNucleo.sln -c Release` → 0 y 0 |

---

## 8. Estructura del repositorio

```
YaLlegaNucleo/
├── YaLlegaNucleo.sln
├── Enunciado.md
├── README.md
├── Diagrama/
│   ├── DiagramaDeClases.pdf          <- entregable: PDF con el diagrama de clases
│   └── generar_diagrama.py           <- script que arma el diagrama
├── Ordenes/                          <- se genera al ejecutar
├── Reportes/                         <- se genera al ejecutar
│
├── YaLlegaNucleo.Modelo/             <- capa de modelo (sin dependencias)
│   ├── Cliente.cs, Orden.cs, LineaDeOrden.cs
│   ├── CalculadoraDeEnvio.cs, OrdenRegistrada.cs
│   ├── Articulo.cs, Carta.cs, CatalogoDeCartas.cs
│   ├── Plan.cs, NivelDeDemanda.cs
│   ├── Cobros/                       <- Strategy
│   │   ├── IModalidadDeCobro.cs, CobroPorDemanda.cs, CobroPromocional.cs
│   ├── Cartas/                       <- Proxy
│   │   ├── IOrigenDeCartas.cs, RepositorioRemotoDeCartas.cs, CacheDeCartas.cs
│   ├── Revision/                     <- Chain of Responsibility
│   │   ├── RevisionDeOrden.cs, IRevisorDeOrden.cs, RevisorDeOrden.cs
│   │   ├── RevisorDeArticulo.cs, RevisorDeCliente.cs, RevisorDePlan.cs
│   └── Excepciones/
│       ├── AccesoNoPermitidoException.cs
│       ├── ArticuloDesconocidoException.cs
│       ├── ArticuloNoEstaEnLaOrdenException.cs
│       ├── CantidadFueraDeRangoException.cs
│       └── OrdenYaCerradaException.cs
│
├── YaLlegaNucleo.Datos/              <- capa de datos
│   ├── IArchivoDeOrdenes.cs, ArchivoDeOrdenes.cs, TextoDeOrden.cs
│   ├── Reportes/                     <- Template Method
│   │   ├── ReporteDeListado.cs, ReporteAutomatico.cs, ReporteDeOrdenes.cs
│   └── Excepciones/ErrorDeArchivoException.cs
│
├── YaLlegaNucleo.Negocio/            <- capa de negocio
│   ├── ServicioDeOrdenes.cs
│   ├── ServicioDeReportes.cs
│   ├── Nucleo.cs
│   └── ArmadorDelSistema.cs
│
└── YaLlegaNucleo.Consola/            <- capa de presentación (el ejecutable)
    ├── Programa.cs
    ├── Verificaciones.cs
    └── Ejemplos.cs
```

---

## 9. Salida esperada

El envío se recalcula solo cuando cambia la demanda:

```
Orden 1 de Lucia (abierta)
  2x Empanada de carne -> $2400.00
  1x Agua con gas -> $1600.00
  Importe de articulos: $4000.00
  Envio (Cobro por demanda, demanda Baja): $600.00
  IMPORTE TOTAL: $4600.00
  Tarifas que se aplicaron: Cobro por demanda con demanda Baja -> $600.00 >> Cobro por demanda con demanda Alta -> $960.00
```

Y el cache se nota en los contadores:

```
Lucia pide la misma carta otra vez: la consola no lo nota, pero sale del dispositivo.
  La carta salio del: Dispositivo
  Consultas al servidor: 1 | veces que sirvio el cache: 1 | cartas guardadas: 1 de 5
```

---

## 10. Menú

| Opción | Acción |
|---|---|
| 1 | Ver las cartas |
| 2 | Ver los artículos de una carta (dice de dónde salió) |
| 3 | Abrir una orden |
| 4 | Sumar un artículo |
| 5 | Sacar un artículo |
| 6 | Avisar la demanda (baja / media / alta) |
| 7 | Poner promoción |
| 8 | Sacar promoción |
| 9 | Ver la orden |
| 10 | Cerrar y guardar |
| 11 | Ver el archivo |
| 12 | Escribir los reportes |
| 13 | Correr las verificaciones |
| 0 | Salir |
