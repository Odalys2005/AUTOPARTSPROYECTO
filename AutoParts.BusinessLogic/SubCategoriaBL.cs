
using AutoParts.DataAccess;
using AutoParts.Entity.Productos;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class SubcategoriaBL
    {
        private readonly SubcategoriaRepository _repository;

        public SubcategoriaBL(SubcategoriaRepository repository)
        {
            _repository = repository;
        }

        public List<Subcategoria> SelectAll()
        {
            return _repository.SelectAll();
        }

        public int Insert(Subcategoria entity)
        {
            Validar(entity);

            entity.EstadoId = 1;

            return _repository.Insert(entity);
        }

        public int Update(Subcategoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.SubcategoriaId <= 0)
                throw new ArgumentException(
                    "Selecciona una subcategoría válida.");

            Validar(entity);

            return _repository.Update(entity);
        }

        public int Delete(Subcategoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.SubcategoriaId <= 0)
                throw new ArgumentException(
                    "Selecciona una subcategoría válida.");

            return _repository.Delete(entity);
        }

        private static void Validar(Subcategoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.CategoriaId <= 0)
                throw new ArgumentException(
                    "Selecciona una categoría válida.");

            if (string.IsNullOrWhiteSpace(entity.Nombre))
                throw new ArgumentException(
                    "El nombre de la subcategoría es obligatorio.");

            entity.Nombre = entity.Nombre.Trim();

            if (entity.Nombre.Length > 120)
                throw new ArgumentException(
                    "El nombre de la subcategoría no puede superar los 120 caracteres.");
        }
    }
}
