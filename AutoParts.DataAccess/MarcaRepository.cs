
using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class MarcaRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public MarcaRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Marca> SelectAll()
        {
            List<Marca> result = new List<Marca>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spMarcaSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            Marca entity = new Marca();

                            entity.MarcaId = dr.GetInt32(0);
                            entity.Nombre = dr.GetString(1);
                            entity.EstadoId = dr.GetInt32(2);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(Marca entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spMarcaInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100)
                        .Value = entity.Nombre;

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
