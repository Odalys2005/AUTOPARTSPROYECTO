using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class CategoriaBL
    {
        private readonly CategoriaRepository _repository;

        public CategoriaBL(CategoriaRepository repository)
        {
            _repository = repository;
        }

        // ==========================================
        // SELECT ALL - CONSULTAR CATEGORIAS
        // ==========================================
        public List<Categoria> SelectAll()
        {
            try
            {
                return _repository.SelectAll();
            }
            catch
            {
                throw;
            }
        }

        // ==========================================
        // INSERT - REGISTRAR CATEGORIA
        // ==========================================
        public int Insert(Categoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.Nombre))
                throw new ArgumentException(
                    "El nombre de la categoría es obligatorio.");

            entity.Nombre = entity.Nombre.Trim();

            if (entity.Nombre.Length > 100)
                throw new ArgumentException(
                    "El nombre no puede superar los 100 caracteres.");

            if (entity.EstadoId != 1 && entity.EstadoId != 2)
                throw new ArgumentException(
                    "Selecciona un estado válido.");

            return _repository.Insert(entity);
        }

        // ==========================================
        // UPDATE - ACTUALIZAR CATEGORIA
        // ==========================================
        public int Update(Categoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.CategoriaId <= 0)
                throw new ArgumentException(
                    "Selecciona una categoría válida.");

            if (string.IsNullOrWhiteSpace(entity.Nombre))
                throw new ArgumentException(
                    "El nombre de la categoría es obligatorio.");

            entity.Nombre = entity.Nombre.Trim();

            if (entity.Nombre.Length > 100)
                throw new ArgumentException(
                    "El nombre no puede superar los 100 caracteres.");

            if (entity.EstadoId != 1 && entity.EstadoId != 2)
                throw new ArgumentException(
                    "Selecciona un estado válido.");

            return _repository.Update(entity);
        }

        // ==========================================
        // DELETE - DESACTIVACION LOGICA
        // ==========================================
        // No elimina físicamente la categoría.
        // Cambia su EstadoId a 2 (Inactivo).
        public int Delete(Categoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.CategoriaId <= 0)
                throw new ArgumentException(
                    "Selecciona una categoría válida.");

            return _repository.Delete(entity);
        }

        /*
        public int Update(Categoria entity)
        {
            int result;

            try
            {
                result = _repository.Update(entity);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }

        public int Delete(Categoria entity)
        {
            int result;

            try
            {
                result = _repository.Delete(entity);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }
        */
    }
}