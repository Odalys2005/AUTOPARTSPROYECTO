
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class CompraBL
    {
        private readonly CompraRepository _repository;

        public CompraBL(CompraRepository repository)
        {
            _repository = repository;
        }

        public List<Compra> SelectAll()
        {
            List<Compra> result = new List<Compra>();

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

        public int Insert(Compra entity)
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
