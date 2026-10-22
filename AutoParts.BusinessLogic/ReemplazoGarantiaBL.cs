using AutoParts.DataAccess;
using AutoParts.Entidades;
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
            List<ReemplazoGarantia> result = null;

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

        public int Insert(ReemplazoGarantia entity)
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