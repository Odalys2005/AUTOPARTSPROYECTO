
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class ClienteBL
    {
        private readonly ClienteRepository _repository;

        public ClienteBL(ClienteRepository repository)
        {
            _repository = repository;
        }

        public List<Cliente> SelectAll()
        {
            try
            {
                return _repository.SelectAll();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int Insert(Cliente entity)
        {
            try
            {
                return _repository.Insert(entity);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
