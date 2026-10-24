
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class UsuarioBL
    {
        private readonly UsuarioRepository _repository;

        public UsuarioBL(UsuarioRepository repository)
        {
            _repository = repository;
        }

        public List<Usuario> SelectAll()
        {
            return _repository.SelectAll();
        }

        public int Insert(Usuario entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.NombreUsuario))
                throw new ArgumentException(
                    "Debe ingresar un nombre de usuario.");

            if (string.IsNullOrWhiteSpace(entity.Contrasena))
                throw new ArgumentException(
                    "Debe ingresar una contraseña.");

            // El repositorio almacena un hash, nunca texto plano.
            Usuario nuevo = new Usuario(
                entity.UsuarioId,
                entity.EmpleadoId,
                entity.RolId,
                entity.NombreUsuario.Trim(),
                PasswordHelper.CrearHash(entity.Contrasena),
                entity.EstadoId
            );

            return _repository.Insert(nuevo);
        }

        public SesionUsuario? IniciarSesion(
            string nombreUsuario,
            string contrasena)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) ||
                string.IsNullOrEmpty(contrasena))
            {
                return null;
            }

            Usuario? usuario = _repository.SelectByNombreUsuario(
                nombreUsuario.Trim());

            if (usuario == null)
                return null;

            if (!PasswordHelper.Verificar(
                contrasena, usuario.Contrasena))
            {
                return null;
            }

            return new SesionUsuario
            {
                UsuarioId = usuario.UsuarioId,
                EmpleadoId = usuario.EmpleadoId,
                RolId = usuario.RolId,
                NombreUsuario = usuario.NombreUsuario
            };
        }
    }

    public class SesionUsuario
    {
        public int UsuarioId { get; set; }
        public int EmpleadoId { get; set; }
        public int RolId { get; set; }
        public string NombreUsuario { get; set; } = "";
    }
}
