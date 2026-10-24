using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class CompatibilidadRepuestoRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public CompatibilidadRepuestoRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<CompatibilidadRepuesto> SelectAll()
        {
            List<CompatibilidadRepuesto> result = new List<CompatibilidadRepuesto>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spCompatibilidadRepuestoSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            CompatibilidadRepuesto entity = new CompatibilidadRepuesto();

                            entity.CompatibilidadId = dr.GetInt32(0);
                            entity.ProductoId = dr.GetInt32(1);
                            entity.VehiculoId = dr.IsDBNull(2)
                                ? null
                                : dr.GetInt32(2);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(CompatibilidadRepuesto entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spCompatibilidadRepuestoInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@ProductoId", SqlDbType.Int)
                        .Value = entity.ProductoId;

                    cmd.Parameters.Add("@VehiculoId", SqlDbType.Int)
                        .Value = (object?)entity.VehiculoId ?? DBNull.Value;

                    conn.Open();

                    result = cmd.ExecuteNonQuery();
                }
            }

            return result;
        }
    }
}