
using Microsoft.Data.SqlClient;
using AutoParts.Entity;
using System;
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
            using (SqlCommand cmd = new SqlCommand("spProductoSelectAll", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                {
                    int colProductoId = dr.GetOrdinal("ProductoId");
                    int colSubcategoriaId = dr.GetOrdinal("SubcategoriaId");
                    int colCodigoOEM = dr.GetOrdinal("CodigoOEM");
                    int colNombre = dr.GetOrdinal("Nombre");
                    int colDescripcion = dr.GetOrdinal("Descripcion");
                    int colPrecioVenta = dr.GetOrdinal("PrecioVenta");
                    int colDiasGarantia = dr.GetOrdinal("DiasGarantia");
                    int colEstadoId = dr.GetOrdinal("EstadoId");
                    int colImagen = dr.GetOrdinal("Imagen");
                    int colCodigoBarras = dr.GetOrdinal("CodigoBarras");

                    while (dr.Read())
                    {
                        Producto entity = new Producto
                        {
                            ProductoId = dr.GetInt32(colProductoId),
                            SubcategoriaId = dr.GetInt32(colSubcategoriaId),
                            CodigoOEM = dr.GetString(colCodigoOEM),
                            Nombre = dr.GetString(colNombre),
                            Descripcion = dr.GetString(colDescripcion),
                            PrecioVenta = dr.GetDecimal(colPrecioVenta),
                            DiasGarantia = dr.GetInt32(colDiasGarantia),
                            EstadoId = dr.GetInt32(colEstadoId),
                            Imagen = dr.IsDBNull(colImagen)
                                ? null
                                : dr.GetString(colImagen),
                            CodigoBarras = dr.IsDBNull(colCodigoBarras)
                                ? null
                                : dr.GetString(colCodigoBarras)
                        };

                        result.Add(entity);
                    }
                }
            }

            return result;
        }

        public int Insert(Producto entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (SqlConnection conn = _connectionFactory.CreateConnection())
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

                cmd.Parameters.Add("@Imagen", SqlDbType.NVarChar, 500)
                    .Value = string.IsNullOrWhiteSpace(entity.Imagen)
                        ? DBNull.Value
                        : entity.Imagen;

                // SQL Server genera CodigoBarras automáticamente.

                conn.Open();

                return cmd.ExecuteNonQuery();
            }
        }
    }
}
