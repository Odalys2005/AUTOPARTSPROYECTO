using System;
using System.Collections.Generic;
using System.Text;

namespace AutoParts.Entity
{
    public class Vehiculo
    {
        public int VehiculoId { get; set; }
        public int ModeloId { get; set; }
        public short Anio { get; set; }
        public decimal Cilindrada { get; set; }
        public string TipoMotor { get; set; }
        public int EstadoId { get; set; }

        public Vehiculo()
        {

        }

        public Vehiculo(int vehiculoId, int modeloId, short anio,
                        decimal cilindrada, string tipoMotor, int estadoId)
        {
            VehiculoId = vehiculoId;
            ModeloId = modeloId;
            Anio = anio;
            Cilindrada = cilindrada;
            TipoMotor = tipoMotor;
            EstadoId = estadoId;
        }
    }
}