using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class VentaBL
    {
        private readonly VentaRepository _repository;

        public VentaBL(VentaRepository repository)
        {
            _repository = repository;
        }

        public List<Venta> SelectAll()
        {
            List<Venta> result = new List<Venta>();

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

        public VentaRegistroResultado RegistrarCompleta(VentaRegistroCompleto solicitud)
        {
            if (solicitud == null)
                throw new ArgumentNullException(nameof(solicitud));
            if (solicitud.ClienteId <= 0 || solicitud.EmpleadoId <= 0 || solicitud.EstadoId <= 0)
                throw new ArgumentException("Selecciona un cliente, empleado y estado válidos.");
            // Número vacío: SQL Server genera FAC-AAAA-NNNNNN al confirmar.
            // Normalizar a cadena vacía mantiene compatible el repositorio actual.
            solicitud.NumeroFactura = (solicitud.NumeroFactura ?? string.Empty).Trim();
            if (solicitud.NumeroFactura.Length > 50)
                throw new ArgumentException("El número de factura no puede superar 50 caracteres.");
            if (solicitud.Lineas == null || solicitud.Lineas.Count == 0)
                throw new ArgumentException("Agrega por lo menos un producto al carrito.");
            if (solicitud.Lineas.Count > 200)
                throw new ArgumentException("La venta supera el máximo de 200 productos diferentes.");

            HashSet<int> ids = new HashSet<int>();
            foreach (VentaLineaRegistro linea in solicitud.Lineas)
            {
                if (linea == null || linea.ProductoId <= 0 || linea.Cantidad <= 0)
                    throw new ArgumentException("El carrito contiene un producto o cantidad inválida.");
                if (!ids.Add(linea.ProductoId))
                    throw new ArgumentException("El carrito contiene productos repetidos.");
            }
            return _repository.RegistrarCompleta(solicitud);
        }

        public int Insert(Venta entity)
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
    }
}