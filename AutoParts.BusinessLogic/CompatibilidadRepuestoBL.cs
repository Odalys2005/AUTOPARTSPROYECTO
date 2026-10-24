
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class CompatibilidadRepuestoBL
    {
        private readonly CompatibilidadRepuestoRepository _repository;

        public CompatibilidadRepuestoBL(CompatibilidadRepuestoRepository repository)
        {
            _repository = repository;
        }

        public List<CompatibilidadRepuesto> SelectAll()
        {
            List<CompatibilidadRepuesto> result = new List<CompatibilidadRepuesto>();

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

        public int Insert(CompatibilidadRepuesto entity)
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
