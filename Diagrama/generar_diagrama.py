"""Arma el HTML del diagrama de clases y despues se convierte a PDF con Chrome.

Uso:
    python generar_diagrama.py
"""

import html
import os

# Cada capa con sus clases. Cada clase: nombre, tipo, atributos y metodos.
CAPAS = [
    {
        "nombre": "Modelo (YaLlegaNucleo.Modelo)",
        "rol": "El modelo del negocio y los patrones Strategy, Proxy y Chain of Responsibility",
        "grupos": [
            {
                "titulo": "Entidades del negocio",
                "clases": [
                    ("Cliente", "clase",
                     ["- Nombre : string", "- Plan : Plan", "+ Habilitado : bool"], []),
                    ("Plan", "clase",
                     ["+ Clasico : Plan (estatico)", "+ Plus : Plan (estatico)",
                      "+ Premium : Plan (estatico)", "- Nombre : string", "- Nivel : int"], []),
                    ("NivelDeDemanda", "enum", ["Baja", "Media", "Alta"], []),
                    ("Articulo", "clase",
                     ["- Codigo : int", "- Detalle : string", "- Importe : decimal",
                      "- NivelNecesario : int", "+ PlanNecesario : string"], []),
                    ("Carta", "clase",
                     ["- Numero : int", "- Titulo : string", "- articulos : List<Articulo>",
                      "+ CuantosArticulos : int"], ["+ Sumar(articulo) : Carta"]),
                    ("CatalogoDeCartas", "clase", ["- cartas : List<Carta>"],
                     ["+ Sumar(carta)", "+ DameCarta(numero) : Carta",
                      "+ DameArticulo(codigo) : Articulo",
                      "+ TodosLosArticulos() : IReadOnlyList<Articulo>"]),
                    ("Orden", "clase",
                     ["- Numero : int", "- Cliente : Cliente",
                      "- Envio : CalculadoraDeEnvio", "- Abierta : bool",
                      "- lineas : List<LineaDeOrden>"],
                     ["+ Sumar(articulo, unidades)", "+ Sacar(articulo)",
                      "+ ImporteDeArticulos() : decimal", "+ ImporteTotal() : decimal",
                      "+ Cerrar()", "+ Tiene(articulo) : bool"]),
                    ("LineaDeOrden", "clase",
                     ["+ Articulo : Articulo", "+ Unidades : int", "+ Importe : decimal"], []),
                    ("CalculadoraDeEnvio", "clase",
                     ["- modalidad : IModalidadDeCobro", "- aplicadas : List<string>",
                      "+ TarifaBase : decimal", "+ Demanda : NivelDeDemanda",
                      "+ ModalidadActual : string"],
                     ["+ Cobrar() : decimal", "+ CambiarDemanda(demanda)",
                      "+ CambiarModalidad(modalidad)"]),
                    ("OrdenRegistrada", "clase",
                     ["+ Fecha : DateTime", "+ Cliente : string", "+ Articulos : string",
                      "+ TarifasDeEnvio : List<string>", "+ CartaServidaDesde : string",
                      "+ IntentosRechazados : int"], []),
                ],
            },
            {
                "titulo": "Carpeta Excepciones",
                "clases": [
                    ("AccesoNoPermitidoException", "clase", [], []),
                    ("ArticuloDesconocidoException", "clase", ["+ Codigo : int"], []),
                    ("ArticuloNoEstaEnLaOrdenException", "clase", ["+ Detalle : string"], []),
                    ("CantidadFueraDeRangoException", "clase", ["+ Unidades : int"], []),
                    ("OrdenYaCerradaException", "clase", ["+ Numero : int"], []),
                ],
            },
        ],
    },
    {
        "nombre": "Modelo / Cobros (Strategy)",
        "rol": "Formas de cobrar el envio, intercambiables mientras la orden sigue abierta",
        "grupos": [
            {
                "titulo": "Modalidades de cobro",
                "clases": [
                    ("IModalidadDeCobro", "interfaz", [],
                     ["+ Nombre : string",
                      "+ Calcular(tarifaBase, demanda) : decimal"]),
                    ("CobroPorDemanda", "clase",
                     ["- RecargoParaDemandaMedia : decimal",
                      "- RecargoParaDemandaAlta : decimal"],
                     ["+ Calcular(tarifaBase, demanda) : decimal"]),
                    ("CobroPromocional", "clase", [],
                     ["+ Calcular(tarifaBase, demanda) : decimal"]),
                ],
            },
        ],
    },
    {
        "nombre": "Modelo / Cartas (Proxy)",
        "rol": "Las cartas viven en un servidor remoto; el cache guarda las ultimas 5 en el dispositivo",
        "grupos": [
            {
                "titulo": "Origen de las cartas y cache",
                "clases": [
                    ("IOrigenDeCartas", "interfaz", [],
                     ["+ DameCarta(numero) : Carta", "+ UltimoOrigen : string"]),
                    ("RepositorioRemotoDeCartas", "clase",
                     ["- catalogo : CatalogoDeCartas", "+ Consultas : int"],
                     ["+ DameCarta(numero) : Carta"]),
                    ("CacheDeCartas", "clase",
                     ["- servidor : IOrigenDeCartas", "- cuantasGuarda : int",
                      "- guardadas : Dictionary<int, Carta>", "- ordenDeUso : List<int>",
                      "+ VecesQueSirvioElCache : int", "+ CartasGuardadas : int"],
                     ["+ DameCarta(numero) : Carta"]),
                ],
            },
        ],
    },
    {
        "nombre": "Modelo / Revision (Chain of Responsibility)",
        "rol": "Cadena de revisiones que impide pedir articulos fuera del plan del cliente",
        "grupos": [
            {
                "titulo": "Revisores",
                "clases": [
                    ("RevisionDeOrden", "clase",
                     ["+ Cliente : Cliente", "+ CodigoDeArticulo : int",
                      "+ Articulo : Articulo"], []),
                    ("IRevisorDeOrden", "interfaz", [],
                     ["+ Revisar(revision)", "+ Enganchar(siguiente) : IRevisorDeOrden"]),
                    ("RevisorDeOrden", "abstracta", ["- siguiente : IRevisorDeOrden"],
                     ["+ Revisar(revision)", "+ Enganchar(siguiente)",
                      "# Controlar(revision) *abstracto*"]),
                    ("RevisorDeArticulo", "clase", ["- catalogo : CatalogoDeCartas"],
                     ["# Controlar(revision)"]),
                    ("RevisorDeCliente", "clase", [], ["# Controlar(revision)"]),
                    ("RevisorDePlan", "clase", [], ["# Controlar(revision)"]),
                ],
            },
        ],
    },
    {
        "nombre": "Datos (YaLlegaNucleo.Datos)",
        "rol": "Guardado de las ordenes y escritura de los listados (Template Method)",
        "grupos": [
            {
                "titulo": "Guardado de las ordenes",
                "clases": [
                    ("IArchivoDeOrdenes", "interfaz", [],
                     ["+ Carpeta : string", "+ Ruta : string",
                      "+ Guardadas : IReadOnlyList<OrdenRegistrada>",
                      "+ Guardar(orden)", "+ LeerLineas() : IReadOnlyList<string>"]),
                    ("ArchivoDeOrdenes", "clase", ["- texto : TextoDeOrden"],
                     ["+ Guardar(orden)", "+ LeerLineas()"]),
                    ("TextoDeOrden", "clase", [], ["+ Armar(orden) : string"]),
                    ("ErrorDeArchivoException", "clase", ["+ Ruta : string"], []),
                ],
            },
            {
                "titulo": "Reportes",
                "clases": [
                    ("ReporteDeListado<T>", "abstracta", [],
                     ["+ Escribir(lista, rutaArchivo)",
                      "# Encabezado() : string *abstracto*",
                      "# Renglon(dato) : string *abstracto*"]),
                    ("ReporteAutomatico<T>", "clase", ["- Separador : string"],
                     ["# Encabezado() : string", "# Renglon(dato) : string"]),
                    ("ReporteDeOrdenes", "clase", [],
                     ["# Encabezado() : string", "# Renglon(dato) : string"]),
                ],
            },
        ],
    },
    {
        "nombre": "Negocio (YaLlegaNucleo.Negocio)",
        "rol": "Las reglas del negocio y el armado de todo el nucleo",
        "grupos": [
            {
                "titulo": "Servicios y armado",
                "clases": [
                    ("ServicioDeOrdenes", "clase",
                     ["- primerRevisor : IRevisorDeOrden", "- cartas : IOrigenDeCartas",
                      "- archivo : IArchivoDeOrdenes", "+ TarifaBaseDeEnvio : decimal",
                      "+ IntentosRechazados : int", "+ CartaServidaDesde : string"],
                     ["+ PedirCarta(numero) : Carta",
                      "+ NuevaOrden(numero, cliente, demanda)",
                      "+ Sumar(codigo, unidades)", "+ Sacar(codigo)",
                      "+ AvisarDemanda(demanda)", "+ PonerPromocion()",
                      "+ SacarPromocion()", "+ CerrarYGuardar() : OrdenRegistrada"]),
                    ("ServicioDeReportes", "clase", ["- carpeta : string"],
                     ["+ Escribir<T>(lista, nombreArchivo) : string",
                      "+ EscribirOrdenes(lista) : string"]),
                    ("Nucleo", "clase",
                     ["+ Ordenes : ServicioDeOrdenes", "+ Reportes : ServicioDeReportes",
                      "+ ConsultasAlServidor : int", "+ VecesQueSirvioElCache : int",
                      "+ CartasGuardadas : int"],
                     []),
                    ("ArmadorDelSistema", "clase", [],
                     ["+ Armar(catalogo, carpetaOrdenes, carpetaReportes, cuantasGuardar) : Nucleo"]),
                ],
            },
        ],
    },
    {
        "nombre": "Consola (YaLlegaNucleo.Consola)",
        "rol": "El ejemplo, las verificaciones y el menu",
        "grupos": [
            {
                "titulo": "Presentacion",
                "clases": [
                    ("Programa", "clase", [],
                     ["- Main(args) : int", "- Ejemplo()", "- MostrarMenuDeOpciones()",
                      "- MostrarOrden()"]),
                    ("Verificaciones", "clase", [],
                     ["+ Correr()", "- Comprobar(queSeProbo, salioBien)",
                      "- Rechaza<T>(queSeProbo, accion)"]),
                    ("Ejemplos", "clase", [],
                     ["+ ArmarCatalogo() : CatalogoDeCartas",
                      "+ ArmarClientes() : List<Cliente>"]),
                ],
            },
        ],
    },
]

# Relaciones entre las clases (las flechas del diagrama).
RELACIONES = [
    ("CalculadoraDeEnvio", "IModalidadDeCobro", "guarda una modalidad de cobro (Strategy)"),
    ("CobroPorDemanda", "IModalidadDeCobro", "implementa (tarifa base, +25 % o +60 % segun la demanda)"),
    ("CobroPromocional", "IModalidadDeCobro", "implementa (no se cobra el envio)"),
    ("Orden", "CalculadoraDeEnvio", "tiene una, aparte de los articulos"),
    ("Orden", "LineaDeOrden", "tiene una lista"),
    ("LineaDeOrden", "Articulo", "apunta a uno"),
    ("CatalogoDeCartas", "Carta", "tiene una lista"),
    ("Carta", "Articulo", "tiene una lista"),
    ("ServicioDeOrdenes", "OrdenRegistrada", "la arma cuando cierra la orden"),
    ("CacheDeCartas", "IOrigenDeCartas", "implementa (Proxy)"),
    ("RepositorioRemotoDeCartas", "IOrigenDeCartas", "implementa (sujeto real)"),
    ("CacheDeCartas", "RepositorioRemotoDeCartas", "lo envuelve y le agrega el cache"),
    ("CacheDeCartas", "Carta", "guarda las ultimas 5 que se pidieron"),
    ("RepositorioRemotoDeCartas", "CatalogoDeCartas", "busca las cartas en el catalogo"),
    ("RevisorDeArticulo", "RevisorDeOrden", "hereda"),
    ("RevisorDeCliente", "RevisorDeOrden", "hereda"),
    ("RevisorDePlan", "RevisorDeOrden", "hereda"),
    ("RevisorDeOrden", "IRevisorDeOrden", "implementa (Chain of Responsibility)"),
    ("RevisorDeOrden", "RevisionDeOrden", "la revisa y se la pasa al siguiente"),
    ("RevisorDeArticulo", "CatalogoDeCartas", "busca el articulo para completar la revision"),
    ("ArchivoDeOrdenes", "IArchivoDeOrdenes", "implementa"),
    ("ArchivoDeOrdenes", "TextoDeOrden", "la usa para armar cada linea"),
    ("ReporteAutomatico<T>", "ReporteDeListado<T>", "hereda y resuelve los pasos con reflexion"),
    ("ReporteDeOrdenes", "ReporteDeListado<T>", "hereda y usa sus propias columnas"),
    ("ServicioDeOrdenes", "IRevisorDeOrden", "arranca la cadena de revision"),
    ("ServicioDeOrdenes", "IOrigenDeCartas", "le pide las cartas (sin saber de donde salen)"),
    ("ServicioDeOrdenes", "IArchivoDeOrdenes", "guarda las ordenes"),
    ("ArmadorDelSistema", "Nucleo", "lo arma con todo conectado"),
]

COLORES = {
    "clase": "#0f766e",
    "abstracta": "#7c3aed",
    "interfaz": "#0369a1",
    "enum": "#b45309",
}


def clase_html(nombre, tipo, atributos, metodos):
    color = COLORES.get(tipo, "#0f766e")
    etiqueta = {"clase": "", "abstracta": "«abstracta»", "interfaz": "«interface»",
                "enum": "«enum»"}[tipo]

    partes = ['<div class="clase">']
    partes.append('<div class="nombre" style="background:%s">' % color)
    partes.append(html.escape(nombre))
    if etiqueta:
        partes.append('<span class="etiqueta">%s</span>' % etiqueta)
    partes.append("</div>")

    if atributos:
        partes.append('<div class="seccion">')
        for atributo in atributos:
            partes.append('<div class="miembro">%s</div>' % html.escape(atributo))
        partes.append("</div>")

    if metodos:
        partes.append('<div class="seccion">')
        for metodo in metodos:
            partes.append('<div class="miembro">%s</div>' % html.escape(metodo))
        partes.append("</div>")

    if not atributos and not metodos:
        partes.append('<div class="seccion vacia">(sin miembros)</div>')

    partes.append("</div>")

    return "\n".join(partes)


def main():
    partes = []

    partes.append("""<!DOCTYPE html>
<html lang="es">
<head>
<meta charset="utf-8">
<title>Diagrama de clases - YaLlega S.A.</title>
<style>
  @page { size: A4 portrait; margin: 11mm; }
  * { box-sizing: border-box; }
  body { font-family: "Segoe UI", Arial, sans-serif; color: #111827; font-size: 8.5pt; margin: 0; }
  h1 { font-size: 15pt; margin: 0 0 2px 0; }
  .sub { color: #4b5563; font-size: 9pt; margin-bottom: 2px; }
  .datos { color: #6b7280; font-size: 8pt; margin-bottom: 12px; }
  h2 { font-size: 11pt; margin: 14px 0 4px 0; padding: 4px 8px; background: #f0fdfa;
       border-left: 4px solid #0f766e; page-break-after: avoid; }
  .rol { color: #6b7280; font-size: 8pt; margin: 0 0 6px 12px; }
  h3 { font-size: 9pt; margin: 10px 0 4px 0; color: #374151; page-break-after: avoid; }
  .fila { display: flex; flex-wrap: wrap; gap: 7px; }
  .clase { border: 1px solid #9ca3af; border-radius: 3px; width: 182px;
           page-break-inside: avoid; background: #fff; }
  .clase .nombre { color: #fff; font-weight: 600; padding: 3px 6px; font-size: 8.5pt;
                   line-height: 1.25; }
  .clase .nombre .etiqueta { display: block; font-weight: 400; font-size: 7pt; opacity: 0.85; }
  .seccion { padding: 3px 6px; border-top: 1px solid #e5e7eb; }
  .seccion.vacia { color: #9ca3af; font-style: italic; }
  .miembro { line-height: 1.35; color: #1f2937; }
  table { border-collapse: collapse; width: 100%; font-size: 8pt; margin-top: 6px; }
  th, td { border: 1px solid #d1d5db; padding: 3px 5px; text-align: left; }
  th { background: #f0fdfa; }
  .leyenda { margin-top: 10px; font-size: 8pt; color: #4b5563; }
  .leyenda b { color: #111827; }
  .corte { page-break-before: always; }
</style>
</head>
<body>
<h1>YaLlega S.A. &mdash; Diagrama de clases</h1>
<div class="sub">Nucleo de la plataforma de pedidos y entregas &mdash; arquitectura de 4 capas</div>
<div class="datos">
  Facultad de Tecnologia Informatica &middot; Ingenieria en Sistemas &middot;
  Practica de parcial &middot; Alumno: Dante Schlogl<br>
  Patrones usados: <b>Strategy</b> (costo de envio) &middot;
  <b>Proxy</b> (cache de las ultimas 5 cartas) &middot;
  <b>Chain of Responsibility</b> (revision por plan) &middot;
  <b>Template Method</b> (reportes genericos)
</div>
""")

    for capa in CAPAS:
        partes.append("<h2>%s</h2>" % html.escape(capa["nombre"]))
        partes.append('<div class="rol">%s</div>' % html.escape(capa["rol"]))

        for grupo in capa["grupos"]:
            partes.append("<h3>%s</h3>" % html.escape(grupo["titulo"]))
            partes.append('<div class="fila">')

            for nombre, tipo, atributos, metodos in grupo["clases"]:
                partes.append(clase_html(nombre, tipo, atributos, metodos))

            partes.append("</div>")

    partes.append('<div class="corte"></div>')
    partes.append("<h2>Relaciones entre las clases</h2>")
    partes.append("<table>")
    partes.append("<tr><th style='width:28%'>Origen</th><th style='width:28%'>Destino</th>"
                  "<th>Relacion</th></tr>")

    for origen, destino, relacion in RELACIONES:
        partes.append("<tr><td>%s</td><td>%s</td><td>%s</td></tr>"
                      % (html.escape(origen), html.escape(destino), html.escape(relacion)))

    partes.append("</table>")

    partes.append("""
<div class="leyenda">
  <p><b>Como leer el diagrama.</b> Las flechas estan detalladas en la tabla de arriba. Las
  dependencias van en un solo sentido: Consola &rarr; Negocio &rarr; Datos &rarr; Modelo.</p>
  <p><b>Strategy.</b> El costo de envio no lo calcula <code>Orden</code>: se lo pregunta a una
  <code>CalculadoraDeEnvio</code>, que tiene guardada una <code>IModalidadDeCobro</code>. Hay dos:
  <code>CobroPorDemanda</code> (tarifa base, +25 % o +60 % segun la demanda) y
  <code>CobroPromocional</code> (no se cobra). Como la modalidad es un objeto aparte, se puede
  cambiar mientras la orden sigue abierta.</p>
  <p><b>Proxy.</b> La consola siempre le pide las cartas a <code>IOrigenDeCartas</code>.
  <code>RepositorioRemotoDeCartas</code> es el servidor de verdad y <code>CacheDeCartas</code> lo
  envuelve: primero mira lo que tiene guardado y, si no esta, se lo pide al servidor. Como tiene la
  misma interfaz, la consola <b>no distingue</b> de donde salio la carta. El cache guarda 5 como
  maximo y va descartando la mas vieja.</p>
  <p><b>Chain of Responsibility.</b> <code>RevisorDeOrden</code> guarda el revisor siguiente y le
  pasa la <code>RevisionDeOrden</code>. Cada revisor escribe solamente su control y tira excepcion
  si algo no esta permitido. Los tres son: el articulo existe, el cliente esta habilitado y el plan
  alcanza. Para agregar otro control se agrega un revisor y se engancha, sin tocar los demas.</p>
  <p><b>Template Method.</b> <code>ReporteDeListado&lt;T&gt;.Escribir()</code> tiene <b>la unica
  rutina</b>: encabezado, renglones y pie. Lo unico que cambia son <code>Encabezado()</code> y
  <code>Renglon()</code>. <code>ReporteAutomatico&lt;T&gt;</code> los resuelve con reflexion, asi que
  sirve para cualquier clase, incluso las que se agreguen despues.
  <code>ReporteDeOrdenes</code> es el ejemplo de columnas propias.</p>
</div>
</body>
</html>
""")

    salida = os.path.join(os.path.dirname(os.path.abspath(__file__)), "diagrama.html")

    with open(salida, "w", encoding="utf-8") as f:
        f.write("\n".join(partes))

    print(salida)


if __name__ == "__main__":
    main()
