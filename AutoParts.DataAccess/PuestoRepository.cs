
using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class PuestoRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public PuestoRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Puesto> SelectAll()
        {
            List<Puesto> result = new List<Puesto>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spPuestoSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            Puesto entity = new Puesto();

                            entity.PuestoId = dr.GetInt32(0);
                            entity.Nombre = dr.GetString(1);
                            entity.Descripcion = dr.IsDBNull(2)
                                ? null
                                : dr.GetString(2);
                            entity.EstadoId = dr.GetInt32(3);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(Puesto entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spPuestoInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(
                        "@Nombre",
                        entity.Nombre
                    );

                    cmd.Parameters.AddWithValue(
                        "@Descripcion",
                        (object?)entity.Descripcion ?? DBNull.Value
                    );

                    cmd.Parameters.Add(
                        "@EstadoId",
                        SqlDbType.Int
                    ).Value = entity.EstadoId;

                    conn.Open();

                    result = cmd.ExecuteNonQuery();
                }
            }

            return result;
        }

        public int Update(Puesto entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spPuestoUpdate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(
                        "@Puesto",
                        entity.PuestoId
                    );

                    cmd.Parameters.AddWithValue(
                        "@Nombre",
                        entity.Nombre
                    );

                    cmd.Parameters.AddWithValue(
                        "@Descripcion",
                        (object?)entity.Descripcion ?? DBNull.Value
                    );

                    cmd.Parameters.Add(
                        "@EstadoId",
                        SqlDbType.Int
                    ).Value = entity.EstadoId;

                    conn.Open();

                    result = cmd.ExecuteNonQuery();
                }
            }

            return result;
        }

        public int Delete(Puesto entity)
        {
            using (SqlConnection conn =
                _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand("spPuestoDelete", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@PuestoId", SqlDbType.Int)
                        .Value = entity.PuestoId;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read())
                            throw new InvalidOperationException(
                                "SQL Server no confirmó la desactivación.");

                        int estadoId = dr.GetInt32(
                            dr.GetOrdinal("EstadoId"));

                        if (estadoId != 2)
                            throw new InvalidOperationException(
                                "El Puesto no quedó inactivo.");
                        return estadoId;
                    }
                }
            }
        }
    }
}
