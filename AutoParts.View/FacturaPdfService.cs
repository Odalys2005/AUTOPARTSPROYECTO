using AutoParts.BusinessLogic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace AutoParts.View
{
    public sealed class FacturaPdfService
    {
        private readonly VentaBL _ventaBL;
        private readonly Detalle_VentaBL _detalleVentaBL;
        private readonly ProductoBL _productoBL;
        private readonly ClienteBL _clienteBL;
        private readonly EmpleadoBL _empleadoBL;

        public FacturaPdfService(
            VentaBL ventaBL,
            Detalle_VentaBL detalleVentaBL,
            ProductoBL productoBL,
            ClienteBL clienteBL,
            EmpleadoBL empleadoBL)
        {
            _ventaBL = ventaBL;
            _detalleVentaBL = detalleVentaBL;
            _productoBL = productoBL;
            _clienteBL = clienteBL;
            _empleadoBL = empleadoBL;
        }

        public string Generar(int ventaId)
        {
            if (ventaId <= 0)
                throw new ArgumentException("Selecciona una venta válida.");

            var venta = _ventaBL.SelectAll().FirstOrDefault(v => v.VentaId == ventaId)
                ?? throw new InvalidOperationException("No existe la venta solicitada.");
            var detalles = _detalleVentaBL.SelectAll()
                .Where(d => d.VentaId == ventaId)
                .ToList();
            if (detalles.Count == 0)
                throw new InvalidOperationException("La venta no tiene detalles registrados.");

            var productos = _productoBL.SelectAll().ToDictionary(p => p.ProductoId);
            var cliente = _clienteBL.SelectAll().FirstOrDefault(c => c.ClienteId == venta.ClienteId);
            var empleado = _empleadoBL.SelectAll().FirstOrDefault(e => e.EmpleadoId == venta.EmpleadoId);
            var cultura = CultureInfo.GetCultureInfo("es-SV");
            string dinero(decimal valor) => "$" + valor.ToString("N2", cultura);

            decimal total = venta.Total;
            string nombreCliente = cliente == null ? "Cliente no identificado" :
                (cliente.Nombre + " " + cliente.Apellido).Trim();
            string nombreEmpleado = empleado == null ? "No identificado" :
                (empleado.Nombre + " " + empleado.Apellido).Trim();

            string carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AutoParts", "Facturas");
            Directory.CreateDirectory(carpeta);
            string numeroSeguro = new string((venta.NumeroFactura ?? "Venta-" + ventaId)
                .Select(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_')
                .ToArray());
            string ruta = Path.Combine(carpeta, "AutoParts_" + numeroSeguro + ".pdf");
            string rutaLogo = Path.Combine(AppContext.BaseDirectory, "Web", "logo-autoparts.png");

            Document.Create(documento =>
            {
                documento.Page(pagina =>
                {
                    pagina.Size(PageSizes.A4);
                    pagina.Margin(35);
                    pagina.PageColor(Colors.White);
                    pagina.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken4));

                    pagina.Header().Column(encabezado =>
                    {
                        encabezado.Item().Row(fila =>
                        {
                            fila.RelativeItem().Column(izquierda =>
                            {
                                if (File.Exists(rutaLogo))
                                {
                                    izquierda.Item().Height(60).Width(190).Image(rutaLogo).FitArea();
                                }
                                else
                                {
                                    izquierda.Item().Text(texto =>
                                    {
                                        texto.Span("Auto").Bold().FontSize(27).FontColor("#202127");
                                        texto.Span("Parts").Bold().FontSize(27).FontColor("#E4A722");
                                    });
                                }
                                izquierda.Item().Text("REPUESTOS Y ACCESORIOS").FontSize(10);
                                izquierda.Item().Text("COMPROBANTE INTERNO").FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                            fila.ConstantItem(175).AlignRight().Column(derecha =>
                            {
                                derecha.Item().AlignRight().Text("FACTURA DE VENTA").Bold().FontSize(13);
                                derecha.Item().AlignRight().Text(venta.NumeroFactura ?? "").FontSize(11);
                                derecha.Item().AlignRight().Text(
                                    venta.FechaVenta.ToString("dd/MM/yyyy HH:mm", cultura));
                            });
                        });
                        encabezado.Item().PaddingTop(14).LineHorizontal(2).LineColor("#A51D2D");
                    });

                    pagina.Content().PaddingVertical(18).Column(contenido =>
                    {
                        contenido.Spacing(12);
                        contenido.Item().Row(fila =>
                        {
                            fila.RelativeItem().Column(columna =>
                            {
                                columna.Item().Text("CLIENTE").Bold().FontColor("#A51D2D");
                                columna.Item().Text(nombreCliente);
                                if (cliente != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(cliente.DUI))
                                        columna.Item().Text("DUI: " + cliente.DUI);
                                    if (!string.IsNullOrWhiteSpace(cliente.Telefono))
                                        columna.Item().Text("Teléfono: " + cliente.Telefono);
                                }
                            });
                            fila.RelativeItem().Column(columna =>
                            {
                                columna.Item().Text("VENTA").Bold().FontColor("#A51D2D");
                                columna.Item().Text("Contado");
                                columna.Item().Text("Atendió: " + nombreEmpleado);
                            });
                        });

                        contenido.Item().PaddingTop(8).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columnas =>
                            {
                                columnas.ConstantColumn(45);
                                columnas.RelativeColumn(5);
                                columnas.ConstantColumn(68);
                                columnas.ConstantColumn(86);
                                columnas.ConstantColumn(80);
                            });
                            tabla.Header(header =>
                            {
                                foreach (var texto in new[] { "Cant.", "Producto", "P. unitario", "Descuento", "Importe" })
                                    header.Cell().Background("#F4E5E7").Padding(7)
                                        .Text(texto).Bold().FontSize(9);
                            });
                            foreach (var d in detalles)
                            {
                                string nombre = productos.TryGetValue(d.ProductoId, out var p)
                                    ? p.Nombre : "Producto #" + d.ProductoId;
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(7).Text(d.Cantidad.ToString(cultura));
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(7).Text(nombre);
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(7).AlignRight().Text(dinero(d.PrecioUnitario));
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(7).AlignRight().Text(dinero(d.Descuento ?? 0m));
                                tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(7).AlignRight().Text(dinero(d.Subtotal));
                            }
                        });

                        contenido.Item().AlignRight().Width(220).Column(totales =>
                        {
                            totales.Spacing(6);
                            totales.Item().LineHorizontal(1).LineColor("#A51D2D");
                            totales.Item().Row(fila =>
                            {
                                fila.RelativeItem().Text("TOTAL").Bold().FontSize(14);
                                fila.AutoItem().Text(dinero(total)).Bold().FontSize(14)
                                    .FontColor("#A51D2D");
                            });
                        });
                        contenido.Item().PaddingTop(15)
                            .Text("Las garantías aplican únicamente a defectos de fabricación, cuando el producto tenga cobertura vigente.")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    });

                    pagina.Footer().Column(pie =>
                    {
                        pie.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        pie.Item().PaddingTop(7).AlignCenter()
                            .Text("Gracias por su compra. Conserve este comprobante.")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });
            }).GeneratePdf(ruta);

            return ruta;
        }
    }

}