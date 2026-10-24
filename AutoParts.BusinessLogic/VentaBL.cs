
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class VentaBL
    {
        private readonly VentaRepository _repository;

        public VentaBL(VentaRepository repository)
        {
            _repository = repository;
        }

        public List<Venta> SelectAll()
        {
            List<Venta> result = new List<Venta>();

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

        public int Insert(Venta entity)
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
