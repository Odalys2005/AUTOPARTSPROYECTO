using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

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
    }
}