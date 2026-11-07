
using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace AutoParts.BusinessLogic
{
    public class ClienteBL
    {
        private readonly ClienteRepository _repository;

        public ClienteBL(ClienteRepository repository)
        {
            _repository = repository;
        }

        public List<Cliente> SelectAll()
        {
            return _repository.SelectAll();
        }

        public Cliente? SelectById(int clienteId)
        {
            if (clienteId <= 0)
                throw new ArgumentException("Selecciona un cliente válido.");

            return _repository.SelectById(clienteId);
        }

        public int Insert(Cliente entity)
        {
            ValidarCliente(entity);

            entity.EstadoId = 1;

            return _repository.Insert(entity);
        }

        public int Update(Cliente entity)
        {
            if (entity == null)
                throw new ArgumentException("Los datos del cliente son obligatorios.");

            if (entity.ClienteId <= 0)
                throw new ArgumentException("Selecciona un cliente válido.");

            ValidarCliente(entity);

            return _repository.Update(entity);
        }

        public int Delete(int clienteId)
        {
            if (clienteId <= 0)
                throw new ArgumentException("Selecciona un cliente válido.");

            return _repository.Delete(clienteId);
        }

        private static void ValidarCliente(Cliente entity)
        {
            if (entity == null)
                throw new ArgumentException("Los datos del cliente son obligatorios.");

            entity.Nombre = (entity.Nombre ?? "").Trim();
            entity.Apellido = (entity.Apellido ?? "").Trim();
            entity.DUI = (entity.DUI ?? "").Trim();
            entity.NIT = string.IsNullOrWhiteSpace(entity.NIT)
                ? null
                : entity.NIT.Trim();
            entity.Telefono = (entity.Telefono ?? "").Trim();
            entity.Correo = (entity.Correo ?? "").Trim();
            entity.Direccion = string.IsNullOrWhiteSpace(entity.Direccion)
                ? null
                : entity.Direccion.Trim();

            if (string.IsNullOrWhiteSpace(entity.Nombre))
                throw new ArgumentException("El nombre del cliente es obligatorio.");

            if (entity.Nombre.Length > 100)
                throw new ArgumentException("El nombre no puede superar los 100 caracteres.");

            if (string.IsNullOrWhiteSpace(entity.Apellido))
                throw new ArgumentException("El apellido del cliente es obligatorio.");

            if (entity.Apellido.Length > 100)
                throw new ArgumentException("El apellido no puede superar los 100 caracteres.");

            if (string.IsNullOrWhiteSpace(entity.DUI))
                throw new ArgumentException("El DUI del cliente es obligatorio.");

            if (entity.DUI.Length != 10 ||
                entity.DUI[8] != '-' ||
                !EsNumerico(entity.DUI.Substring(0, 8)) ||
                !EsNumerico(entity.DUI.Substring(9, 1)))
                throw new ArgumentException("El DUI debe tener el formato 00000000-0.");

            if (entity.NIT != null && entity.NIT.Length > 20)
                throw new ArgumentException("El NIT no puede superar los 20 caracteres.");

            if (string.IsNullOrWhiteSpace(entity.Telefono))
                throw new ArgumentException("El teléfono del cliente es obligatorio.");

            if (entity.Telefono.Length > 25)
                throw new ArgumentException("El teléfono no puede superar los 25 caracteres.");

            if (string.IsNullOrWhiteSpace(entity.Correo))
                throw new ArgumentException("El correo electrónico del cliente es obligatorio.");

            if (entity.Correo.Length > 150)
                throw new ArgumentException("El correo no puede superar los 150 caracteres.");

            try
            {
                MailAddress correo = new MailAddress(entity.Correo);

                if (!string.Equals(
                    correo.Address,
                    entity.Correo,
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Ingresa un correo electrónico válido.");
                }
            }
            catch (FormatException)
            {
                throw new ArgumentException("Ingresa un correo electrónico válido.");
            }

            if (entity.Direccion != null && entity.Direccion.Length > 50)
                throw new ArgumentException("La dirección no puede superar los 50 caracteres.");
        }

        private static bool EsNumerico(string valor)
        {
            foreach (char caracter in valor)
            {
                if (caracter < '0' || caracter > '9')
                    return false;
            }

            return true;
        }
    }
}
