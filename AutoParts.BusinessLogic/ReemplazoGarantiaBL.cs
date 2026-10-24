
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class ReemplazoGarantiaBL
    {
        private readonly ReemplazoGarantiaRepository _repository;

        public ReemplazoGarantiaBL(ReemplazoGarantiaRepository repository)
        {
            _repository = repository;
        }

        public List<ReemplazoGarantia> SelectAll()
        {
            List<ReemplazoGarantia> result = new List<ReemplazoGarantia>();

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

        public int Insert(ReemplazoGarantia entity)
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
