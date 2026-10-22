using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class EstadoBL
    {
        private readonly EstadoRepository _repository;

        public EstadoBL(EstadoRepository repository)
        {
            _repository = repository;
        }

        public List<Estado> SelectAll()
        {
            List<Estado> result = null;

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
    }
}