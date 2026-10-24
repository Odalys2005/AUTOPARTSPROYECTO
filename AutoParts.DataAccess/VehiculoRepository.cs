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
            _connectionFactory = connectionFactory;
        }

        public List<Vehiculo> SelectAll()
        {
            List<Vehiculo> result = new List<Vehiculo>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spVehiculoSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            Vehiculo entity = new Vehiculo();

                            entity.VehiculoId = dr.GetInt32(0);
                            entity.ModeloId = dr.GetInt32(1);
                            entity.Anio = dr.GetInt16(2);
                            entity.Cilindrada = dr.GetDecimal(3);
                            entity.TipoMotor = dr.GetString(4);
                            entity.EstadoId = dr.GetInt32(5);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(Vehiculo entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spVehiculoInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@ModeloId", SqlDbType.Int)
                        .Value = entity.ModeloId;

                    cmd.Parameters.Add("@Anio", SqlDbType.SmallInt)
                        .Value = entity.Anio;

                    SqlParameter cilindrada = cmd.Parameters.Add(
                        "@Cilindrada", SqlDbType.Decimal);

                    cilindrada.Precision = 5;
                    cilindrada.Scale = 2;
                    cilindrada.Value = entity.Cilindrada;

                    cmd.Parameters.Add("@TipoMotor", SqlDbType.VarChar, 100)
                        .Value = entity.TipoMotor;

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