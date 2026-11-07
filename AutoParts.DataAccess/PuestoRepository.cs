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
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public List<Puesto> SelectAll()
        {
            List<Puesto> result = new List<Puesto>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spPuestoSelectAll", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    int colPuestoId = dr.GetOrdinal("PuestoId");
                    int colNombre = dr.GetOrdinal("Nombre");
                    int colDescripcion = dr.GetOrdinal("Descripcion");
                    int colEstadoId = dr.GetOrdinal("EstadoId");

                    while (dr.Read())
                    {
                        Puesto entity = new Puesto
                        {
                            PuestoId = dr.GetInt32(colPuestoId),
                            Nombre = dr.GetString(colNombre),
                            Descripcion = dr.IsDBNull(colDescripcion) ? null : dr.GetString(colDescripcion),
                            EstadoId = dr.GetInt32(colEstadoId)
                        };

                        result.Add(entity);
                    }
                }
            }

            return result;
        }

        public int Insert(Puesto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spPuestoInsert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = entity.Nombre;
                cmd.Parameters.Add("@Descripcion", SqlDbType.VarChar, 500).Value =
                    (object?)entity.Descripcion ?? DBNull.Value;
                cmd.Parameters.Add("@EstadoId", SqlDbType.Int).Value = entity.EstadoId;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Update(Puesto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (entity.PuestoId <= 0)
                throw new ArgumentException("Selecciona un puesto válido.");

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spPuestoUpdate", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@PuestoId", SqlDbType.Int).Value = entity.PuestoId;
                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = entity.Nombre;
                cmd.Parameters.Add("@Descripcion", SqlDbType.VarChar, 500).Value =
                    (object?)entity.Descripcion ?? DBNull.Value;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Delete(Puesto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (entity.PuestoId <= 0)
                throw new ArgumentException("Selecciona un puesto válido.");

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spPuestoDelete", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@PuestoId", SqlDbType.Int).Value = entity.PuestoId;

                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        throw new InvalidOperationException("SQL Server no confirmó el cambio de estado del puesto.");

                    int estadoId = dr.GetInt32(dr.GetOrdinal("EstadoId"));

                    if (estadoId != 1 && estadoId != 2)
                        throw new InvalidOperationException("SQL Server devolvió un estado no válido para el puesto.");

                    return estadoId;
                }
            }
        }
    }
}