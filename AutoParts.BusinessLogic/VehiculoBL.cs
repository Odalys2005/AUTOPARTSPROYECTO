
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
            List<Vehiculo> result = new List<Vehiculo>();

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

        public int Insert(Vehiculo entity)
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
