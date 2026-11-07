
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class CompatibilidadRepuestoBL
    {
        private readonly CompatibilidadRepuestoRepository _repository;

        public CompatibilidadRepuestoBL(
            CompatibilidadRepuestoRepository repository)
        {
            _repository = repository
                ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<CompatibilidadRepuesto> SelectAll()
        {
            return _repository.SelectAll();
        }

        public int Insert(CompatibilidadRepuesto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            ValidarCompatibilidad(entity);

            entity.EstadoId = 1;

            return _repository.Insert(entity);
        }

        public int Update(CompatibilidadRepuesto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.CompatibilidadId <= 0)
                throw new ArgumentException(
                    "Selecciona una compatibilidad válida.");

            ValidarCompatibilidad(entity);

            return _repository.Update(entity);
        }

        public int Delete(CompatibilidadRepuesto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.CompatibilidadId <= 0)
                throw new ArgumentException(
                    "Selecciona una compatibilidad válida.");

            return _repository.Delete(entity);
        }

        private static void ValidarCompatibilidad(
            CompatibilidadRepuesto entity)
        {
            if (entity.ProductoId <= 0)
                throw new ArgumentException(
                    "Selecciona un producto válido.");

            if (!entity.VehiculoId.HasValue ||
                entity.VehiculoId.Value <= 0)
                throw new ArgumentException(
                    "Selecciona un vehículo válido.");
        }
    }
}
