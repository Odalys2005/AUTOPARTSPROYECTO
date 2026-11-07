
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class ProveedorBL
    {
        private readonly ProveedorRepository _repository;

        public ProveedorBL(ProveedorRepository repository)
        {
            _repository = repository;
        }

        public List<Proveedor> SelectAll()
        {
            return _repository.SelectAll();
        }

        public int Insert(Proveedor entity)
        {
            Validar(entity);

            entity.EstadoId = 1;

            return _repository.Insert(entity);
        }

        public int Update(Proveedor entity)
        {
            Validar(entity);

            if (entity.ProveedorId <= 0)
            {
                throw new ArgumentException(
                    "Selecciona un proveedor válido.");
            }

            return _repository.Update(entity);
        }

        public int Delete(Proveedor entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.ProveedorId <= 0)
            {
                throw new ArgumentException(
                    "Selecciona un proveedor válido.");
            }

            return _repository.Delete(entity);
        }

        private void Validar(Proveedor entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrWhiteSpace(entity.NombreEmpresa))
            {
                throw new ArgumentException(
                    "El nombre de la empresa es obligatorio.");
            }

            if (entity.NombreEmpresa.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El nombre de la empresa no puede superar los 100 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(entity.NIT))
            {
                throw new ArgumentException(
                    "El NIT es obligatorio.");
            }

            if (entity.NIT.Trim().Length > 20)
            {
                throw new ArgumentException(
                    "El NIT no puede superar los 20 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(entity.Telefono))
            {
                throw new ArgumentException(
                    "El teléfono es obligatorio.");
            }

            if (entity.Telefono.Trim().Length > 25)
            {
                throw new ArgumentException(
                    "El teléfono no puede superar los 25 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(entity.Correo))
            {
                throw new ArgumentException(
                    "El correo es obligatorio.");
            }

            if (entity.Correo.Trim().Length > 150)
            {
                throw new ArgumentException(
                    "El correo no puede superar los 150 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(entity.Contacto))
            {
                throw new ArgumentException(
                    "El contacto es obligatorio.");
            }

            if (entity.Contacto.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El contacto no puede superar los 100 caracteres.");
            }

            entity.NombreEmpresa = entity.NombreEmpresa.Trim();
            entity.NIT = entity.NIT.Trim();
            entity.Telefono = entity.Telefono.Trim();
            entity.Correo = entity.Correo.Trim();
            entity.Contacto = entity.Contacto.Trim();
        }
    }
}
