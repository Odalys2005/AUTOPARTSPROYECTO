
using System.Collections.Generic;

namespace AutoParts.Entity
{
    public class VentaRegistroCompleto
    {
        public int ClienteId { get; set; }
        public int EmpleadoId { get; set; }
        public string NumeroFactura { get; set; } = string.Empty;
        public int EstadoId { get; set; }

        public List<VentaLineaRegistro> Lineas { get; set; } = new();
    }

    public class VentaLineaRegistro
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }


    public class VentaRegistroResultado
    {
        public int VentaId { get; set; }

        public string NumeroFactura { get; set; } = string.Empty;

        public decimal Total { get; set; }
    }

}
