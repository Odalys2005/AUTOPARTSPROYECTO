
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class InventarioBL
    {
        private readonly InventarioRepository _repository;

        public InventarioBL(InventarioRepository repository)
        {
            _repository = repository;
        }

        public List<Inventario> SelectAll()
        {
            List<Inventario> result = new List<Inventario>();

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

        public int Insert(Inventario entity)
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
