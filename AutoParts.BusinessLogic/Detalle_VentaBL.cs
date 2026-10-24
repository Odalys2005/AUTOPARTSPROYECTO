
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class Detalle_VentaBL
    {
        private readonly Detalle_VentaRepository _repository;

        public Detalle_VentaBL(Detalle_VentaRepository repository)
        {
            _repository = repository;
        }

        public List<Detalle_Venta> SelectAll()
        {
            List<Detalle_Venta> result = new List<Detalle_Venta>();

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

        public int Insert(Detalle_Venta entity)
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
