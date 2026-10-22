using AutoParts.DataAccess;
using AutoParts.Entidades;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class PuestoBL
    {
        private readonly PuestoRepository _repository;

        public PuestoBL(PuestoRepository repository)
        {
            _repository = repository;
        }

        public List<Puesto> SelectAll()
        {
            List<Puesto> result = null;

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

        public int Insert(Puesto entity)
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