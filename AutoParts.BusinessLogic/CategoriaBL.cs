
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class CategoriaBL
    {
        private readonly CategoriaRepository _repository;

        public CategoriaBL(CategoriaRepository repository)
        {
            _repository = repository;
        }

        public List<Categoria> SelectAll()
        {
            return _repository.SelectAll();
        }

        public int Insert(Categoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.Nombre))
                throw new ArgumentException(
                    "El nombre de la categoría es obligatorio.");

            entity.Nombre = entity.Nombre.Trim();

            if (entity.Nombre.Length > 100)
                throw new ArgumentException(
                    "El nombre no puede superar los 100 caracteres.");

            entity.EstadoId = 1;

            return _repository.Insert(entity);
        }

        public int Update(Categoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.CategoriaId <= 0)
                throw new ArgumentException(
                    "Selecciona una categoría válida.");

            if (string.IsNullOrWhiteSpace(entity.Nombre))
                throw new ArgumentException(
                    "El nombre de la categoría es obligatorio.");

            entity.Nombre = entity.Nombre.Trim();

            if (entity.Nombre.Length > 100)
                throw new ArgumentException(
                    "El nombre no puede superar los 100 caracteres.");

            return _repository.Update(entity);
        }

        public int Delete(Categoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.CategoriaId <= 0)
                throw new ArgumentException(
                    "Selecciona una categoría válida.");

            return _repository.Delete(entity);
        }
    }
}
