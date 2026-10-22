using System;
using System.Collections.Generic;
using System.Text;

namespace AutoParts.Entity.Productos
{
    public class Subcategoria
    {
        public int SubcategoriaId { get; set; }
        public int CategoriaId { get; set; }
        public string Nombre { get; set; }
        public int EstadoId { get; set; }

        public Subcategoria()
        {

        }

        public Subcategoria(int subcategoriaId, int categoriaId,
                           string nombre, int estadoId)
        {
            SubcategoriaId = subcategoriaId;
            CategoriaId = categoriaId;
            Nombre = nombre;
            EstadoId = estadoId;
        }
    }
}