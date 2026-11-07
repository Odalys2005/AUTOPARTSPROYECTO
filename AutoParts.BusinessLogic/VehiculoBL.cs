
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class VehiculoBL
    {
        private readonly VehiculoRepository _repository;

        public VehiculoBL(VehiculoRepository repository)
        {
            _repository = repository
                ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<Vehiculo> SelectAll()
        {
            return _repository.SelectAll();
        }

        public int Insert(Vehiculo entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            ValidarVehiculo(entity);

            entity.EstadoId = 1;

            return _repository.Insert(entity);
        }

        public int Update(Vehiculo entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.VehiculoId <= 0)
                throw new ArgumentException(
                    "Selecciona un vehículo válido.");

            ValidarVehiculo(entity);

            return _repository.Update(entity);
        }

        public int Delete(Vehiculo entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.VehiculoId <= 0)
                throw new ArgumentException(
                    "Selecciona un vehículo válido.");

            return _repository.Delete(entity);
        }

        private static void ValidarVehiculo(Vehiculo entity)
        {
            if (entity.ModeloId <= 0)
                throw new ArgumentException(
                    "Selecciona un modelo válido.");

            if (entity.Anio < 1886)
                throw new ArgumentException(
                    "Ingresa un año de vehículo válido.");

            if (entity.Cilindrada <= 0 ||
                entity.Cilindrada > 999.99m ||
                decimal.Round(entity.Cilindrada, 2) != entity.Cilindrada)
                throw new ArgumentException(
                    "La cilindrada debe ser mayor que cero y no superar 999.99, con un máximo de dos decimales.");

            if (string.IsNullOrWhiteSpace(entity.TipoMotor))
                throw new ArgumentException(
                    "Ingresa el tipo de motor.");

            entity.TipoMotor = entity.TipoMotor.Trim();

            if (entity.TipoMotor.Length > 100)
                throw new ArgumentException(
                    "El tipo de motor no puede superar los 100 caracteres.");
        }
    }
}
