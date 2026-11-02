
using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class ProveedorRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public ProveedorRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Proveedor> SelectAll()
        {
            List<Proveedor> result = new List<Proveedor>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spProveedorSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            Proveedor entity = new Proveedor();

                            entity.ProveedorId = dr.GetInt32(0);
                            entity.NombreEmpresa = dr.GetString(1);
                            entity.NIT = dr.GetString(2);
                            entity.Telefono = dr.GetString(3);
                            entity.Correo = dr.GetString(4);
                            entity.Contacto = dr.GetString(5);
                            entity.EstadoId = dr.GetInt32(6);
                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(Proveedor entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spProveedorInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@NombreEmpresa", SqlDbType.VarChar, 100)
                        .Value = entity.NombreEmpresa;

                    cmd.Parameters.Add("@NIT", SqlDbType.VarChar, 20)
                        .Value = entity.NIT;

                    cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 25)
                        .Value = entity.Telefono;

                    cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 150)
                        .Value = entity.Correo;

                    cmd.Parameters.Add("@Contacto", SqlDbType.VarChar, 100)
                        .Value = entity.Contacto;

                    cmd.Parameters.Add("@EstadoId", SqlDbType.Int)
                        .Value = entity.EstadoId;


                    conn.Open();

                    result = cmd.ExecuteNonQuery();
                }
            }

            return result;
        }

        public int Update(Proveedor entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spProveedorUpdate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@ProveedorId", SqlDbType.Int)
                        .Value = entity.ProveedorId;

                    cmd.Parameters.Add("@NombreEmpresa", SqlDbType.VarChar, 100)
                        .Value = entity.NombreEmpresa;

                    cmd.Parameters.Add("@NIT", SqlDbType.VarChar, 20)
                        .Value = entity.NIT;

                    cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 25)
                        .Value = entity.Telefono;

                    cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 150)
                        .Value = entity.Correo;

                    cmd.Parameters.Add("@Contacto", SqlDbType.VarChar, 100)
                        .Value = entity.Contacto;

                    cmd.Parameters.Add("@EstadoId", SqlDbType.Int)
                        .Value = entity.EstadoId;


                    conn.Open();

                    result = cmd.ExecuteNonQuery();
                }
            }

            return result;
        }

        public int Delete(Proveedor entity)
        {
            using (SqlConnection conn =
                _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd =
                    new SqlCommand("spProveedorDelete", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@ProveedorId", SqlDbType.Int)
                        .Value = entity.ProveedorId;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read())
                            throw new InvalidOperationException(
                                "SQL Server no confirmó la desactivación.");

                        int estadoId = dr.GetInt32(
                            dr.GetOrdinal("EstadoId"));

                        if (estadoId != 2)
                            throw new InvalidOperationException(
                                "El Proveedor no quedó inactivo.");
                        return estadoId;
                    }
                }
            }
        }
    }
}
