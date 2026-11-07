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
            _connectionFactory = connectionFactory
                ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public List<Marca> SelectAll()
        {
            List<Marca> result = new List<Marca>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spMarcaSelectAll", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    int marcaIdOrdinal = dr.GetOrdinal("MarcaId");
                    int nombreOrdinal = dr.GetOrdinal("Nombre");
                    int estadoIdOrdinal = dr.GetOrdinal("EstadoId");

                    while (dr.Read())
                    {
                        Marca entity = new Marca
                        {
                            MarcaId = dr.GetInt32(marcaIdOrdinal),
                            Nombre = dr.GetString(nombreOrdinal),
                            EstadoId = dr.GetInt32(estadoIdOrdinal)
                        };

                        result.Add(entity);
                    }
                }
            }

            return result;
        }

        public int Insert(Marca entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spMarcaInsert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100)
                    .Value = entity.Nombre;

                cmd.Parameters.Add("@EstadoId", SqlDbType.Int)
                    .Value = 1;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Update(Marca entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.MarcaId <= 0)
                throw new ArgumentException("Selecciona una marca válida.", nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spMarcaUpdate", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@MarcaId", SqlDbType.Int)
                    .Value = entity.MarcaId;

                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100)
                    .Value = entity.Nombre;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Delete(Marca entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.MarcaId <= 0)
                throw new ArgumentException("Selecciona una marca válida.", nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spMarcaDelete", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@MarcaId", SqlDbType.Int)
                    .Value = entity.MarcaId;

                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        throw new InvalidOperationException(
                            "SQL Server no confirmó el cambio de estado de la marca.");

                    int estadoId = dr.GetInt32(dr.GetOrdinal("EstadoId"));

                    if (estadoId != 1 && estadoId != 2)
                        throw new InvalidOperationException(
                            "SQL Server devolvió un estado no válido para la marca.");

                    return estadoId;
                }
            }
        }
    }
}