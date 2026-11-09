using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AutoParts.View
{
    public sealed class FacturaCorreoService
    {
        private static readonly SemaphoreSlim Candado = new SemaphoreSlim(1, 1);
        private static readonly string RutaEnvios = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AutoParts", "Facturas", "correos-enviados.json");

        public async Task<bool> EnviarSiPendienteAsync(
            int ventaId,
            string numeroFactura,
            string correoDestino,
            string nombreCliente,
            string rutaPdf,
            string enlaceDigital)
        {
            if (ventaId <= 0)
                throw new ArgumentException("La venta no es válida.");
            if (string.IsNullOrWhiteSpace(numeroFactura))
                throw new ArgumentException("La factura no es válida.");
            if (string.IsNullOrWhiteSpace(rutaPdf) || !File.Exists(rutaPdf))
                throw new FileNotFoundException("No se encontró el PDF de la factura.", rutaPdf);
            if (!Uri.TryCreate(enlaceDigital, UriKind.Absolute, out var enlace) ||
                enlace.Scheme != Uri.UriSchemeHttps ||
                !enlace.Host.Equals("guiaqhvpitswyusrjpzj.supabase.co", StringComparison.OrdinalIgnoreCase) ||
                !enlace.AbsolutePath.Equals("/functions/v1/consultar-factura", StringComparison.Ordinal))
                throw new ArgumentException("El enlace digital no es válido.");
            if (!MailAddress.TryCreate(correoDestino, out var destinatario) ||
                !string.Equals(destinatario.Address, correoDestino.Trim(), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("El correo del cliente no es válido.");

            string gmail = Environment.GetEnvironmentVariable("AUTOPARTS_GMAIL") ?? "";
            string clave = Environment.GetEnvironmentVariable("AUTOPARTS_GMAIL_APP_PASSWORD") ?? "";
            if (string.IsNullOrWhiteSpace(gmail) || string.IsNullOrWhiteSpace(clave))
                throw new InvalidOperationException("Configura AUTOPARTS_GMAIL y AUTOPARTS_GMAIL_APP_PASSWORD en Windows y reinicia AutoParts.");
            if (!MailAddress.TryCreate(gmail, out var remitente) ||
                !remitente.Host.Equals("gmail.com", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("AUTOPARTS_GMAIL debe ser una cuenta Gmail válida.");

            await Candado.WaitAsync();
            try
            {
                Dictionary<string, string> enviados = LeerEnviados();
                string identificador = ventaId.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (enviados.ContainsKey(identificador))
                    return false;

                using var mensaje = new MailMessage();
                mensaje.From = new MailAddress(remitente.Address, "AutoParts", Encoding.UTF8);
                mensaje.To.Add(destinatario);
                mensaje.Subject = "AutoParts - Factura " + numeroFactura;
                string saludo = string.IsNullOrWhiteSpace(nombreCliente) ? "Estimado cliente" : nombreCliente.Trim();
                mensaje.Body = "Hola, " + saludo + ":\r\n\r\n" +
                    "Gracias por tu compra en AutoParts.\r\n\r\n" +
                    "Adjuntamos tu comprobante interno en PDF.\r\n" +
                    "Factura: " + numeroFactura + "\r\n\r\n" +
                    "También puedes consultar tu factura digital aquí:\r\n" +
                    enlaceDigital + "\r\n\r\n" +
                    "AutoParts - Repuestos y Accesorios\r\n";
                mensaje.BodyEncoding = Encoding.UTF8;
                mensaje.SubjectEncoding = Encoding.UTF8;
                mensaje.Attachments.Add(new Attachment(rutaPdf, "application/pdf"));

                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(remitente.Address, clave),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 45000
                };
                await smtp.SendMailAsync(mensaje);

                enviados[identificador] = DateTimeOffset.UtcNow.ToString("O");
                string carpeta = Path.GetDirectoryName(RutaEnvios)!;
                Directory.CreateDirectory(carpeta);
                string temporal = RutaEnvios + ".tmp";
                File.WriteAllText(temporal, JsonSerializer.Serialize(enviados));
                File.Move(temporal, RutaEnvios, true);
                return true;
            }
            finally
            {
                Candado.Release();
            }
        }

        private static Dictionary<string, string> LeerEnviados()
        {
            if (!File.Exists(RutaEnvios))
                return new Dictionary<string, string>();
            return JsonSerializer.Deserialize<Dictionary<string, string>>(
                File.ReadAllText(RutaEnvios)) ?? new Dictionary<string, string>();
        }
    }
}