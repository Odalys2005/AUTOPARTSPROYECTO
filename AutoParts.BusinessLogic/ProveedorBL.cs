
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class ProveedorBL
    {
        private readonly ProveedorRepository _repository;

        public ProveedorBL(ProveedorRepository repository)
        {
            _repository = repository;
        }

        public List<Proveedor> SelectAll()
        {
            List<Proveedor> result = new List<Proveedor>();

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

        public int Insert(Proveedor entity)
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
