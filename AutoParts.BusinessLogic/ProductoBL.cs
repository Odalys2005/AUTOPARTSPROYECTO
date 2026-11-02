
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class ProductoBL
    {
        private readonly ProductoRepository _repository;

        public ProductoBL(ProductoRepository repository)
        {
            _repository = repository;
        }

        public List<Producto> SelectAll()
        {
            List<Producto> result = new List<Producto>();

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

        public int Insert(Producto entity)
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

        public int Update(Producto entity)
        {
            int result = 0;

            try
            {
                result = _repository.Update(entity);
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        public int Delete(Producto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.ProductoId <= 0)
                throw new ArgumentException(
                    "Selecciona un Producto válido.");

            return _repository.Delete(entity);
        }
    }
}
