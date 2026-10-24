using Microsoft.Data.SqlClient;
using AutoParts.Entity.Productos;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class SubcategoriaRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public SubcategoriaRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Subcategoria> SelectAll()
        {
            List<Subcategoria> result = new List<Subcategoria>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spSubcategoriaSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            Subcategoria entity = new Subcategoria();

                            entity.SubcategoriaId = dr.GetInt32(0);
                            entity.CategoriaId = dr.GetInt32(1);
                            entity.Nombre = dr.GetString(2);
                            entity.EstadoId = dr.GetInt32(3);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(Subcategoria entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spSubcategoriaInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CategoriaId", SqlDbType.Int)
                        .Value = entity.CategoriaId;

                    cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 120)
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