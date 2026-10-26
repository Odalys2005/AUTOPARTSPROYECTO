
using System;

namespace AutoParts.BusinessLogic
{
    public static class PermisosSistema
    {
        // Rol administrador de AutoParts.
        // Según los registros actuales, Administrador = RolId 1.
        public const int RolAdministrador = 1;

        public static bool EsAdministrador(
            SesionUsuario? sesion)
        {
            return sesion != null &&
                   sesion.RolId == RolAdministrador;
        }

        public static bool PuedeAcceder(
            SesionUsuario? sesion,
            string modulo)
        {
            if (sesion == null)
                return false;

            // Por ahora solo habilitamos al administrador.
            return EsAdministrador(sesion);
        }

        public static bool PuedeRealizar(
            SesionUsuario? sesion,
            string modulo,
            string accion)
        {
            if (sesion == null)
                return false;

            // El administrador dispone de todos los permisos.
            return EsAdministrador(sesion);
        }
    }
}
