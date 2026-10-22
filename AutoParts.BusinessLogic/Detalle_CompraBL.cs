using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class Detalle_CompraBL
    {
        private readonly Detalle_CompraRepository _repository;

        public Detalle_CompraBL(Detalle_CompraRepository repository)
        {
            _repository = repository;
        }

        public List<Detalle_Compra> SelectAll()
        {
            List<Detalle_Compra> result = null;

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

        public int Insert(Detalle_Compra entity)
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