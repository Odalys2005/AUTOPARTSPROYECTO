
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class MarcaBL
    {
        private readonly MarcaRepository _repository;

        public MarcaBL(MarcaRepository repository)
        {
            _repository = repository;
        }

        public List<Marca> SelectAll()
        {
            return _repository.SelectAll();
        }

        public int Insert(Marca entity)
        {
            return _repository.Insert(entity);
        }

        public int Update(Marca entity)
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


        public int Delete(Marca entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.MarcaId <= 0)
                throw new ArgumentException(
                    "Selecciona una Marca válida.");

            return _repository.Delete(entity);
        }


    }
}
