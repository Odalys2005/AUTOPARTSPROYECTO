using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class MarcaBL
    {
        private readonly MarcaRepository _repository;

        public MarcaBL(MarcaRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<Marca> SelectAll()
        {
            return _repository.SelectAll();
        }

        public int Insert(Marca entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Nombre = ValidarNombre(entity.Nombre);
            entity.EstadoId = 1;

            return _repository.Insert(entity);
        }

        public int Update(Marca entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.MarcaId <= 0)
                throw new ArgumentException("Selecciona una marca válida.");

            entity.Nombre = ValidarNombre(entity.Nombre);

            return _repository.Update(entity);
        }

        public int Delete(Marca entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.MarcaId <= 0)
                throw new ArgumentException("Selecciona una marca válida.");

            return _repository.Delete(entity);
        }

        private static string ValidarNombre(string? nombre)
        {
            string texto = (nombre ?? string.Empty).Trim();

            if (texto.Length == 0)
                throw new ArgumentException("El nombre de la marca es obligatorio.");

            if (texto.Length > 100)
                throw new ArgumentException("El nombre de la marca no puede superar 100 caracteres.");

            return texto;
        }
    }
}