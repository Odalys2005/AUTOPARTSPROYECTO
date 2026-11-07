
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class RolBL
    {
        private readonly RolRepository _repository;

        public RolBL(RolRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<Rol> SelectAll()
        {
            return _repository.SelectAll();
        }

        public int Insert(Rol entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.Nombre))
                throw new ArgumentException("El nombre del rol es obligatorio.");

            entity.Nombre = entity.Nombre.Trim();

            if (entity.Nombre.Length > 100)
                throw new ArgumentException("El nombre no puede superar los 100 caracteres.");

            entity.EstadoId = 1;

            return _repository.Insert(entity);
        }

        public int Update(Rol entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.RolId <= 0)
                throw new ArgumentException("Selecciona un rol válido.");

            if (string.IsNullOrWhiteSpace(entity.Nombre))
                throw new ArgumentException("El nombre del rol es obligatorio.");

            entity.Nombre = entity.Nombre.Trim();

            if (entity.Nombre.Length > 100)
                throw new ArgumentException("El nombre no puede superar los 100 caracteres.");

            return _repository.Update(entity);
        }

        public int Delete(Rol entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.RolId <= 0)
                throw new ArgumentException("Selecciona un rol válido.");

            return _repository.Delete(entity);
        }
    }
}
