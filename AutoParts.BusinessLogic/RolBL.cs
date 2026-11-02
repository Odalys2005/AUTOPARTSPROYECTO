
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class RolBL
    {
        private readonly RolRepository _repository;

        public RolBL(RolRepository repository)
        {
            _repository = repository;
        }

        public List<Rol> SelectAll()
        {
            List<Rol> result = null;

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

        public int Insert(Rol entity)
        {
            int result;

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

        public int Update(Rol entity)
        {
            int result;

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

        public int Delete(Rol entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.RolId <= 0)
                throw new ArgumentException(
                    "Selecciona un Rol válido.");

            return _repository.Delete(entity);
        }
    }
}
