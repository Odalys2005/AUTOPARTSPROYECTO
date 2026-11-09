using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AutoParts.View
{
    public sealed class FacturaDigitalService
    {
        private static readonly Uri UrlPublicacion = new Uri(
            "https://guiaqhvpitswyusrjpzj.supabase.co/functions/v1/publicar-factura");

        public async Task<string> PublicarAsync(
            string rutaPdf, string numeroFactura, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(rutaPdf) || !File.Exists(rutaPdf))
                throw new FileNotFoundException("No se encontró la factura PDF para publicar.", rutaPdf);

            if (!Regex.IsMatch(numeroFactura ?? "", @"\AFAC-\d{4}-\d{6,12}\z", RegexOptions.IgnoreCase))
                throw new ArgumentException("El número de factura no cumple el formato de publicación.");

            var archivo = new FileInfo(rutaPdf);
            if (archivo.Length <= 0 || archivo.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("El PDF debe medir entre 1 byte y 5 MB.");

            string clave = Environment.GetEnvironmentVariable("AUTOPARTS_UPLOAD_KEY") ?? "";
            if (clave.Length != 64 || !Regex.IsMatch(clave, @"\A[0-9a-fA-F]{64}\z"))
                throw new InvalidOperationException(
                    "Configura AUTOPARTS_UPLOAD_KEY en las variables de entorno de Windows y reinicia AutoParts.");

            using var cliente = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            using var solicitud = new HttpRequestMessage(HttpMethod.Post, UrlPublicacion);
            solicitud.Headers.TryAddWithoutValidation("x-autoparts-key", clave);

            await using var archivoStream = new FileStream(
                rutaPdf, FileMode.Open, FileAccess.Read, FileShare.Read,
                81920, useAsync: true);

            using var contenido = new MultipartFormDataContent();
            using var pdf = new StreamContent(archivoStream);
            pdf.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            contenido.Add(pdf, "archivo", Path.GetFileName(rutaPdf));
            contenido.Add(new StringContent(numeroFactura), "numeroFactura");
            solicitud.Content = contenido;

            using var respuesta = await cliente.SendAsync(
                solicitud, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            string cuerpo = await respuesta.Content.ReadAsStringAsync(cancellationToken);

            if (!respuesta.IsSuccessStatusCode)
            {
                string detalle = "HTTP " + (int)respuesta.StatusCode;
                try
                {
                    using var error = JsonDocument.Parse(cuerpo);
                    if (error.RootElement.TryGetProperty("mensaje", out var mensaje))
                        detalle += ": " + mensaje.GetString();
                }
                catch (JsonException) { }
                throw new InvalidOperationException("No se publicó el PDF. " + detalle);
            }

            using var documento = JsonDocument.Parse(cuerpo);
            if (!documento.RootElement.TryGetProperty("correcto", out var correcto) ||
                correcto.ValueKind != JsonValueKind.True ||
                !documento.RootElement.TryGetProperty("enlace", out var enlaceJson))
                throw new InvalidOperationException("La publicación no devolvió un enlace válido.");

            string enlace = enlaceJson.GetString() ?? "";
            if (!Uri.TryCreate(enlace, UriKind.Absolute, out var uri) ||
                uri.Scheme != Uri.UriSchemeHttps ||
                !string.Equals(uri.Host, UrlPublicacion.Host, StringComparison.OrdinalIgnoreCase) ||
                uri.AbsolutePath != "/functions/v1/consultar-factura")
                throw new InvalidOperationException("Supabase devolvió una URL inesperada.");

            return enlace;
        }
    }
}
