using System;
using System.Collections.Generic;
using System.Text;

namespace AutoParts.Entity
{
    public class Producto
    {
        public int ProductoId { get; set; }
        public string CodigoOEM { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioVenta { get; set; }
        public int DiasGarantia { get; set; }
        public string? Imagen { get; set; }
        public string? CodigoBarras { get; set; }

        public int SubcategoriaId { get; set; }
        public int EstadoId { get; set; }

        public Producto()
        {

        }

        public Producto(int productoId, string codigoOEM,
                        string nombre, string descripcion,
                        decimal precioVenta, int diasGarantia, string? imagen, string? codigoBarras,
                        int subcategoriaId, int estadoId)
        {
            ProductoId = productoId;
            CodigoOEM = codigoOEM;
            Nombre = nombre;
            Descripcion = descripcion;
            PrecioVenta = precioVenta;
            DiasGarantia = diasGarantia;
            Imagen = imagen;
            CodigoBarras = codigoBarras;
            SubcategoriaId = subcategoriaId;
            EstadoId = estadoId;
        }
    }
}