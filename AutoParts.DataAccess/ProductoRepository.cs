using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System.Collections.Generic;
using System.Data;

namespace AutoParts.DataAccess
{
    public class ProductoRepository
    {
        private readonly ConnectionFactory _connectionFactory;

        public ProductoRepository(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Producto> SelectAll()
        {
            List<Producto> result = new List<Producto>();

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spProductoSelectAll", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (dr.Read())
                        {
                            Producto entity = new Producto();

                            entity.ProductoId = dr.GetInt32(
                                dr.GetOrdinal("ProductoId"));

                            entity.CodigoOEM = dr.GetString(
                                dr.GetOrdinal("CodigoOEM"));

                            entity.Nombre = dr.GetString(
                                dr.GetOrdinal("Nombre"));

                            entity.Descripcion = dr.GetString(
                                dr.GetOrdinal("Descripcion"));

                            entity.PrecioVenta = dr.GetDecimal(
                                dr.GetOrdinal("PrecioVenta"));

                            entity.DiasGarantia = dr.GetInt32(
                                dr.GetOrdinal("DiasGarantia"));

                            entity.SubcategoriaId = dr.GetInt32(
                                dr.GetOrdinal("SubcategoriaId"));

                            entity.EstadoId = dr.GetInt32(
                                dr.GetOrdinal("EstadoId"));

                            result.Add(entity);
                        }
                    }
                }
            }

            return result;
        }

        public int Insert(Producto entity)
        {
            int result = 0;

            using (SqlConnection conn = _connectionFactory.CreateConnection())
            {
                using (SqlCommand cmd = new SqlCommand("spProductoInsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@SubcategoriaId", SqlDbType.Int)
                        .Value = entity.SubcategoriaId;

                    cmd.Parameters.Add("@CodigoOEM", SqlDbType.VarChar, 50)
                        .Value = entity.CodigoOEM;

                    cmd.Parameters.Add("@Nombre", SqlDbType.VarChar, 100)
                        .Value = entity.Nombre;

                    cmd.Parameters.Add("@Descripcion", SqlDbType.VarChar, 500)
                        .Value = entity.Descripcion;

                    SqlParameter precioVenta = cmd.Parameters.Add(
                        "@PrecioVenta", SqlDbType.Decimal);

                    precioVenta.Precision = 18;
                    precioVenta.Scale = 2;
                    precioVenta.Value = entity.PrecioVenta;

                    cmd.Parameters.Add("@DiasGarantia", SqlDbType.Int)
                        .Value = entity.DiasGarantia;

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