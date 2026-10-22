
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
    }
}
