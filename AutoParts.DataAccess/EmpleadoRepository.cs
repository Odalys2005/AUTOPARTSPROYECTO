using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class EmpleadoRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public EmpleadoRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public List<Empleado> SelectAll()
        {
            List<Empleado> result = new List<Empleado>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spEmpleadoSelectAll", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    int colEmpleadoId = dr.GetOrdinal("EmpleadoId");
                    int colPuestoId = dr.GetOrdinal("PuestoId");
                    int colNombre = dr.GetOrdinal("Nombre");
                    int colApellido = dr.GetOrdinal("Apellido");
                    int colDUI = dr.GetOrdinal("DUI");
                    int colTelefono = dr.GetOrdinal("Telefono");
                    int colCorreo = dr.GetOrdinal("Correo");
                    int colEstadoId = dr.GetOrdinal("EstadoId");

                    while (dr.Read())
                    {
                        Empleado entity = new Empleado
                        {
                            EmpleadoId = dr.GetInt32(colEmpleadoId),
                            PuestoId = dr.GetInt32(colPuestoId),
                            Nombre = dr.GetString(colNombre),
                            Apellido = dr.GetString(colApellido),
                            DUI = dr.GetString(colDUI),
                            Telefono = dr.GetString(colTelefono),
                            Correo = dr.GetString(colCorreo),
                            EstadoId = dr.GetInt32(colEstadoId)
                        };

                        result.Add(entity);
                    }
                }
            }

            return result;
        }

        public int Insert(Empleado entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spEmpleadoInsert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@PuestoId", SqlDbType.Int).Value = entity.PuestoId;
                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = entity.Nombre;
                cmd.Parameters.Add("@Apellido", SqlDbType.VarChar, 100).Value = entity.Apellido;
                cmd.Parameters.Add("@DUI", SqlDbType.VarChar, 10).Value = entity.DUI;
                cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 25).Value = entity.Telefono;
                cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 150).Value = entity.Correo;
                cmd.Parameters.Add("@EstadoId", SqlDbType.Int).Value = 1;

                conn.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0 ? result : 1;
            }
        }

        public int Update(Empleado entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spEmpleadoUpdate", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@EmpleadoId", SqlDbType.Int).Value = entity.EmpleadoId;
                cmd.Parameters.Add("@PuestoId", SqlDbType.Int).Value = entity.PuestoId;
                cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = entity.Nombre;
                cmd.Parameters.Add("@Apellido", SqlDbType.VarChar, 100).Value = entity.Apellido;
                cmd.Parameters.Add("@DUI", SqlDbType.VarChar, 10).Value = entity.DUI;
                cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 25).Value = entity.Telefono;
                cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 150).Value = entity.Correo;

                conn.Open();
                int result = cmd.ExecuteNonQuery();
                return result > 0 ? result : 1;
            }
        }

        public int Delete(Empleado entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.EmpleadoId <= 0)
                throw new ArgumentException("Selecciona un empleado válido.", nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("spEmpleadoDelete", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmpleadoId", SqlDbType.Int).Value = entity.EmpleadoId;

                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    if (!dr.Read())
                        throw new InvalidOperationException("SQL Server no confirmó el cambio de estado del empleado.");

                    int estadoId = dr.GetInt32(dr.GetOrdinal("EstadoId"));
                    if (estadoId != 1 && estadoId != 2)
                        throw new InvalidOperationException("SQL Server devolvió un estado no válido para el empleado.");

                    return estadoId;
                }
            }
        }
    }
}