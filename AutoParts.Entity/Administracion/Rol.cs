namespace AutoParts.Entity
{
    public class Rol
    {
        public int RolId { get; set; }
        public string Nombre { get; set; }
        public int EstadoId { get; set; }

        public Rol()
        {
        }

        public Rol(int rolId, string nombre, int estadoId)
        {
            RolId = rolId;
            Nombre = nombre;
            EstadoId = estadoId;
        }
    }
}