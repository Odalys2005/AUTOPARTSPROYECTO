
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
    }
}
