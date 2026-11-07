using System;

namespace AutoParts.BusinessLogic
{
    public static class PermisosSistema
    {
        public const int RolAdministrador = 1;
        public const int RolVendedor = 2;
        public const int RolTecnico = 3;

        public static bool EsAdministrador(SesionUsuario? sesion)
        {
            return sesion != null && sesion.RolId == RolAdministrador;
        }

        public static bool PuedeAcceder(SesionUsuario? sesion, string modulo)
        {
            if (sesion == null || string.IsNullOrWhiteSpace(modulo))
                return false;

            if (EsAdministrador(sesion))
                return true;

            string nombre = NormalizarModulo(modulo);

            if (sesion.RolId == RolVendedor)
            {
                return nombre == "dashboard" ||
                       nombre == "productos" ||
                       nombre == "inventario" ||
                       nombre == "clientes" ||
                       nombre == "ventas" ||
                       nombre == "detalles-venta";
            }

            if (sesion.RolId == RolTecnico)
            {
                return nombre == "dashboard" ||
                       nombre == "productos" ||
                       nombre == "inventario" ||
                       nombre == "clientes" ||
                       nombre == "ventas" ||
                       nombre == "detalles-venta" ||
                       nombre == "garantias" ||
                       nombre == "reemplazos-garantia";
            }

            return false;
        }

        public static bool PuedeRealizar(SesionUsuario? sesion, string modulo, string accion)
        {
            if (sesion == null || string.IsNullOrWhiteSpace(modulo) ||
                string.IsNullOrWhiteSpace(accion))
                return false;

            if (EsAdministrador(sesion))
                return true;

            string nombre = NormalizarModulo(modulo);
            string operacion = accion.Trim().ToLowerInvariant();

            if (!PuedeAcceder(sesion, nombre))
                return false;

            if (nombre == "dashboard")
                return operacion == "solicitarresumen";

            if (nombre == "productos" || nombre == "inventario" ||
                nombre == "ventas" || nombre == "clientes" ||
                nombre == "detalles-venta")
            {
                if (operacion == "selectall" || operacion == "selectbyid")
                    return true;
            }

            if (sesion.RolId == RolVendedor)
            {
                if (nombre == "ventas")
                    return operacion == "registrarcompleta" ||
                           operacion == "imprimirticket" ||
                           operacion == "abrirnuevo";

                if (nombre == "clientes")
                    return operacion == "guardarintegrado" ||
                           operacion == "actualizarintegrado" ||
                           operacion == "desactivarintegrado" ||
                           operacion == "abrirnuevo";
            }

            if (sesion.RolId == RolTecnico)
            {
                if (nombre == "garantias")
                    return operacion == "selectall" ||
                           operacion == "selectbyid" ||
                           operacion == "registrarsolicitud" ||
                           operacion == "evaluar";

                if (nombre == "reemplazos-garantia")
                    return operacion == "selectall" ||
                           operacion == "selectbyid" ||
                           operacion == "entregar";
            }

            return false;
        }

        private static string NormalizarModulo(string modulo)
        {
            return modulo.Trim().ToLowerInvariant() switch
            {
                "inicio" => "dashboard",
                "detalles de venta" => "detalles-venta",
                "garantías" => "garantias",
                "reemplazos de garantía" => "reemplazos-garantia",
                _ => modulo.Trim().ToLowerInvariant()
            };
        }
    }
}