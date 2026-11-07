
using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class CategoriaRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public CategoriaRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Categoria> SelectAll()
        {
            List<Categoria> result = new List<Categoria>();

            using (SqlConnection conn =
                _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand("spCategoriaSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr =
                        cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        int id = dr.GetOrdinal("CategoriaId");
                        int nombre = dr.GetOrdinal("Nombre");
                        int estado = dr.GetOrdinal("EstadoId");

                        while (dr.Read())
                        {
                            Categoria entity = new Categoria
                            {
                                CategoriaId = dr.GetInt32(id),
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

        public int Insert(Categoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn =
                _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand("spCategoriaInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100)
                        .Value = entity.Nombre;

                    cmd.Parameters.Add("@EstadoId", SqlDbType.Int)
                        .Value = 1;

                    conn.Open();

                    cmd.ExecuteNonQuery();

                    return 1;
                }
            }
        }

        public int Update(Categoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn =
                _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand("spCategoriaUpdate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CategoriaId", SqlDbType.Int)
                        .Value = entity.CategoriaId;

                    cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100)
                        .Value = entity.Nombre;

                    conn.Open();

                    cmd.ExecuteNonQuery();

                    return 1;
                }
            }
        }

        public int Delete(Categoria entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.CategoriaId <= 0)
                throw new ArgumentException(
                    "Selecciona una categoría válida.");

            using (SqlConnection conn =
                _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand("spCategoriaDelete", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@CategoriaId", SqlDbType.Int)
                        .Value = entity.CategoriaId;

                    conn.Open();

                    using (SqlDataReader dr =
                        cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!dr.Read())
                            throw new InvalidOperationException(
                                "SQL Server no confirmó el cambio de estado.");

                        int estadoId = dr.GetInt32(
                            dr.GetOrdinal("EstadoId"));

                        if (estadoId != 1 && estadoId != 2)
                            throw new InvalidOperationException(
                                "SQL Server devolvió un estado no válido.");

                        return estadoId;
                    }
                }
            }
        }
    }
}
