
using AutoParts.DataAccess;
using AutoParts.Entity;
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
            List<Subcategoria> result = new List<Subcategoria>();

            try
            {
                result = _repository.SelectAll();
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        public int Insert(Subcategoria entity)
        {
            int result = 0;

            try
            {
                result = _repository.Insert(entity);
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        public int Update(Subcategoria entity)
        {
            int result = 0;

            try
            {
                result = _repository.Update(entity);
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        public int Delete(Subcategoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.SubcategoriaId <= 0)
                throw new ArgumentException(
                    "Selecciona una SubCategoria válida.");

            return _repository.Delete(entity);
        }
    }
}
