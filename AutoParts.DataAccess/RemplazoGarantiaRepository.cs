
using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class ReemplazoGarantiaRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public ReemplazoGarantiaRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<ReemplazoGarantia> SelectAll()
        {
            List<ReemplazoGarantia> result = new List<ReemplazoGarantia>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spReemplazoGarantiaSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            ReemplazoGarantia entity = new ReemplazoGarantia();

                            entity.ReemplazoId = dr.GetInt32(0);
                            entity.GarantiaId = dr.GetInt32(1);
                            entity.EmpleadoId = dr.GetInt32(2);
                            entity.FechaReemplazo = dr.GetDateTime(3);
                            entity.Cantidad = dr.GetInt32(4);

                            entity.Observaciones = dr.IsDBNull(5)
                                ? null
                                : dr.GetString(5);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        // Registra la entrega real en una transacción SQL y descuenta stock.
        // Si no hay existencias, el procedimiento devuelve PENDIENTE_REEMPLAZO
        // sin generar un registro de entrega ni descontar inventario.
        public ReemplazoEntregaResultado EntregarReemplazo(
            int garantiaId, int empleadoId, string? observaciones = null)
        {
            if (garantiaId <= 0)
                throw new ArgumentException("Selecciona una garantía válida.", nameof(garantiaId));
            if (empleadoId <= 0)
                throw new ArgumentException("Selecciona un empleado válido.", nameof(empleadoId));
            if (observaciones != null && observaciones.Length > 1000)
                throw new ArgumentException("Las observaciones superan 1000 caracteres.", nameof(observaciones));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spGarantiaEntregarReemplazo", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 45;
                cmd.Parameters.Add("@GarantiaId", SqlDbType.Int).Value = garantiaId;
                cmd.Parameters.Add("@EmpleadoId", SqlDbType.Int).Value = empleadoId;
                cmd.Parameters.Add("@Observaciones", SqlDbType.VarChar, 1000).Value =
                    string.IsNullOrWhiteSpace(observaciones)
                        ? DBNull.Value
                        : observaciones.Trim();

                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        throw new InvalidOperationException(
                            "SQL Server no devolvió confirmación del reemplazo. Verifica el estado antes de reintentar.");

                    int colId = dr.GetOrdinal("ReemplazoId");
                    return new ReemplazoEntregaResultado
                    {
                        ReemplazoId = dr.IsDBNull(colId) ? null : dr.GetInt32(colId),
                        GarantiaId = dr.GetInt32(dr.GetOrdinal("GarantiaId")),
                        ProductoId = dr.GetInt32(dr.GetOrdinal("ProductoId")),
                        Cantidad = dr.GetInt32(dr.GetOrdinal("Cantidad")),
                        StockRestante = dr.GetInt32(dr.GetOrdinal("StockRestante")),
                        EstadoGarantia = dr.GetString(dr.GetOrdinal("EstadoGarantia")),
                        Mensaje = dr.GetString(dr.GetOrdinal("Mensaje"))
                    };
                }
            }
        }

        // LEGADO: este procedimiento NO descuenta inventario.
        // No usar este método en el flujo integrado de garantías.
        public int Insert(ReemplazoGarantia entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spReemplazoGarantiaInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@GarantiaId", SqlDbType.Int)
                        .Value = entity.GarantiaId;

                    cmd.Parameters.Add("@EmpleadoId", SqlDbType.Int)
                        .Value = entity.EmpleadoId;

                    cmd.Parameters.Add("@FechaReemplazo", SqlDbType.DateTime)
                        .Value = entity.FechaReemplazo;

                    cmd.Parameters.Add("@Cantidad", SqlDbType.Int)
                        .Value = entity.Cantidad;

                    cmd.Parameters.Add("@Observaciones", SqlDbType.VarChar, 1000)
                        .Value = (object?)entity.Observaciones ?? DBNull.Value;

                    conn.Open();

                    result = cmd.ExecuteNonQuery();
                }
            }

            return result;
        }
    }
}

namespace AutoParts.DataAccess
{
    public class ReemplazoEntregaResultado
    {
        // NULL cuando la garantía permanece pendiente de reemplazo.
        public int? ReemplazoId { get; set; }
        public int GarantiaId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public int StockRestante { get; set; }
        public string EstadoGarantia { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
    }
}
