using AutoParts.Entidades;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class EstadoRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public EstadoRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Estado> SelectAll()
        {
            List<Estado> result = new List<Estado>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spEstadoSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Estado entity = new Estado()
                            {
                                EstadoId = Convert.ToInt32(dr["EstadoId"]),
                                Nombre = dr["Nombre"].ToString()
                            };

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }
    }
}