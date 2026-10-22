using System;
using System.Collections.Generic;
using System.Text;

namespace AutoParts.Entity
{
    public class CompatibilidadRepuesto
    {
        public int CompatibilidadId { get; set; }
        public int ProductoId { get; set; }
        public int? VehiculoId { get; set; }

        public CompatibilidadRepuesto()
        {

        }

        public CompatibilidadRepuesto(int compatibilidadId,
                                      int productoId, int? vehiculoId)
        {
            CompatibilidadId = compatibilidadId;
            ProductoId = productoId;
            VehiculoId = vehiculoId;
        }
    }
}