
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
            List<Empleado> result = new List<Empleado>();

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

        public int Insert(Empleado entity)
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

        public int Update(Empleado entity)
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

        public int Delete(Empleado entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.EmpleadoId <= 0)
                throw new ArgumentException(
                    "Selecciona un Empleado válido.");

            return _repository.Delete(entity);
        }
    }
}
