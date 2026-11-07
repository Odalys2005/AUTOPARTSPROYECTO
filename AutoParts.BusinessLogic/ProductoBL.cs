using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class ProductoBL
    {
        private readonly ProductoRepository _repository;

        public ProductoBL(ProductoRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<Producto> SelectAll()
        {
            return _repository.SelectAll();
        }

        public int Insert(Producto entity)
        {
            ValidarProducto(entity);

            entity.EstadoId = 1;

            return _repository.Insert(entity);
        }

        public int Update(Producto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.ProductoId <= 0)
                throw new ArgumentException("Selecciona un producto válido.");

            ValidarProducto(entity);

            return _repository.Update(entity);
        }

        public int Delete(Producto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.ProductoId <= 0)
                throw new ArgumentException("Selecciona un producto válido.");

            return _repository.Delete(entity);
        }

        private static void ValidarProducto(Producto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.SubcategoriaId <= 0)
                throw new ArgumentException("Selecciona una subcategoría válida.");

            if (string.IsNullOrWhiteSpace(entity.CodigoOEM))
                throw new ArgumentException("El código OEM es obligatorio.");

            entity.CodigoOEM = entity.CodigoOEM.Trim();

            if (entity.CodigoOEM.Length > 50)
                throw new ArgumentException("El código OEM no puede superar los 50 caracteres.");

            if (string.IsNullOrWhiteSpace(entity.Nombre))
                throw new ArgumentException("El nombre del producto es obligatorio.");

            entity.Nombre = entity.Nombre.Trim();

            if (entity.Nombre.Length > 100)
                throw new ArgumentException("El nombre no puede superar los 100 caracteres.");

            if (string.IsNullOrWhiteSpace(entity.Descripcion))
                throw new ArgumentException("La descripción del producto es obligatoria.");

            entity.Descripcion = entity.Descripcion.Trim();

            if (entity.Descripcion.Length > 500)
                throw new ArgumentException("La descripción no puede superar los 500 caracteres.");

            if (entity.PrecioVenta <= 0)
                throw new ArgumentException("El precio de venta debe ser mayor que cero.");

            if (decimal.Round(entity.PrecioVenta, 2) != entity.PrecioVenta)
                throw new ArgumentException("El precio de venta admite hasta dos decimales.");

            if (entity.DiasGarantia < 0 || entity.DiasGarantia > 36500)
                throw new ArgumentException("Los días de garantía deben estar entre 0 y 36500.");

            if (entity.Imagen != null && entity.Imagen.Length > 500)
                throw new ArgumentException("La ruta de la imagen no puede superar los 500 caracteres.");
        }
    }
}