
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoParts.BusinessLogic;
using AutoParts.DataAccess;
using AutoParts.Entity;

namespace AutoParts.View
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("conn")!;

            var services = new ServiceCollection();

            services.AddSingleton(new ConnectionFactory(connectionString));

            // Categorías y productos
            services.AddTransient<CategoriaRepository>();
            services.AddTransient<SubcategoriaRepository>();
            services.AddTransient<ProductoRepository>();
            services.AddTransient<InventarioRepository>();

            // Marcas y vehículos
            services.AddTransient<MarcaRepository>();
            services.AddTransient<ModeloRepository>();
            services.AddTransient<VehiculoRepository>();
            services.AddTransient<CompatibilidadRepuestoRepository>();

            // Personal y seguridad
            services.AddTransient<EstadoRepository>();
            services.AddTransient<RolRepository>();
            services.AddTransient<PuestoRepository>();
            services.AddTransient<EmpleadoRepository>();
            services.AddTransient<UsuarioRepository>();

            // Proveedores y compras
            services.AddTransient<ProveedorRepository>();
            services.AddTransient<CompraRepository>();
            services.AddTransient<Detalle_CompraRepository>();

            // Clientes y ventas
            services.AddTransient<ClienteRepository>();
            services.AddTransient<VentaRepository>();
            services.AddTransient<Detalle_VentaRepository>();

            // Garantías
            services.AddTransient<GarantiaRepository>();
            services.AddTransient<ReemplazoGarantiaRepository>();
            //BL

            services.AddScoped<CategoriaBL>();
            services.AddScoped<EstadoBL>();
            services.AddScoped<RolBL>();
            services.AddScoped<ProveedorBL>();
            services.AddScoped<ClienteBL>();
            services.AddScoped<MarcaBL>();
            services.AddScoped<PuestoBL>();
            //VIEW
           
            services.AddTransient<MainForm>();

            services.AddTransient<frmCategoriasNuevo>();
            services.AddTransient<frmCategorias>();

            services.AddTransient<Login>();

            services.AddTransient<frmEstados>();

            services.AddTransient<frmRoles>();
            services.AddTransient<frmRolesNuevo>();

            services.AddTransient<frmProveedores>();
            services.AddTransient<frmProveedoresNuevo>();

            services.AddTransient<frmClientes>();
            services.AddTransient<frmClientesNuevo>();

            services.AddTransient<frmMarcas>();
            services.AddTransient<frmMarcasNuevo>();

            services.AddTransient<frmPuestos>();
            services.AddTransient<frmPuestosNuevo>();

            

            var serviceProvider = services.BuildServiceProvider();

            var mainForm = serviceProvider.GetRequiredService<MainForm>();

            Application.Run(mainForm);
        }
    }
}
