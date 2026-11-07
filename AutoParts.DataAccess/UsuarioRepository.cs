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
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public List<Usuario> SelectAll()
        {
            List<Usuario> result = new List<Usuario>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spUsuarioSelectAll", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    while (dr.Read())
                    {
                        Usuario usuario = LeerUsuario(dr);
                        usuario.Contrasena = string.Empty;
                        result.Add(usuario);
                    }
                }
            }

            return result;
        }

        public int Insert(Usuario entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrWhiteSpace(entity.Contrasena))
                throw new ArgumentException("Falta el hash de la contraseña.");

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spUsuarioInsert", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@EmpleadoId", SqlDbType.Int).Value = entity.EmpleadoId;
                cmd.Parameters.Add("@RolId", SqlDbType.Int).Value = entity.RolId;
                cmd.Parameters.Add("@NombreUsuario", SqlDbType.VarChar, 50).Value = entity.NombreUsuario;
                cmd.Parameters.Add("@Contraseña", SqlDbType.VarChar, 255).Value = entity.Contrasena;
                cmd.Parameters.Add("@EstadoId", SqlDbType.Int).Value = entity.EstadoId ?? 1;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Update(Usuario entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spUsuarioUpdate", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = entity.UsuarioId;
                cmd.Parameters.Add("@EmpleadoId", SqlDbType.Int).Value = entity.EmpleadoId;
                cmd.Parameters.Add("@RolId", SqlDbType.Int).Value = entity.RolId;
                cmd.Parameters.Add("@NombreUsuario", SqlDbType.VarChar, 50).Value = entity.NombreUsuario;
                cmd.Parameters.Add("@Contraseña", SqlDbType.VarChar, 255).Value =
                    string.IsNullOrEmpty(entity.Contrasena) ? DBNull.Value : (object)entity.Contrasena;

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public int Delete(Usuario entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (entity.UsuarioId <= 0)
                throw new ArgumentException("Selecciona un usuario válido.");

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spUsuarioDelete", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = entity.UsuarioId;
                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        throw new InvalidOperationException("SQL Server no confirmó el cambio de estado.");

                    int estadoId = dr.GetInt32(dr.GetOrdinal("EstadoId"));
                    if (estadoId != 1 && estadoId != 2)
                        throw new InvalidOperationException("SQL Server devolvió un estado no válido.");

                    return estadoId;
                }
            }
        }

        public Usuario? SelectByNombreUsuario(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                return null;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spUsuarioSelectByNombreUsuario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@NombreUsuario", SqlDbType.VarChar, 50).Value = nombreUsuario.Trim();
                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    return dr.Read() ? LeerUsuario(dr) : null;
                }
            }
        }

        private static Usuario LeerUsuario(SqlDataReader dr)
        {
            int columnaContrasena = dr.GetOrdinal("Contraseña");
            int columnaEstado = dr.GetOrdinal("EstadoId");

            return new Usuario
            {
                UsuarioId = dr.GetInt32(dr.GetOrdinal("UsuarioId")),
                EmpleadoId = dr.GetInt32(dr.GetOrdinal("EmpleadoId")),
                RolId = dr.GetInt32(dr.GetOrdinal("RolId")),
                NombreUsuario = dr.GetString(dr.GetOrdinal("NombreUsuario")),
                Contrasena = dr.IsDBNull(columnaContrasena)
                    ? string.Empty
                    : dr.GetString(columnaContrasena),
                EstadoId = dr.IsDBNull(columnaEstado)
                    ? null
                    : dr.GetInt32(columnaEstado)
            };
        }


        public Usuario? ValidarSesion(int usuarioId)
        {
            if (usuarioId < 0)
                return null;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            using (SqlCommand cmd = new SqlCommand("dbo.spUsuarioValidarSesion", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UsuarioId", SqlDbType.Int).Value = usuarioId;

                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!dr.Read())
                        return null;

                    return new Usuario
                    {
                        UsuarioId = dr.GetInt32(dr.GetOrdinal("UsuarioId")),
                        EmpleadoId = dr.GetInt32(dr.GetOrdinal("EmpleadoId")),
                        RolId = dr.GetInt32(dr.GetOrdinal("RolId")),
                        NombreUsuario = dr.GetString(dr.GetOrdinal("NombreUsuario")),
                        Contrasena = string.Empty,
                        EstadoId = 1
                    };
                }
            }
        }


    }
}