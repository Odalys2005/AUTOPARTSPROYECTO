using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class ReemplazoGarantiaBL
    {
        private readonly ReemplazoGarantiaRepository _repository;

        public ReemplazoGarantiaBL(ReemplazoGarantiaRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        // Conserva el listado de reemplazos ya registrados.
        public List<ReemplazoGarantia> SelectAll()
        {
            return _repository.SelectAll();
        }

        // Flujo nuevo y seguro: SQL verifica que la garantía esté aprobada,
        // que no exista otra entrega y que haya stock disponible.
        // Registra la entrega y descuenta el inventario en una sola transacción.
        // Si no hay stock, retorna PENDIENTE_REEMPLAZO sin crear una entrega.
        public ReemplazoEntregaResultado EntregarReemplazo(
            int garantiaId,
            int empleadoId,
            string? observaciones = null)
        {
            if (garantiaId <= 0)
                throw new ArgumentException("Selecciona una garantía válida.", nameof(garantiaId));

            if (empleadoId <= 0)
                throw new ArgumentException("Selecciona un empleado válido.", nameof(empleadoId));

            observaciones = string.IsNullOrWhiteSpace(observaciones)
                ? null
                : observaciones.Trim();

            if (observaciones != null && observaciones.Length > 1000)
                throw new ArgumentException(
                    "Las observaciones no pueden superar 1000 caracteres.",
                    nameof(observaciones));

            ReemplazoEntregaResultado resultado =
                _repository.EntregarReemplazo(garantiaId, empleadoId, observaciones);

            if (resultado == null)
                throw new InvalidOperationException(
                    "No se recibió confirmación del estado del reemplazo.");

            if (resultado.EstadoGarantia != "REEMPLAZADA" &&
                resultado.EstadoGarantia != "PENDIENTE_REEMPLAZO")
                throw new InvalidOperationException(
                    "SQL Server devolvió un estado de reemplazo no esperado.");

            return resultado;
        }

        // LEGADO: se mantiene para no romper las referencias existentes.
        // NO usar en el Dashboard para entregas reales: el procedimiento
        // spReemplazoGarantiaInsert no descuenta inventario.
        public int Insert(ReemplazoGarantia entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return _repository.Insert(entity);
        }
    }
}
