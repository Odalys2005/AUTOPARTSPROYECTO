using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace AutoParts.BusinessLogic
{
    public class EmpleadoBL
    {
        private readonly EmpleadoRepository _repository;

        public EmpleadoBL(EmpleadoRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<Empleado> SelectAll()
        {
            return _repository.SelectAll();
        }

        public int Insert(Empleado entity)
        {
            ValidarEmpleado(entity, false);
            entity.EstadoId = 1;
            return _repository.Insert(entity);
        }

        public int Update(Empleado entity)
        {
            ValidarEmpleado(entity, true);
            return _repository.Update(entity);
        }

        public int Delete(Empleado entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity.EmpleadoId <= 0)
                throw new ArgumentException("Selecciona un empleado válido.");

            return _repository.Delete(entity);
        }

        private static void ValidarEmpleado(Empleado entity, bool esEdicion)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (esEdicion && entity.EmpleadoId <= 0)
                throw new ArgumentException("Selecciona un empleado válido.");

            if (entity.PuestoId <= 0)
                throw new ArgumentException("Selecciona un puesto válido.");

            entity.Nombre = ValidarTexto(entity.Nombre, 100, "nombre");
            entity.Apellido = ValidarTexto(entity.Apellido, 100, "apellido");
            entity.DUI = ValidarTexto(entity.DUI, 10, "DUI");
            entity.Telefono = ValidarTexto(entity.Telefono, 25, "teléfono");
            entity.Correo = ValidarTexto(entity.Correo, 150, "correo electrónico");

            if (!MailAddress.TryCreate(entity.Correo, out MailAddress? correo) ||
                !string.Equals(correo.Address, entity.Correo, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Ingresa un correo electrónico válido.");
        }

        private static string ValidarTexto(string? valor, int longitudMaxima, string campo)
        {
            string texto = (valor ?? string.Empty).Trim();

            if (texto.Length == 0)
                throw new ArgumentException($"El campo {campo} es obligatorio.");

            if (texto.Length > longitudMaxima)
                throw new ArgumentException($"El campo {campo} no puede superar {longitudMaxima} caracteres.");

            return texto;
        }
    }
}