
using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class ClienteRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public ClienteRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Cliente> SelectAll()
        {
            List<Cliente> result = new List<Cliente>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spClienteSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        int ordinalClienteId = dr.GetOrdinal("ClienteId");
                        int ordinalNombre = dr.GetOrdinal("Nombre");
                        int ordinalApellido = dr.GetOrdinal("Apellido");
                        int ordinalDUI = dr.GetOrdinal("DUI");
                        int ordinalNIT = dr.GetOrdinal("NIT");
                        int ordinalTelefono = dr.GetOrdinal("Telefono");
                        int ordinalCorreo = dr.GetOrdinal("Correo");
                        int ordinalDireccion = dr.GetOrdinal("Direccion");
                        int ordinalEstadoId = dr.GetOrdinal("EstadoId");

                        while (dr.Read())
                        {
                            Cliente entity = new Cliente();

                            entity.ClienteId = dr.GetInt32(ordinalClienteId);
                            entity.Nombre = dr.IsDBNull(ordinalNombre) ? "" : dr.GetString(ordinalNombre);
                            entity.Apellido = dr.IsDBNull(ordinalApellido) ? "" : dr.GetString(ordinalApellido);
                            entity.DUI = dr.IsDBNull(ordinalDUI) ? "" : dr.GetString(ordinalDUI);
                            entity.NIT = dr.IsDBNull(ordinalNIT) ? null : dr.GetString(ordinalNIT);
                            entity.Telefono = dr.IsDBNull(ordinalTelefono) ? "" : dr.GetString(ordinalTelefono);
                            entity.Correo = dr.IsDBNull(ordinalCorreo) ? "" : dr.GetString(ordinalCorreo);
                            entity.Direccion = dr.IsDBNull(ordinalDireccion) ? null : dr.GetString(ordinalDireccion);
                            entity.EstadoId = dr.GetInt32(ordinalEstadoId);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public Cliente? SelectById(int clienteId)
        {
            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spClienteSelectById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!dr.Read())
                        {
                            return null;
                        }

                        Cliente entity = new Cliente();

                        entity.ClienteId = dr.GetInt32(dr.GetOrdinal("ClienteId"));
                        entity.Nombre = dr.GetString(dr.GetOrdinal("Nombre"));
                        entity.Apellido = dr.GetString(dr.GetOrdinal("Apellido"));
                        entity.DUI = dr.GetString(dr.GetOrdinal("DUI"));

                        int ordinalNIT = dr.GetOrdinal("NIT");
                        entity.NIT = dr.IsDBNull(ordinalNIT) ? null : dr.GetString(ordinalNIT);

                        entity.Telefono = dr.GetString(dr.GetOrdinal("Telefono"));
                        entity.Correo = dr.GetString(dr.GetOrdinal("Correo"));

                        int ordinalDireccion = dr.GetOrdinal("Direccion");
                        entity.Direccion = dr.IsDBNull(ordinalDireccion) ? null : dr.GetString(ordinalDireccion);

                        entity.EstadoId = dr.GetInt32(dr.GetOrdinal("EstadoId"));

                        return entity;
                    }
                }
            }
        }

        public int Insert(Cliente entity)
        {
            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spClienteInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = entity.Nombre;
                    cmd.Parameters.Add("@Apellido", SqlDbType.VarChar, 100).Value = entity.Apellido;
                    cmd.Parameters.Add("@DUI", SqlDbType.VarChar, 10).Value = entity.DUI;
                    cmd.Parameters.Add("@NIT", SqlDbType.VarChar, 20).Value =
                        (object?)entity.NIT ?? DBNull.Value;
                    cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 25).Value = entity.Telefono;
                    cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 150).Value = entity.Correo;
                    cmd.Parameters.Add("@Direccion", SqlDbType.VarChar, 50).Value =
                        (object?)entity.Direccion ?? DBNull.Value;

                    conn.Open();

                    cmd.ExecuteNonQuery();

                    return 1;
                }
            }
        }

        public int Update(Cliente entity)
        {
            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spClienteUpdate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = entity.ClienteId;
                    cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100).Value = entity.Nombre;
                    cmd.Parameters.Add("@Apellido", SqlDbType.VarChar, 100).Value = entity.Apellido;
                    cmd.Parameters.Add("@DUI", SqlDbType.VarChar, 10).Value = entity.DUI;
                    cmd.Parameters.Add("@NIT", SqlDbType.VarChar, 20).Value =
                        (object?)entity.NIT ?? DBNull.Value;
                    cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 25).Value = entity.Telefono;
                    cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 150).Value = entity.Correo;
                    cmd.Parameters.Add("@Direccion", SqlDbType.VarChar, 50).Value =
                        (object?)entity.Direccion ?? DBNull.Value;

                    conn.Open();

                    cmd.ExecuteNonQuery();

                    return 1;
                }
            }
        }

        public int Delete(int clienteId)
        {
            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spClienteDelete", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@ClienteId", SqlDbType.Int).Value = clienteId;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!dr.Read())
                        {
                            throw new InvalidOperationException(
                                "No fue posible cambiar el estado del cliente.");
                        }

                        return dr.GetInt32(dr.GetOrdinal("EstadoId"));
                    }
                }
            }
        }
    }
}
