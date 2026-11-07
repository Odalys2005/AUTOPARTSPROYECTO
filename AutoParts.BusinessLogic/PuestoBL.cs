using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class PuestoBL
    {
        private readonly PuestoRepository _repository;

        public PuestoBL(PuestoRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<Puesto> SelectAll()
        {
            return _repository.SelectAll();
        }

        public int Insert(Puesto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            ValidarCampos(entity);
            entity.EstadoId = 1;

            return _repository.Insert(entity);
        }

        public int Update(Puesto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.PuestoId <= 0)
                throw new ArgumentException("Selecciona un puesto válido.");

            ValidarCampos(entity);

            return _repository.Update(entity);
        }

        public int Delete(Puesto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.PuestoId <= 0)
                throw new ArgumentException("Selecciona un puesto válido.");

            return _repository.Delete(entity);
        }

        private static void ValidarCampos(Puesto entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Nombre))
                throw new ArgumentException("El nombre del puesto es obligatorio.");

            entity.Nombre = entity.Nombre.Trim();

            if (entity.Nombre.Length > 100)
                throw new ArgumentException("El nombre no puede superar los 100 caracteres.");

            if (string.IsNullOrWhiteSpace(entity.Descripcion))
                throw new ArgumentException("La descripción del puesto es obligatoria.");

            entity.Descripcion = entity.Descripcion.Trim();

            if (entity.Descripcion.Length > 500)
                throw new ArgumentException("La descripción no puede superar los 500 caracteres.");
        }
    }
}