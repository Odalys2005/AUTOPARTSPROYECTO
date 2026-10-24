using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class UsuarioRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public UsuarioRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Usuario> SelectAll()
        {
            List<Usuario> result = new List<Usuario>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spUsuarioSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            Usuario entity = new Usuario();

                            entity.UsuarioId = dr.GetInt32(0);
                            entity.EmpleadoId = dr.GetInt32(1);
                            entity.RolId = dr.GetInt32(2);
                            entity.NombreUsuario = dr.GetString(3);
                            entity.Contrasena = dr.GetString(4);
                            entity.EstadoId = dr.IsDBNull(5)
                                ? null
                                : dr.GetInt32(5);

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(Usuario entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spUsuarioInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@EmpleadoId", SqlDbType.Int)
                        .Value = entity.EmpleadoId;

                    cmd.Parameters.Add("@RolId", SqlDbType.Int)
                        .Value = entity.RolId;

                    cmd.Parameters.Add("@NombreUsuario", SqlDbType.VarChar, 50)
                        .Value = entity.NombreUsuario;

                    cmd.Parameters.Add("@Contraseña", SqlDbType.VarChar, 255)
                        .Value = entity.Contrasena;

                    cmd.Parameters.Add("@EstadoId", SqlDbType.Int)
                        .Value = (object?)entity.EstadoId ?? DBNull.Value;

                    conn.Open();

                    result = cmd.ExecuteNonQuery();
                }
            }

            return result;
        }
    
    
public Usuario? SelectByNombreUsuario(string nombreUsuario)
        {
            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand(
                "dbo.spUsuarioSelectByNombreUsuario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@NombreUsuario", SqlDbType.VarChar, 50)
                    .Value = nombreUsuario;

                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(
                    CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        return null;

                    return new Usuario
                    {
                        UsuarioId = dr.GetInt32(0),
                        EmpleadoId = dr.GetInt32(1),
                        RolId = dr.GetInt32(2),
                        NombreUsuario = dr.GetString(3),
                        Contrasena = dr.GetString(4),
                        EstadoId = dr.IsDBNull(5)
                            ? null
                            : dr.GetInt32(5)
                    };
                }
            }
        }

    }
}