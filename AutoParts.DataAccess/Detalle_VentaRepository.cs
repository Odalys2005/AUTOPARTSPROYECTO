
using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class Detalle_VentaRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public Detalle_VentaRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Detalle_Venta> SelectAll()
        {
            List<Detalle_Venta> result = new List<Detalle_Venta>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spDetalleVentaSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            Detalle_Venta entity = new Detalle_Venta();

                            entity.DetalleVentaId = dr.GetInt32(0);
                            entity.VentaId = dr.GetInt32(1);
                            entity.ProductoId = dr.GetInt32(2);
                            entity.Cantidad = dr.GetInt32(3);
                            entity.PrecioUnitario = dr.GetDecimal(4);

                            entity.Descuento = dr.IsDBNull(5)
                                ? null
                                : dr.GetDecimal(5);

                            entity.Subtotal = dr.GetDecimal(6);

                            entity.DiasGarantiaVenta = dr.IsDBNull(7)
                                ? null
                                : dr.GetInt32(7);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(Detalle_Venta entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spDetalleVentaInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@VentaId", SqlDbType.Int)
                        .Value = entity.VentaId;

                    cmd.Parameters.Add("@ProductoId", SqlDbType.Int)
                        .Value = entity.ProductoId;

                    cmd.Parameters.Add("@Cantidad", SqlDbType.Int)
                        .Value = entity.Cantidad;

                    SqlParameter precioUnitario = cmd.Parameters.Add(
                        "@PrecioUnitario", SqlDbType.Decimal);

                    precioUnitario.Precision = 18;
                    precioUnitario.Scale = 2;
                    precioUnitario.Value = entity.PrecioUnitario;

                    SqlParameter descuento = cmd.Parameters.Add(
                        "@Descuento", SqlDbType.Decimal);

                    descuento.Precision = 18;
                    descuento.Scale = 2;
                    descuento.Value = (object?)entity.Descuento ?? DBNull.Value;

                    SqlParameter subtotal = cmd.Parameters.Add(
                        "@Subtotal", SqlDbType.Decimal);

                    subtotal.Precision = 18;
                    subtotal.Scale = 2;
                    subtotal.Value = entity.Subtotal;

                    cmd.Parameters.Add("@DiasGarantiaVenta", SqlDbType.Int)
                        .Value = (object?)entity.DiasGarantiaVenta ?? DBNull.Value;

                    conn.Open();

                    result = cmd.ExecuteNonQuery();
                }
            }

            return result;
        }
    }
}
