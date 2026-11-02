
using AutoParts.DataAccess;
using AutoParts.Entity;
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
            return _repository.SelectAll();
        }

        public int Insert(Puesto entity)
        {
            return _repository.Insert(entity);
        }

        public int Update(Puesto entity)
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

        public int Delete(Puesto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.PuestoId <= 0)
                throw new ArgumentException(
                    "Selecciona un Puesto válido.");

            return _repository.Delete(entity);
        }
    }
}
