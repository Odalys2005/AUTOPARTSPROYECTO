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
            _connectionFactory = connectionFactory
                ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public List<Modelo> SelectAll()
        {
            List<Modelo> result = new List<Modelo>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spModeloSelectAll", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    int columnaModeloId = dr.GetOrdinal("ModeloId");
                    int columnaMarcaId = dr.GetOrdinal("MarcaId");
                    int columnaNombre = dr.GetOrdinal("Nombre");
                    int columnaEstadoId = dr.GetOrdinal("EstadoId");

                    while (dr.Read())
                    {
                        Modelo entity = new Modelo
                        {
                            ModeloId = dr.GetInt32(columnaModeloId),
                            MarcaId = dr.GetInt32(columnaMarcaId),
                            Nombre = dr.GetString(columnaNombre),
                            EstadoId = dr.GetInt32(columnaEstadoId)
                        };

                        result.Add(entity);
                    }
                }
            }

            return result;
        }

        public int Insert(Modelo entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spModeloInsert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@MarcaId", SqlDbType.Int).Value = entity.MarcaId;
                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = entity.Nombre;

                // Un modelo nuevo siempre se registra activo.
                // El procedimiento conserva este parámetro por compatibilidad.
                cmd.Parameters.Add("@EstadoId", SqlDbType.Int).Value = 1;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Update(Modelo entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spModeloUpdate", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@ModeloId", SqlDbType.Int).Value = entity.ModeloId;
                cmd.Parameters.Add("@MarcaId", SqlDbType.Int).Value = entity.MarcaId;
                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = entity.Nombre;

                // No se envía @EstadoId: editar no cambia el estado.
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Delete(Modelo entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.ModeloId <= 0)
                throw new ArgumentException("Selecciona un modelo válido.", nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spModeloDelete", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ModeloId", SqlDbType.Int).Value = entity.ModeloId;

                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        throw new InvalidOperationException(
                            "SQL Server no confirmó el cambio de estado del modelo.");

                    int estadoId = dr.GetInt32(dr.GetOrdinal("EstadoId"));

                    if (estadoId != 1 && estadoId != 2)
                        throw new InvalidOperationException(
                            "SQL Server devolvió un estado no válido para el modelo.");

                    return estadoId;
                }
            }
        }
    }
}