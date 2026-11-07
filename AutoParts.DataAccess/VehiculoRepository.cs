using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class VehiculoRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public VehiculoRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public List<Vehiculo> SelectAll()
        {
            List<Vehiculo> result = new List<Vehiculo>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spVehiculoSelectAll", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    int columnaVehiculoId = dr.GetOrdinal("VehiculoId");
                    int columnaModeloId = dr.GetOrdinal("ModeloId");
                    int columnaAnio = dr.GetOrdinal("Anio");
                    int columnaCilindrada = dr.GetOrdinal("Cilindrada");
                    int columnaTipoMotor = dr.GetOrdinal("TipoMotor");
                    int columnaEstadoId = dr.GetOrdinal("EstadoId");

                    while (dr.Read())
                    {
                        Vehiculo entity = new Vehiculo
                        {
                            VehiculoId = dr.GetInt32(columnaVehiculoId),
                            ModeloId = dr.GetInt32(columnaModeloId),
                            Anio = dr.GetInt16(columnaAnio),
                            Cilindrada = dr.GetDecimal(columnaCilindrada),
                            TipoMotor = dr.GetString(columnaTipoMotor),
                            EstadoId = dr.GetInt32(columnaEstadoId)
                        };

                        result.Add(entity);
                    }
                }
            }

            return result;
        }

        public int Insert(Vehiculo entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spVehiculoInsert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@ModeloId", SqlDbType.Int).Value = entity.ModeloId;
                cmd.Parameters.Add("@Anio", SqlDbType.SmallInt).Value = entity.Anio;

                SqlParameter cilindrada = cmd.Parameters.Add("@Cilindrada", SqlDbType.Decimal);
                cilindrada.Precision = 5;
                cilindrada.Scale = 2;
                cilindrada.Value = entity.Cilindrada;

                cmd.Parameters.Add("@TipoMotor", SqlDbType.VarChar, 100).Value = entity.TipoMotor;
                cmd.Parameters.Add("@EstadoId", SqlDbType.Int).Value = 1;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Update(Vehiculo entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spVehiculoUpdate", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@VehiculoId", SqlDbType.Int).Value = entity.VehiculoId;
                cmd.Parameters.Add("@ModeloId", SqlDbType.Int).Value = entity.ModeloId;
                cmd.Parameters.Add("@Anio", SqlDbType.SmallInt).Value = entity.Anio;

                SqlParameter cilindrada = cmd.Parameters.Add("@Cilindrada", SqlDbType.Decimal);
                cilindrada.Precision = 5;
                cilindrada.Scale = 2;
                cilindrada.Value = entity.Cilindrada;

                cmd.Parameters.Add("@TipoMotor", SqlDbType.VarChar, 100).Value = entity.TipoMotor;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Delete(Vehiculo entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.VehiculoId <= 0)
                throw new ArgumentException("Selecciona un vehículo válido.", nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spVehiculoDelete", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@VehiculoId", SqlDbType.Int).Value = entity.VehiculoId;

                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        throw new InvalidOperationException("SQL Server no confirmó el cambio de estado del vehículo.");

                    int estadoId = dr.GetInt32(dr.GetOrdinal("EstadoId"));

                    if (estadoId != 1 && estadoId != 2)
                        throw new InvalidOperationException("SQL Server devolvió un estado no válido para el vehículo.");

                    return estadoId;
                }
            }
        }
    }
}