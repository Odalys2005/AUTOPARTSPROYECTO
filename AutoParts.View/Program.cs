
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

            // DAL
            services.AddTransient<CategoriaRepository>();
            services.AddTransient<ProveedorRepository>();
            services.AddTransient<CompraRepository>();
            services.AddTransient<Detalle_CompraRepository>();
            services.AddTransient<ProductoRepository>();
            services.AddTransient<InventarioRepository>();
            services.AddTransient<MarcaRepository>();
            services.AddTransient<EstadoRepository>();

            // BL
            services.AddScoped<CategoriaBL>();
            services.AddScoped<EstadoBL>();

            // Form
            services.AddTransient<MainForm>();
            services.AddTransient<frmCategoriasNuevo>();
            services.AddTransient<frmCategorias>();
            services.AddTransient<Login>();

            var serviceProvider = services.BuildServiceProvider();

            var mainForm = serviceProvider.GetRequiredService<Login>();

            Application.Run(mainForm);
        }
    }
}
