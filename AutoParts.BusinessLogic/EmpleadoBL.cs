using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class EmpleadoBL
    {
        private readonly EmpleadoRepository _repository;

        public EmpleadoBL(EmpleadoRepository repository)
        {
            _repository = repository;
        }

        public List<Empleado> SelectAll()
        {
            List<Empleado> result = null;

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

        public int Insert(Empleado entity)
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