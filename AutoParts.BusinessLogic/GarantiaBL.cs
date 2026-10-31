using AutoParts.DataAccess;
using AutoParts.Entity;
using System;
using System.Collections.Generic;

namespace AutoParts.BusinessLogic
{
    public class GarantiaBL
    {
        private readonly GarantiaRepository _repository;

        public GarantiaBL(GarantiaRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public List<Garantia> SelectAll()
        {
            return _repository.SelectAll();
        }

        // Método heredado: se mantiene para compatibilidad.
        // Las nuevas solicitudes del Dashboard deben usar RegistrarSolicitud,
        // ya que el procedimiento antiguo spGarantiaInsert no aplica las reglas nuevas.
        public int Insert(Garantia entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return _repository.Insert(entity);
        }

        // Registra una solicitud; no la aprueba ni modifica inventario.
        public GarantiaSolicitudResultado RegistrarSolicitud(
            int detalleVentaId,
            int empleadoId,
            int cantidadReclamada,
            string descripcionFalla,
            string? observaciones = null)
        {
            if (detalleVentaId <= 0)
                throw new ArgumentException("Selecciona un detalle de venta válido.", nameof(detalleVentaId));
            if (empleadoId <= 0)
                throw new ArgumentException("Selecciona un empleado válido.", nameof(empleadoId));
            if (cantidadReclamada <= 0)
                throw new ArgumentException("La cantidad reclamada debe ser mayor que cero.", nameof(cantidadReclamada));

            descripcionFalla = (descripcionFalla ?? string.Empty).Trim();
            if (descripcionFalla.Length == 0 || descripcionFalla.Length > 1000)
                throw new ArgumentException("Describe la falla (máximo 1000 caracteres).", nameof(descripcionFalla));

            observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();
            if (observaciones?.Length > 1000)
                throw new ArgumentException("Las observaciones no pueden superar 1000 caracteres.", nameof(observaciones));

            // SQL Server valida la venta original, el plazo y la cantidad no reclamada.
            return _repository.RegistrarSolicitud(
                detalleVentaId, empleadoId, cantidadReclamada, descripcionFalla, observaciones);
        }

        // La evaluación solo permite aprobar un defecto de fabricación o rechazarlo.
        // Nunca descuenta stock; la entrega se gestiona por el módulo de reemplazos.
        public GarantiaEvaluacionResultado Evaluar(
            int garantiaId,
            int empleadoId,
            string decision,
            bool defectoFabricacion,
            string? observaciones = null)
        {
            if (garantiaId <= 0)
                throw new ArgumentException("Selecciona una garantía válida.", nameof(garantiaId));
            if (empleadoId <= 0)
                throw new ArgumentException("Selecciona un empleado válido.", nameof(empleadoId));

            decision = (decision ?? string.Empty).Trim().ToUpperInvariant();
            if (decision != "APROBADA" && decision != "RECHAZADA")
                throw new ArgumentException("La decisión debe ser APROBADA o RECHAZADA.", nameof(decision));
            if (decision == "APROBADA" && !defectoFabricacion)
                throw new ArgumentException("Solo pueden aprobarse defectos de fabricación.", nameof(defectoFabricacion));
            if (decision == "RECHAZADA" && defectoFabricacion)
                throw new ArgumentException("Una solicitud rechazada no debe confirmar defecto de fabricación.", nameof(defectoFabricacion));

            observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();
            if (observaciones?.Length > 1000)
                throw new ArgumentException("Las observaciones no pueden superar 1000 caracteres.", nameof(observaciones));
            if (decision == "RECHAZADA" && observaciones == null)
                throw new ArgumentException("Explica el motivo del rechazo.", nameof(observaciones));

            return _repository.Evaluar(
                garantiaId, empleadoId, decision, defectoFabricacion, observaciones);
        }
    }
}
