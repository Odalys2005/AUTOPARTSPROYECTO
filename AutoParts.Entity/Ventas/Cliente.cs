using System;
using System.Collections.Generic;
using System.Text;

namespace AutoParts.Entity
{
    public class Cliente
    {
        public int ClienteId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string DUI { get; set; } = string.Empty;
        public string? NIT { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public int EstadoId { get; set; } = 1;

        public Cliente()
        {
        }

        public Cliente(
            int clienteId,
            string nombre,
            string apellido,
            string dui,
            string? nit,
            string telefono,
            string correo,
            string? direccion)
        {
            ClienteId = clienteId;
            Nombre = nombre;
            Apellido = apellido;
            DUI = dui;
            NIT = nit;
            Telefono = telefono;
            Correo = correo;
            Direccion = direccion;
            EstadoId = 1;
        }

        public Cliente(
            int clienteId,
            string nombre,
            string apellido,
            string dui,
            string? nit,
            string telefono,
            string correo,
            string? direccion,
            int estadoId)
            : this(
                clienteId,
                nombre,
                apellido,
                dui,
                nit,
                telefono,
                correo,
                direccion)
        {
            EstadoId = estadoId;
        }
    }
}
