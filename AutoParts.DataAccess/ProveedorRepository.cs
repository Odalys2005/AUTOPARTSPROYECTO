
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
                        int id = dr.GetOrdinal("ProveedorId");
                        int nombre = dr.GetOrdinal("NombreEmpresa");
                        int nit = dr.GetOrdinal("NIT");
                        int telefono = dr.GetOrdinal("Telefono");
                        int correo = dr.GetOrdinal("Correo");
                        int contacto = dr.GetOrdinal("Contacto");
                        int estado = dr.GetOrdinal("EstadoId");

                        while (dr.Read())
                        {
                            Proveedor entity = new Proveedor();

                            entity.ProveedorId = dr.GetInt32(id);
                            entity.NombreEmpresa = dr.GetString(nombre);
                            entity.NIT = dr.GetString(nit);
                            entity.Telefono = dr.GetString(telefono);
                            entity.Correo = dr.GetString(correo);
                            entity.Contacto = dr.GetString(contacto);
                            entity.EstadoId = dr.GetInt32(estado);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(Proveedor entity)
        {
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

                    conn.Open();

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public int Update(Proveedor entity)
        {
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

                    conn.Open();

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public int Delete(Proveedor entity)
        {
            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spProveedorDelete", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@ProveedorId", SqlDbType.Int)
                        .Value = entity.ProveedorId;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            throw new InvalidOperationException(
                                "No se encontró el proveedor o su estado no es válido.");
                        }

                        int estadoId = dr.GetInt32(
                            dr.GetOrdinal("EstadoId"));

                        if (estadoId != 1 && estadoId != 2)
                        {
                            throw new InvalidOperationException(
                                "SQL Server devolvió un estado no válido.");
                        }

                        return estadoId;
                    }
                }
            }
        }
    }
}
