
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
                        int id = dr.GetOrdinal("SubcategoriaId");
                        int categoria = dr.GetOrdinal("CategoriaId");
                        int categoriaNombre = dr.GetOrdinal("CategoriaNombre");
                        int nombre = dr.GetOrdinal("Nombre");
                        int estado = dr.GetOrdinal("EstadoId");

                        while (dr.Read())
                        {
                            Subcategoria entity = new Subcategoria
                            {
                                SubcategoriaId = dr.GetInt32(id),
                                CategoriaId = dr.GetInt32(categoria),
                                CategoriaNombre = dr.GetString(categoriaNombre),
                                Nombre = dr.GetString(nombre),
                                EstadoId = dr.GetInt32(estado)
                            };

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(Subcategoria entity)
        {
            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spSubcategoriaInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CategoriaId", SqlDbType.Int)
                        .Value = entity.CategoriaId;

                    cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 120)
                        .Value = entity.Nombre;

                    conn.Open();

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public int Update(Subcategoria entity)
        {
            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spSubcategoriaUpdate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@SubcategoriaId", SqlDbType.Int)
                        .Value = entity.SubcategoriaId;

                    cmd.Parameters.Add("@CategoriaId", SqlDbType.Int)
                        .Value = entity.CategoriaId;

                    cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 120)
                        .Value = entity.Nombre;

                    conn.Open();

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public int Delete(Subcategoria entity)
        {
            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spSubcategoriaDelete", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@SubcategoriaId", SqlDbType.Int)
                        .Value = entity.SubcategoriaId;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        if (!dr.Read())
                        {
                            throw new InvalidOperationException(
                                "SQL Server no encontró la subcategoría o no confirmó el cambio de estado.");
                        }

                        int estadoId = dr.GetInt32(
                            dr.GetOrdinal("EstadoId"));

                        if (estadoId != 1 && estadoId != 2)
                        {
                            throw new InvalidOperationException(
                                "SQL Server devolvió un estado no válido.");
                        }

                        return estadoId;
                    }
                }
            }
        }
    }
}
