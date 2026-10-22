
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
                        while (dr.Read())
                        {
                            Cliente entity = new Cliente();

                            entity.ClienteId = dr.GetInt32(0);
                            entity.Nombre = dr.IsDBNull(1) ? "" : dr.GetString(1);
                            entity.Apellido = dr.IsDBNull(2) ? "" : dr.GetString(2);
                            entity.DUI = dr.IsDBNull(3) ? "" : dr.GetString(3);
                            entity.NIT = dr.IsDBNull(4) ? "" : dr.GetString(4);
                            entity.Telefono = dr.IsDBNull(5) ? "" : dr.GetString(5);
                            entity.Correo = dr.IsDBNull(6) ? "" : dr.GetString(6);
                            entity.Direccion = dr.IsDBNull(7) ? "" : dr.GetString(7);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(Cliente entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spClienteInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Nombre", entity.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", entity.Apellido);
                    cmd.Parameters.AddWithValue("@DUI", entity.DUI);
                    cmd.Parameters.AddWithValue("@NIT",
                        (object?)entity.NIT ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefono",
                        (object?)entity.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Correo",
                        (object?)entity.Correo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Direccion",
                        (object?)entity.Direccion ?? DBNull.Value);

                    conn.Open();

                    result = cmd.ExecuteNonQuery();
                }
            }

            return result;
        }
    }
}
