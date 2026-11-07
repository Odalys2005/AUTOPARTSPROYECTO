using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace AutoParts.BusinessLogic
{
    public class UsuarioBL
    {
        private readonly UsuarioRepository _repository;

        public UsuarioBL(UsuarioRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<Usuario> SelectAll()
        {
            return _repository.SelectAll();
        }

        public int Insert(Usuario entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            ValidarDatos(entity);

            if (string.IsNullOrWhiteSpace(entity.Contrasena) ||
                entity.Contrasena.Length < 8 || entity.Contrasena.Length > 72)
                throw new ArgumentException("La contraseña debe tener entre 8 y 72 caracteres.");

            Usuario nuevo = new Usuario(
                0,
                entity.EmpleadoId,
                entity.RolId,
                entity.NombreUsuario.Trim(),
                PasswordHelper.CrearHash(entity.Contrasena),
                1
            );

            return _repository.Insert(nuevo);
        }

        public int Update(Usuario entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.UsuarioId <= 0)
                throw new ArgumentException("Selecciona un usuario válido.");

            ValidarDatos(entity);

            string hashNuevo = string.Empty;
            if (!string.IsNullOrEmpty(entity.Contrasena))
            {
                if (string.IsNullOrWhiteSpace(entity.Contrasena) ||
                    entity.Contrasena.Length < 8 || entity.Contrasena.Length > 72)
                    throw new ArgumentException("La contraseña nueva debe tener entre 8 y 72 caracteres.");

                hashNuevo = PasswordHelper.CrearHash(entity.Contrasena);
            }

            Usuario modificado = new Usuario(
                entity.UsuarioId,
                entity.EmpleadoId,
                entity.RolId,
                entity.NombreUsuario.Trim(),
                hashNuevo,
                entity.EstadoId
            );

            return _repository.Update(modificado);
        }

        public int Delete(Usuario entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.UsuarioId <= 0)
                throw new ArgumentException("Selecciona un usuario válido.");

            return _repository.Delete(entity);
        }


        public SesionUsuario? ValidarSesion(int usuarioId)
        {
            if (usuarioId < 0)
                return null;

            Usuario? usuario = _repository.ValidarSesion(usuarioId);

            if (usuario == null)
                return null;

            return new SesionUsuario
            {
                UsuarioId = usuario.UsuarioId,
                EmpleadoId = usuario.EmpleadoId,
                RolId = usuario.RolId,
                NombreUsuario = usuario.NombreUsuario
            };
        }


        public SesionUsuario? IniciarSesion(string nombreUsuario, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) ||
                string.IsNullOrEmpty(contrasena))
                return null;

            Usuario? usuario = _repository.SelectByNombreUsuario(nombreUsuario.Trim());

            if (usuario == null || usuario.EstadoId != 1)
                return null;

            if (!PasswordHelper.Verificar(contrasena, usuario.Contrasena))
                return null;

            return new SesionUsuario
            {
                UsuarioId = usuario.UsuarioId,
                EmpleadoId = usuario.EmpleadoId,
                RolId = usuario.RolId,
                NombreUsuario = usuario.NombreUsuario
            };
        }

        private static void ValidarDatos(Usuario entity)
        {
            if (entity.EmpleadoId <= 0)
                throw new ArgumentException("Selecciona un empleado válido.");

            if (entity.RolId <= 0)
                throw new ArgumentException("Selecciona un rol válido.");

            if (string.IsNullOrWhiteSpace(entity.NombreUsuario))
                throw new ArgumentException("El nombre de usuario es obligatorio.");

            string nombre = entity.NombreUsuario.Trim();
            if (nombre.Length < 3 || nombre.Length > 50)
                throw new ArgumentException("El nombre de usuario debe tener entre 3 y 50 caracteres.");
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