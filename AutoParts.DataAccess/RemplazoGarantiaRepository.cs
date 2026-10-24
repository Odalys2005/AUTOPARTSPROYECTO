
using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class ReemplazoGarantiaRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public ReemplazoGarantiaRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<ReemplazoGarantia> SelectAll()
        {
            List<ReemplazoGarantia> result = new List<ReemplazoGarantia>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spReemplazoGarantiaSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            ReemplazoGarantia entity = new ReemplazoGarantia();

                            entity.ReemplazoId = dr.GetInt32(0);
                            entity.GarantiaId = dr.GetInt32(1);
                            entity.EmpleadoId = dr.GetInt32(2);
                            entity.FechaReemplazo = dr.GetDateTime(3);
                            entity.Cantidad = dr.GetInt32(4);

                            entity.Observaciones = dr.IsDBNull(5)
                                ? null
                                : dr.GetString(5);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(ReemplazoGarantia entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spReemplazoGarantiaInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@GarantiaId", SqlDbType.Int)
                        .Value = entity.GarantiaId;

                    cmd.Parameters.Add("@EmpleadoId", SqlDbType.Int)
                        .Value = entity.EmpleadoId;

                    cmd.Parameters.Add("@FechaReemplazo", SqlDbType.DateTime)
                        .Value = entity.FechaReemplazo;

                    cmd.Parameters.Add("@Cantidad", SqlDbType.Int)
                        .Value = entity.Cantidad;

                    cmd.Parameters.Add("@Observaciones", SqlDbType.VarChar, 1000)
                        .Value = (object?)entity.Observaciones ?? DBNull.Value;

                    conn.Open();

                    result = cmd.ExecuteNonQuery();
                }
            }

            return result;
        }
    }
}
