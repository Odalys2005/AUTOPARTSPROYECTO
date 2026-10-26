using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class VentaRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public VentaRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Venta> SelectAll()
        {
            List<Venta> result = new List<Venta>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spVentaSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            Venta entity = new Venta();

                            entity.VentaId = dr.GetInt32(0);
                            entity.ClienteId = dr.GetInt32(1);
                            entity.EmpleadoId = dr.GetInt32(2);
                            entity.NumeroFactura = dr.GetString(3);
                            entity.FechaVenta = dr.GetDateTime(4);
                            entity.Total = dr.GetDecimal(5);
                            entity.EstadoId = dr.GetInt32(6);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        // Registra cabecera + detalles + salida de inventario en UNA transacción SQL.
        public VentaRegistroResultado RegistrarCompleta(VentaRegistroCompleto solicitud)
        {
            if (solicitud == null) throw new ArgumentNullException(nameof(solicitud));

            DataTable lineas = new DataTable();
            lineas.Columns.Add("ProductoId", typeof(int));
            lineas.Columns.Add("Cantidad", typeof(int));
            foreach (VentaLineaRegistro linea in solicitud.Lineas)
                lineas.Rows.Add(linea.ProductoId, linea.Cantidad);

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spVentaRegistrarCompleta", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 45;
                cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = solicitud.ClienteId;
                cmd.Parameters.Add("@EmpleadoId", SqlDbType.Int).Value = solicitud.EmpleadoId;
                cmd.Parameters.Add("@NumeroFactura", SqlDbType.VarChar, 50).Value =
                    string.IsNullOrWhiteSpace(solicitud.NumeroFactura)
                        ? DBNull.Value
                        : solicitud.NumeroFactura.Trim();
                cmd.Parameters.Add("@EstadoId", SqlDbType.Int).Value = solicitud.EstadoId;

                SqlParameter carrito = cmd.Parameters.Add("@Lineas", SqlDbType.Structured);
                carrito.TypeName = "dbo.AutoPartsVentaLinea";
                carrito.Value = lineas;

                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    if (!dr.Read())
                        throw new InvalidOperationException("SQL Server no devolvió la confirmación de la venta.");

                    return new VentaRegistroResultado
                    {
                        VentaId = dr.GetInt32(dr.GetOrdinal("VentaId")),
                        Total = dr.GetDecimal(dr.GetOrdinal("Total")),
                        NumeroFactura = dr.GetString(dr.GetOrdinal("NumeroFactura"))
                    };
                }
            }
        }

        public int Insert(Venta entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spVentaInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@ClienteId", SqlDbType.Int)
                        .Value = entity.ClienteId;

                    cmd.Parameters.Add("@EmpleadoId", SqlDbType.Int)
                        .Value = entity.EmpleadoId;

                    cmd.Parameters.Add("@NumeroFactura", SqlDbType.VarChar, 50)
                        .Value = entity.NumeroFactura;

                    cmd.Parameters.Add("@FechaVenta", SqlDbType.DateTime)
                        .Value = entity.FechaVenta;

                    SqlParameter total = cmd.Parameters.Add(
                        "@Total", SqlDbType.Decimal);

                    total.Precision = 18;
                    total.Scale = 2;
                    total.Value = entity.Total;

                    cmd.Parameters.Add("@EstadoId", SqlDbType.Int)
                        .Value = entity.EstadoId;

                    conn.Open();

                    result = cmd.ExecuteNonQuery();
                }
            }

            return result;
        }
    }
}