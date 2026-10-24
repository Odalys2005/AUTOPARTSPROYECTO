
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
