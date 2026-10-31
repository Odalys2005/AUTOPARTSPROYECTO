using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;

namespace AutoParts.DataAccess
{
    public class CompraRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public CompraRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Compra> SelectAll()
        {
            List<Compra> result = new List<Compra>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spCompraSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            Compra entity = new Compra();

                            entity.CompraId = dr.GetInt32(0);
                            entity.ProveedorId = dr.GetInt32(1);
                            entity.EmpleadoId = dr.GetInt32(2);
                            entity.NumeroComprobante = dr.GetString(3);
                            entity.FechaCompra = dr.GetDateTime(4);
                            entity.TotalCompra = dr.GetDecimal(5);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        // Se conserva para compatibilidad con otras pantallas existentes.
        // NO utilizar este Insert individual para registrar compras con stock.
        public int Insert(Compra entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spCompraInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@ProveedorId", SqlDbType.Int)
                        .Value = entity.ProveedorId;

                    cmd.Parameters.Add("@EmpleadoId", SqlDbType.Int)
                        .Value = entity.EmpleadoId;

                    cmd.Parameters.Add("@NumeroComprobante", SqlDbType.VarChar, 50)
                        .Value = entity.NumeroComprobante;

                    cmd.Parameters.Add("@FechaCompra", SqlDbType.DateTime)
                        .Value = entity.FechaCompra;

                    SqlParameter totalCompra = cmd.Parameters.Add(
                        "@TotalCompra", SqlDbType.Decimal);

                    totalCompra.Precision = 18;
                    totalCompra.Scale = 2;
                    totalCompra.Value = entity.TotalCompra;

                    conn.Open();

                    result = cmd.ExecuteNonQuery();
                }
            }

            return result;
        }

        // Registra cabecera, detalles y aumentos de inventario mediante
        // UNA transaccion controlada en dbo.spCompraRegistrarCompleta.
        // No llamar tambien a Insert(Compra) ni a Detalle_CompraRepository.Insert().
        public CompraRegistroResultado RegistrarCompleta(
            Compra compra,
            IReadOnlyCollection<Detalle_Compra> detalles)
        {
            if (compra == null)
                throw new ArgumentNullException(nameof(compra));
            if (detalles == null)
                throw new ArgumentNullException(nameof(detalles));
            if (compra.ProveedorId <= 0 || compra.EmpleadoId <= 0)
                throw new ArgumentException("Selecciona un proveedor y un empleado válidos.");

            string comprobante = (compra.NumeroComprobante ?? "").Trim();
            if (comprobante.Length == 0 || comprobante.Length > 50)
                throw new ArgumentException("Ingresa un número de comprobante de hasta 50 caracteres.");
            if (compra.FechaCompra == default ||
                compra.FechaCompra < new DateTime(1753, 1, 1) ||
                compra.FechaCompra > new DateTime(9999, 12, 31))
                throw new ArgumentException("Ingresa una fecha de compra válida para SQL Server.");
            if (detalles.Count == 0)
                throw new ArgumentException("Agrega al menos un producto a la compra.");
            if (detalles.Any(d => d == null || d.ProductoId <= 0 || d.Cantidad <= 0 ||
                                  d.PrecioCosto < 0 || decimal.Round(d.PrecioCosto, 2) != d.PrecioCosto))
                throw new ArgumentException("Revisa los productos, cantidades y precios de costo.");
            if (detalles.GroupBy(d => d.ProductoId).Any(g => g.Count() > 1))
                throw new ArgumentException("Un producto se repite: unifica sus cantidades.");

            // La base de datos recalcula los subtotales y el total; no se
            // confia en importes enviados por el HTML ni por el llamador.
            string detallesJson = JsonSerializer.Serialize(detalles.Select(d => new
            {
                d.ProductoId,
                d.Cantidad,
                d.PrecioCosto
            }).ToArray());

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spCompraRegistrarCompleta", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 60;
                cmd.Parameters.Add("@ProveedorId", SqlDbType.Int).Value = compra.ProveedorId;
                cmd.Parameters.Add("@EmpleadoId", SqlDbType.Int).Value = compra.EmpleadoId;
                cmd.Parameters.Add("@NumeroComprobante", SqlDbType.VarChar, 50).Value = comprobante;
                cmd.Parameters.Add("@FechaCompra", SqlDbType.DateTime).Value = compra.FechaCompra;
                cmd.Parameters.Add("@Detalles", SqlDbType.NVarChar, -1).Value = detallesJson;

                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        throw new InvalidOperationException(
                            "SQL Server no devolvió la confirmación de la compra. Verifica el estado antes de reintentar.");

                    return new CompraRegistroResultado
                    {
                        CompraId = dr.GetInt32(dr.GetOrdinal("CompraId")),
                        NumeroComprobante = dr.GetString(dr.GetOrdinal("NumeroComprobante")),
                        TotalCompra = dr.GetDecimal(dr.GetOrdinal("TotalCompra")),
                        AdvertenciasStockMaximo = dr.IsDBNull(dr.GetOrdinal("AdvertenciasStockMaximo"))
                            ? ""
                            : dr.GetString(dr.GetOrdinal("AdvertenciasStockMaximo"))
                    };
                }
            }
        }
    }

    // Resultado de la operacion completa, sin exponer el mecanismo SQL al Dashboard.
    public class CompraRegistroResultado
    {
        public int CompraId { get; set; }
        public string NumeroComprobante { get; set; } = "";
        public decimal TotalCompra { get; set; }
        public string AdvertenciasStockMaximo { get; set; } = "";
    }
}