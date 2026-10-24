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
            _connectionFactory = connectionFactory;
        }

        public List<Empleado> SelectAll()
        {
            List<Empleado> result = new List<Empleado>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spEmpleadoSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            Empleado entity = new Empleado();

                            entity.EmpleadoId = dr.GetInt32(0);
                            entity.PuestoId = dr.GetInt32(1);
                            entity.Nombre = dr.GetString(2);
                            entity.Apellido = dr.GetString(3);
                            entity.DUI = dr.GetString(4);
                            entity.Telefono = dr.GetString(5);
                            entity.Correo = dr.GetString(6);
                            entity.EstadoId = dr.GetInt32(7);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(Empleado entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spEmpleadoInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@PuestoId", SqlDbType.Int)
                        .Value = entity.PuestoId;

                    cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100)
                        .Value = entity.Nombre;

                    cmd.Parameters.Add("@Apellido", SqlDbType.VarChar, 100)
                        .Value = entity.Apellido;

                    cmd.Parameters.Add("@DUI", SqlDbType.VarChar, 10)
                        .Value = entity.DUI;

                    cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 25)
                        .Value = entity.Telefono;

                    cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 150)
                        .Value = entity.Correo;

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