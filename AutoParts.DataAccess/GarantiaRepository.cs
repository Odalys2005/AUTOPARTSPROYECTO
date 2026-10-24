
using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class GarantiaRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public GarantiaRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Garantia> SelectAll()
        {
            List<Garantia> result = new List<Garantia>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spGarantiaSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            Garantia entity = new Garantia();

                            entity.GarantiaId = dr.GetInt32(0);
                            entity.DetalleVentaId = dr.GetInt32(1);
                            entity.EmpleadoId = dr.GetInt32(2);
                            entity.FechaSolicitud = dr.GetDateTime(3);
                            entity.CantidadReclamada = dr.GetInt32(4);
                            entity.DescripcionFalla = dr.GetString(5);

                            entity.ResultadoEvaluacion = dr.IsDBNull(6)
                                ? null
                                : dr.GetString(6);

                            entity.FechaResolucion = dr.IsDBNull(7)
                                ? null
                                : dr.GetDateTime(7);

                            entity.Observaciones = dr.IsDBNull(8)
                                ? null
                                : dr.GetString(8);

                            entity.EstadoId = dr.GetInt32(9);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(Garantia entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spGarantiaInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@DetalleVentaId", SqlDbType.Int)
                        .Value = entity.DetalleVentaId;

                    cmd.Parameters.Add("@EmpleadoId", SqlDbType.Int)
                        .Value = entity.EmpleadoId;

                    cmd.Parameters.Add("@FechaSolicitud", SqlDbType.DateTime)
                        .Value = entity.FechaSolicitud;

                    cmd.Parameters.Add("@CantidadReclamada", SqlDbType.Int)
                        .Value = entity.CantidadReclamada;

                    cmd.Parameters.Add("@DescripcionFalla", SqlDbType.VarChar, 1000)
                        .Value = entity.DescripcionFalla;

                    cmd.Parameters.Add("@ResultadoEvaluacion", SqlDbType.VarChar, 500)
                        .Value = (object?)entity.ResultadoEvaluacion ?? DBNull.Value;

                    cmd.Parameters.Add("@FechaResolucion", SqlDbType.DateTime)
                        .Value = (object?)entity.FechaResolucion ?? DBNull.Value;

                    cmd.Parameters.Add("@Observaciones", SqlDbType.VarChar, 1000)
                        .Value = (object?)entity.Observaciones ?? DBNull.Value;

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
