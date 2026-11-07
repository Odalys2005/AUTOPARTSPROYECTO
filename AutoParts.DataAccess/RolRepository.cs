
using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class RolRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public RolRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        
        public List<Rol> SelectAll()
        {
            List<Rol> result = new List<Rol>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spRolSelectAll", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(
                    CommandBehavior.SingleResult))
                {
                    int colRolId = dr.GetOrdinal("RolId");
                    int colNombre = dr.GetOrdinal("Nombre");
                    int colEstadoId = dr.GetOrdinal("EstadoId");

                    while (dr.Read())
                    {
                        Rol entity = new Rol
                        {
                            RolId = dr.GetInt32(colRolId),
                            Nombre = dr.GetString(colNombre),
                            EstadoId = dr.GetInt32(colEstadoId)
                        };

                        result.Add(entity);
                    }
                }
            }

            return result;
        }

        
        public int Insert(Rol entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spRolInsert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100)
                    .Value = entity.Nombre;

                cmd.Parameters.Add("@EstadoId", SqlDbType.Int)
                    .Value = entity.EstadoId;

                conn.Open();

                cmd.ExecuteNonQuery();

                return 1;
            }
        }

        
        public int Update(Rol entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spRolUpdate", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@RolId", SqlDbType.Int)
                    .Value = entity.RolId;

                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100)
                    .Value = entity.Nombre;

                conn.Open();

                cmd.ExecuteNonQuery();

                return 1;
            }
        }



        public int Delete(Rol entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.RolId <= 0)
                throw new ArgumentException("Selecciona un rol válido.");

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spRolDelete", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@RolId", SqlDbType.Int)
                    .Value = entity.RolId;

                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(
                    CommandBehavior.SingleRow))
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
