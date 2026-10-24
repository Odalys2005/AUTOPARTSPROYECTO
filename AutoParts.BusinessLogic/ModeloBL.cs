
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
            _repository = repository;
        }

        public List<Modelo> SelectAll()
        {
            List<Modelo> result = new List<Modelo>();

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

        public int Insert(Modelo entity)
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
    }
}
