
using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class Detalle_CompraRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public Detalle_CompraRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Detalle_Compra> SelectAll()
        {
            List<Detalle_Compra> result = new List<Detalle_Compra>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spDetalleCompraSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            Detalle_Compra entity = new Detalle_Compra();

                            entity.DetalleCompraId = dr.GetInt32(0);
                            entity.CompraId = dr.GetInt32(1);
                            entity.ProductoId = dr.GetInt32(2);
                            entity.Cantidad = dr.GetInt32(3);
                            entity.PrecioCosto = dr.GetDecimal(4);
                            entity.Subtotal = dr.GetDecimal(5);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(Detalle_Compra entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spDetalleCompraInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CompraId", SqlDbType.Int)
                        .Value = entity.CompraId;

                    cmd.Parameters.Add("@ProductoId", SqlDbType.Int)
                        .Value = entity.ProductoId;

                    cmd.Parameters.Add("@Cantidad", SqlDbType.Int)
                        .Value = entity.Cantidad;

                    SqlParameter precioCosto = cmd.Parameters.Add(
                        "@PrecioCosto", SqlDbType.Decimal);

                    precioCosto.Precision = 18;
                    precioCosto.Scale = 2;
                    precioCosto.Value = entity.PrecioCosto;

                    SqlParameter subtotal = cmd.Parameters.Add(
                        "@Subtotal", SqlDbType.Decimal);

                    subtotal.Precision = 18;
                    subtotal.Scale = 2;
                    subtotal.Value = entity.Subtotal;

                    conn.Open();

                    result = cmd.ExecuteNonQuery();
                }
            }

            return result;
        }
    }
}
