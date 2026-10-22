using AutoParts.DataAccess;
using AutoParts.Entity.Productos;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class SubCategoriaBL
    {
        private readonly SubCategoriaRepository _repository;

        public SubCategoriaBL(SubCategoriaRepository repository)
        {
            _repository = repository;
        }

        public List<Subcategoria> SelectAll()
        {
            List<Subcategoria> result = null;

            try
            {
                result = _repository.SelectAll();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public int Insert(Subcategoria entity)
        {
            int result;

            try
            {
                result = _repository.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }
    }
}