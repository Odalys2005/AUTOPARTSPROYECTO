using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class ModeloRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public ModeloRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Modelo> SelectAll()
        {
            List<Modelo> result = new List<Modelo>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spModeloSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            Modelo entity = new Modelo();

                            entity.ModeloId = dr.GetInt32(0);
                            entity.MarcaId = dr.GetInt32(1);
                            entity.Nombre = dr.GetString(2);
                            entity.EstadoId = dr.GetInt32(3);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(Modelo entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spModeloInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@MarcaId", SqlDbType.Int)
                        .Value = entity.MarcaId;

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