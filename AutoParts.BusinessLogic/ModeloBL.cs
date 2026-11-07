using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class ModeloBL
    {
        private readonly ModeloRepository _repository;

        public ModeloBL(ModeloRepository repository)
        {
            _repository = repository
                ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<Modelo> SelectAll()
        {
            return _repository.SelectAll();
        }

        public int Insert(Modelo entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            ValidarDatosModelo(entity);

            entity.EstadoId = 1;

            return _repository.Insert(entity);
        }

        public int Update(Modelo entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.ModeloId <= 0)
                throw new ArgumentException("Selecciona un modelo válido.");

            ValidarDatosModelo(entity);
            return _repository.Update(entity);
        }

        public int Delete(Modelo entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.ModeloId <= 0)
                throw new ArgumentException("Selecciona un modelo válido.");

            
            return _repository.Delete(entity);
        }

        private static void ValidarDatosModelo(Modelo entity)
        {
            if (entity.MarcaId <= 0)
                throw new ArgumentException("Selecciona una marca válida.");

            if (string.IsNullOrWhiteSpace(entity.Nombre))
                throw new ArgumentException("Ingresa el nombre del modelo.");

            entity.Nombre = entity.Nombre.Trim();

            if (entity.Nombre.Length > 100)
                throw new ArgumentException("El nombre del modelo no puede superar los 100 caracteres.");
        }
    }
}