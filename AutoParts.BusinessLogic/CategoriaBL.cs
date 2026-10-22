
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
            List<Categoria> result = null;

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

        public int Insert(Categoria entity)
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
