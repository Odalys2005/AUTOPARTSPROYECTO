using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class VehiculoBL
    {
        private readonly VehiculoRepository _repository;

        public VehiculoBL(VehiculoRepository repository)
        {
            _repository = repository;
        }

        public List<Vehiculo> SelectAll()
        {
            List<Vehiculo> result = null;

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

        public int Insert(Vehiculo entity)
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