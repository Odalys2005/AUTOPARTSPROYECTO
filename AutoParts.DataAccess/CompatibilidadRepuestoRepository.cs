using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class CompatibilidadRepuestoRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public CompatibilidadRepuestoRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<CompatibilidadRepuesto> SelectAll()
        {
            List<CompatibilidadRepuesto> result = new List<CompatibilidadRepuesto>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spCompatibilidadRepuestoSelectAll", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    int idOrdinal = dr.GetOrdinal("CompatibilidadId");
                    int productoOrdinal = dr.GetOrdinal("ProductoId");
                    int vehiculoOrdinal = dr.GetOrdinal("VehiculoId");
                    int estadoOrdinal = dr.GetOrdinal("EstadoId");

                    while (dr.Read())
                    {
                        CompatibilidadRepuesto entity = new CompatibilidadRepuesto
                        {
                            CompatibilidadId = dr.GetInt32(idOrdinal),
                            ProductoId = dr.GetInt32(productoOrdinal),
                            VehiculoId = dr.IsDBNull(vehiculoOrdinal)
                                ? null
                                : dr.GetInt32(vehiculoOrdinal),
                            EstadoId = dr.GetInt32(estadoOrdinal)
                        };

                        result.Add(entity);
                    }
                }
            }

            return result;
        }

        public int Insert(CompatibilidadRepuesto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spCompatibilidadRepuestoInsert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ProductoId", SqlDbType.Int).Value = entity.ProductoId;
                cmd.Parameters.Add("@VehiculoId", SqlDbType.Int).Value =
                    (object?)entity.VehiculoId ?? DBNull.Value;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Update(CompatibilidadRepuesto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spCompatibilidadRepuestoUpdate", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CompatibilidadId", SqlDbType.Int).Value = entity.CompatibilidadId;
                cmd.Parameters.Add("@ProductoId", SqlDbType.Int).Value = entity.ProductoId;
                cmd.Parameters.Add("@VehiculoId", SqlDbType.Int).Value =
                    (object?)entity.VehiculoId ?? DBNull.Value;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Delete(CompatibilidadRepuesto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spCompatibilidadRepuestoDelete", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@CompatibilidadId", SqlDbType.Int).Value = entity.CompatibilidadId;

                conn.Open();
                object? resultado = cmd.ExecuteScalar();

                if (resultado == null || resultado == DBNull.Value)
                    throw new InvalidOperationException("SQL Server no confirmó el cambio de estado.");

                int estadoId = Convert.ToInt32(resultado);

                if (estadoId != 1 && estadoId != 2)
                    throw new InvalidOperationException("SQL Server devolvió un estado no válido.");

                return estadoId;
            }
        }
    }
}