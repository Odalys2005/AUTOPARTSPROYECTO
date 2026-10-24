
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class GarantiaBL
    {
        private readonly GarantiaRepository _repository;

        public GarantiaBL(GarantiaRepository repository)
        {
            _repository = repository;
        }

        public List<Garantia> SelectAll()
        {
            List<Garantia> result = new List<Garantia>();

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

        public int Insert(Garantia entity)
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
