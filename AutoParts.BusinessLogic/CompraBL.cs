using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoParts.BusinessLogic
{
    public class CompraBL
    {
        private readonly CompraRepository _repository;

        public CompraBL(CompraRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<Compra> SelectAll()
        {
            return _repository.SelectAll();
        }

        // Se conserva por compatibilidad con el proyecto existente.
        // NO usar Insert para compras que deban aumentar inventario:
        // usar RegistrarCompleta, que trabaja en una unica transaccion SQL.
        public int Insert(Compra entity)
        {
            return _repository.Insert(entity);
        }

        // La compra, todos sus detalles y el incremento de existencias
        // se confirman o se deshacen juntos en spCompraRegistrarCompleta.
        public CompraRegistroResultado RegistrarCompleta(
            Compra compra,
            IReadOnlyCollection<Detalle_Compra> detalles)
        {
            if (compra == null)
                throw new ArgumentNullException(nameof(compra));
            if (detalles == null)
                throw new ArgumentNullException(nameof(detalles));

            if (compra.ProveedorId <= 0)
                throw new ArgumentException("Selecciona un proveedor valido.");
            if (compra.EmpleadoId <= 0)
                throw new ArgumentException("Selecciona un empleado valido.");

            compra.NumeroComprobante = (compra.NumeroComprobante ?? "").Trim();
            if (compra.NumeroComprobante.Length == 0 || compra.NumeroComprobante.Length > 50)
                throw new ArgumentException("El numero de comprobante debe tener entre 1 y 50 caracteres.");

            if (compra.FechaCompra < new DateTime(1753, 1, 1) ||
                compra.FechaCompra > new DateTime(9999, 12, 31))
                throw new ArgumentException("Ingresa una fecha de compra valida.");

            if (detalles.Count == 0)
                throw new ArgumentException("Agrega al menos un producto a la compra.");

            var productosVistos = new HashSet<int>();
            decimal totalCalculado = 0m;
            foreach (Detalle_Compra detalle in detalles)
            {
                if (detalle == null)
                    throw new ArgumentException("Hay un detalle de compra vacio.");
                if (detalle.ProductoId <= 0)
                    throw new ArgumentException("Selecciona un producto valido en cada detalle.");
                if (detalle.Cantidad <= 0)
                    throw new ArgumentException("La cantidad de cada producto debe ser mayor que cero.");
                if (detalle.PrecioCosto < 0 || detalle.PrecioCosto > 9999999999999999.99m ||
                    detalle.PrecioCosto != decimal.Round(detalle.PrecioCosto, 2))
                    throw new ArgumentException("El costo debe ser positivo o cero, con hasta dos decimales.");
                if (!productosVistos.Add(detalle.ProductoId))
                    throw new ArgumentException("No repitas un producto: unifica sus cantidades.");

                try
                {
                    checked
                    {
                        decimal subtotal = detalle.Cantidad * detalle.PrecioCosto;
                        totalCalculado += subtotal;
                        if (subtotal > 9999999999999999.99m || totalCalculado > 9999999999999999.99m)
                            throw new ArgumentException("El importe de la compra supera el limite admitido.");
                    }
                }
                catch (OverflowException)
                {
                    throw new ArgumentException("El importe de la compra es demasiado grande.");
                }
            }

            // No utilizar Compra.TotalCompra ni Detalle_Compra.Subtotal enviados
            // desde la interfaz: SQL Server vuelve a calcular los importes.
            // StockMaximo es orientativo: la base permite superarlo y devuelve aviso.
            return _repository.RegistrarCompleta(compra, detalles);
        }
    }
}