CREATE DATABASE AutoParts
GO

USE AutoParts
GO

-- Estado
CREATE TABLE Estado
(
    EstadoId INT PRIMARY KEY IDENTITY,
    Nombre VARCHAR(100) NOT NULL
)
GO

CREATE PROC spEstadoSelectAll
AS
BEGIN
    SELECT * FROM Estado
END
GO

CREATE PROC spEstadoInsert
(
    @Nombre VARCHAR(100)
)
AS
BEGIN
    INSERT INTO Estado(Nombre)
    VALUES (@Nombre)
END
GO

-- Rol
CREATE TABLE Rol
(
    RolId INT PRIMARY KEY IDENTITY,
    Nombre VARCHAR(100) NOT NULL
)
GO

CREATE PROC spRolSelectAll
AS
BEGIN
    SELECT * FROM Rol
END
GO

CREATE PROC spRolInsert
(
    @Nombre VARCHAR(100)
)
AS
BEGIN
    INSERT INTO Rol(Nombre)
    VALUES (@Nombre)
END
GO

-- Proveedor
CREATE TABLE Proveedor
(
    ProveedorId INT PRIMARY KEY IDENTITY,
    NombreEmpresa VARCHAR(100) NOT NULL,
    NIT VARCHAR(20) NOT NULL,
    Telefono VARCHAR(25) NOT NULL,
    Correo VARCHAR(150) NOT NULL,
    Contacto VARCHAR(100) NOT NULL
)
GO

CREATE PROC spProveedorSelectAll
AS
BEGIN
    SELECT * FROM Proveedor
END
GO

CREATE PROC spProveedorInsert
(
    @NombreEmpresa VARCHAR(100),
    @NIT VARCHAR(20),
    @Telefono VARCHAR(25),
    @Correo VARCHAR(150),
    @Contacto VARCHAR(100)
)
AS
BEGIN
    INSERT INTO Proveedor(NombreEmpresa, NIT, Telefono, Correo, Contacto)
    VALUES (@NombreEmpresa, @NIT, @Telefono, @Correo, @Contacto)
END
GO

-- Cliente
CREATE TABLE Cliente
(
    ClienteId INT PRIMARY KEY IDENTITY,
    Nombre VARCHAR(100) NOT NULL,
    Apellido VARCHAR(100) NOT NULL,
    DUI VARCHAR(10) NOT NULL,
    NIT VARCHAR(20) NOT NULL,
    Telefono VARCHAR(25) NOT NULL,
    Correo VARCHAR(150) NOT NULL,
    Direccion VARCHAR(300) NOT NULL
)
GO

CREATE PROC spClienteSelectAll
AS
BEGIN
    SELECT * FROM Cliente
END
GO

CREATE PROC spClienteInsert
(
    @Nombre VARCHAR(100),
    @Apellido VARCHAR(100),
    @DUI VARCHAR(10),
    @NIT VARCHAR(20),
    @Telefono VARCHAR(25),
    @Correo VARCHAR(150),
    @Direccion VARCHAR(300)
)
AS
BEGIN
    INSERT INTO Cliente(Nombre, Apellido, DUI, NIT, Telefono, Correo, Direccion)
    VALUES (@Nombre, @Apellido, @DUI, @NIT, @Telefono, @Correo, @Direccion)
END
GO

-- Categoria
CREATE TABLE Categoria
(
    CategoriaId INT PRIMARY KEY IDENTITY,
    Nombre VARCHAR(100) NOT NULL,
    EstadoId INT NOT NULL FOREIGN KEY REFERENCES Estado(EstadoId)
)
GO

CREATE PROC spCategoriaSelectAll
AS
BEGIN
    SELECT * FROM Categoria
END
GO

CREATE PROC spCategoriaInsert
(
    @Nombre VARCHAR(100),
    @EstadoId INT
)
AS
BEGIN
    INSERT INTO Categoria(Nombre, EstadoId)
    VALUES (@Nombre, @EstadoId)
END
GO

-- Marca
CREATE TABLE Marca
(
    MarcaId INT PRIMARY KEY IDENTITY,
    Nombre VARCHAR(100) NOT NULL,
    EstadoId INT NOT NULL FOREIGN KEY REFERENCES Estado(EstadoId)
)
GO

CREATE PROC spMarcaSelectAll
AS
BEGIN
    SELECT * FROM Marca
END
GO

CREATE PROC spMarcaInsert
(
    @Nombre VARCHAR(100),
    @EstadoId INT
)
AS
BEGIN
    INSERT INTO Marca(Nombre, EstadoId)
    VALUES (@Nombre, @EstadoId)
END
GO

-- Modelo
CREATE TABLE Modelo
(
    ModeloId INT PRIMARY KEY IDENTITY,
    MarcaId INT NOT NULL FOREIGN KEY REFERENCES Marca(MarcaId),
    Nombre VARCHAR(100) NOT NULL,
    EstadoId INT NOT NULL FOREIGN KEY REFERENCES Estado(EstadoId)
)
GO

CREATE PROC spModeloSelectAll
AS
BEGIN
    SELECT * FROM Modelo
END
GO

CREATE PROC spModeloInsert
(
    @MarcaId INT,
    @Nombre VARCHAR(100),
    @EstadoId INT
)
AS
BEGIN
    INSERT INTO Modelo(MarcaId, Nombre, EstadoId)
    VALUES (@MarcaId, @Nombre, @EstadoId)
END
GO

-- Vehiculo
CREATE TABLE Vehiculo
(
    VehiculoId INT PRIMARY KEY IDENTITY,
    ModeloId INT NOT NULL FOREIGN KEY REFERENCES Modelo(ModeloId),
    Anio SMALLINT NOT NULL,
    Cilindrada DECIMAL(5,2) NOT NULL,
    TipoMotor VARCHAR(100) NOT NULL,
    EstadoId INT NOT NULL FOREIGN KEY REFERENCES Estado(EstadoId)
)
GO

CREATE PROC spVehiculoSelectAll
AS
BEGIN
    SELECT * FROM Vehiculo
END
GO

CREATE PROC spVehiculoInsert
(
    @ModeloId INT,
    @Anio SMALLINT,
    @Cilindrada DECIMAL(5,2),
    @TipoMotor VARCHAR(100),
    @EstadoId INT
)
AS
BEGIN
    INSERT INTO Vehiculo(ModeloId, Anio, Cilindrada, TipoMotor, EstadoId)
    VALUES (@ModeloId, @Anio, @Cilindrada, @TipoMotor, @EstadoId)
END
GO

-- Puesto
CREATE TABLE Puesto
(
    PuestoId INT PRIMARY KEY IDENTITY,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(500) NOT NULL,
    EstadoId INT NOT NULL FOREIGN KEY REFERENCES Estado(EstadoId)
)
GO

CREATE PROC spPuestoSelectAll
AS
BEGIN
    SELECT * FROM Puesto
END
GO

CREATE PROC spPuestoInsert
(
    @Nombre VARCHAR(100),
    @Descripcion VARCHAR(500),
    @EstadoId INT
)
AS
BEGIN
    INSERT INTO Puesto(Nombre, Descripcion, EstadoId)
    VALUES (@Nombre, @Descripcion, @EstadoId)
END
GO

-- Empleado
CREATE TABLE Empleado
(
    EmpleadoId INT PRIMARY KEY IDENTITY,
    PuestoId INT NOT NULL FOREIGN KEY REFERENCES Puesto(PuestoId),
    Nombre VARCHAR(100) NOT NULL,
    Apellido VARCHAR(100) NOT NULL,
    DUI VARCHAR(10) NOT NULL,
    Telefono VARCHAR(25) NOT NULL,
    Correo VARCHAR(150) NOT NULL,
    EstadoId INT NOT NULL FOREIGN KEY REFERENCES Estado(EstadoId)
)
GO

CREATE PROC spEmpleadoSelectAll
AS
BEGIN
    SELECT * FROM Empleado
END
GO

CREATE PROC spEmpleadoInsert
(
    @PuestoId INT,
    @Nombre VARCHAR(100),
    @Apellido VARCHAR(100),
    @DUI VARCHAR(10),
    @Telefono VARCHAR(25),
    @Correo VARCHAR(150),
    @EstadoId INT
)
AS
BEGIN
    INSERT INTO Empleado(PuestoId, Nombre, Apellido, DUI, Telefono, Correo, EstadoId)
    VALUES (@PuestoId, @Nombre, @Apellido, @DUI, @Telefono, @Correo, @EstadoId)
END
GO

-- Usuario
CREATE TABLE Usuario
(
    UsuarioId INT PRIMARY KEY IDENTITY,
    EmpleadoId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Empleado(EmpleadoId),
    RolId INT NOT NULL FOREIGN KEY REFERENCES Rol(RolId),
    NombreUsuario VARCHAR(50) NOT NULL UNIQUE,
    Contraseña VARCHAR(255) NOT NULL,
    EstadoId INT NULL FOREIGN KEY REFERENCES Estado(EstadoId)
)
GO

CREATE PROC spUsuarioSelectAll
AS
BEGIN
    SELECT * FROM Usuario
END
GO

CREATE PROC spUsuarioInsert
(
    @EmpleadoId INT,
    @RolId INT,
    @NombreUsuario VARCHAR(50),
    @Contraseña VARCHAR(255),
    @EstadoId INT
)
AS
BEGIN
    INSERT INTO Usuario(EmpleadoId, RolId, NombreUsuario, Contraseña, EstadoId)
    VALUES (@EmpleadoId, @RolId, @NombreUsuario, @Contraseña, @EstadoId)
END
GO

-- Producto
CREATE TABLE Producto
(
    ProductoId INT PRIMARY KEY IDENTITY,
    CategoriaId INT NOT NULL FOREIGN KEY REFERENCES Categoria(CategoriaId),
    CodigoOEM VARCHAR(50) NOT NULL,
    Nombre VARCHAR(100) NOT NULL,
    Descripcion VARCHAR(500) NOT NULL,
    PrecioVenta DECIMAL(18,2) NOT NULL CHECK (PrecioVenta >= 0),
    DiasGarantia INT NOT NULL DEFAULT 0 CHECK (DiasGarantia >= 0),
    EstadoId INT NOT NULL FOREIGN KEY REFERENCES Estado(EstadoId)
)
GO

CREATE PROC spProductoSelectAll
AS
BEGIN
    SELECT * FROM Producto
END
GO

CREATE PROC spProductoInsert
(
    @CategoriaId INT,
    @CodigoOEM VARCHAR(50),
    @Nombre VARCHAR(100),
    @Descripcion VARCHAR(500),
    @PrecioVenta DECIMAL(18,2),
    @DiasGarantia INT,
    @EstadoId INT
)
AS
BEGIN
    INSERT INTO Producto(CategoriaId, CodigoOEM, Nombre, Descripcion, PrecioVenta, DiasGarantia, EstadoId)
    VALUES (@CategoriaId, @CodigoOEM, @Nombre, @Descripcion, @PrecioVenta, @DiasGarantia, @EstadoId)
END
GO

-- Inventario
CREATE TABLE Inventario
(
    InventarioId INT PRIMARY KEY IDENTITY,
    ProductoId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Producto(ProductoId),
    StockActual INT NOT NULL CHECK (StockActual >= 0),
    StockMinimo INT NOT NULL CHECK (StockMinimo >= 0),
    StockMaximo INT NOT NULL CHECK (StockMaximo >= 0),
    EstadoId INT NOT NULL FOREIGN KEY REFERENCES Estado(EstadoId)
)
GO

CREATE PROC spInventarioSelectAll
AS
BEGIN
    SELECT * FROM Inventario
END
GO

CREATE PROC spInventarioInsert
(
    @ProductoId INT,
    @StockActual INT,
    @StockMinimo INT,
    @StockMaximo INT,
    @EstadoId INT
)
AS
BEGIN
    INSERT INTO Inventario(ProductoId, StockActual, StockMinimo, StockMaximo, EstadoId)
    VALUES (@ProductoId, @StockActual, @StockMinimo, @StockMaximo, @EstadoId)
END
GO

-- CompatibilidadRepuesto
CREATE TABLE CompatibilidadRepuesto
(
    CompatibilidadId INT PRIMARY KEY IDENTITY,
    ProductoId INT NOT NULL FOREIGN KEY REFERENCES Producto(ProductoId),
    VehiculoId INT NULL FOREIGN KEY REFERENCES Vehiculo(VehiculoId),
    UNIQUE (ProductoId, VehiculoId)
)
GO

CREATE PROC spCompatibilidadRepuestoSelectAll
AS
BEGIN
    SELECT * FROM CompatibilidadRepuesto
END
GO

CREATE PROC spCompatibilidadRepuestoInsert
(
    @ProductoId INT,
    @VehiculoId INT
)
AS
BEGIN
    INSERT INTO CompatibilidadRepuesto(ProductoId, VehiculoId)
    VALUES (@ProductoId, @VehiculoId)
END
GO

-- Compra
CREATE TABLE Compra
(
    CompraId INT PRIMARY KEY IDENTITY,
    ProveedorId INT NOT NULL FOREIGN KEY REFERENCES Proveedor(ProveedorId),
    EmpleadoId INT NOT NULL FOREIGN KEY REFERENCES Empleado(EmpleadoId),
    NumeroComprobante VARCHAR(50) NOT NULL,
    FechaCompra DATETIME NOT NULL,
    TotalCompra DECIMAL(18,2) NOT NULL CHECK (TotalCompra >= 0)
)
GO

CREATE PROC spCompraSelectAll
AS
BEGIN
    SELECT * FROM Compra
END
GO

CREATE PROC spCompraInsert
(
    @ProveedorId INT,
    @EmpleadoId INT,
    @NumeroComprobante VARCHAR(50),
    @FechaCompra DATETIME,
    @TotalCompra DECIMAL(18,2)
)
AS
BEGIN
    INSERT INTO Compra(ProveedorId, EmpleadoId, NumeroComprobante, FechaCompra, TotalCompra)
    VALUES (@ProveedorId, @EmpleadoId, @NumeroComprobante, @FechaCompra, @TotalCompra)
END
GO

-- Detalle_Compra
CREATE TABLE Detalle_Compra
(
    DetalleCompraId INT PRIMARY KEY IDENTITY,
    CompraId INT NOT NULL FOREIGN KEY REFERENCES Compra(CompraId),
    ProductoId INT NOT NULL FOREIGN KEY REFERENCES Producto(ProductoId),
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    PrecioCosto DECIMAL(18,2) NOT NULL CHECK (PrecioCosto >= 0),
    Subtotal DECIMAL(18,2) NOT NULL CHECK (Subtotal >= 0)
)
GO

CREATE PROC spDetalleCompraSelectAll
AS
BEGIN
    SELECT * FROM Detalle_Compra
END
GO

CREATE PROC spDetalleCompraInsert
(
    @CompraId INT,
    @ProductoId INT,
    @Cantidad INT,
    @PrecioCosto DECIMAL(18,2),
    @Subtotal DECIMAL(18,2)
)
AS
BEGIN
    INSERT INTO Detalle_Compra(CompraId, ProductoId, Cantidad, PrecioCosto, Subtotal)
    VALUES (@CompraId, @ProductoId, @Cantidad, @PrecioCosto, @Subtotal)
END
GO

-- Venta
CREATE TABLE Venta
(
    VentaId INT PRIMARY KEY IDENTITY,
    ClienteId INT NOT NULL FOREIGN KEY REFERENCES Cliente(ClienteId),
    EmpleadoId INT NOT NULL FOREIGN KEY REFERENCES Empleado(EmpleadoId),
    NumeroFactura VARCHAR(50) NOT NULL,
    FechaVenta DATETIME NOT NULL,
    Total DECIMAL(18,2) NOT NULL CHECK (Total >= 0),
    EstadoId INT NOT NULL FOREIGN KEY REFERENCES Estado(EstadoId)
)
GO

CREATE PROC spVentaSelectAll
AS
BEGIN
    SELECT * FROM Venta
END
GO

CREATE PROC spVentaInsert
(
    @ClienteId INT,
    @EmpleadoId INT,
    @NumeroFactura VARCHAR(50),
    @FechaVenta DATETIME,
    @Total DECIMAL(18,2),
    @EstadoId INT
)
AS
BEGIN
    INSERT INTO Venta(ClienteId, EmpleadoId, NumeroFactura, FechaVenta, Total, EstadoId)
    VALUES (@ClienteId, @EmpleadoId, @NumeroFactura, @FechaVenta, @Total, @EstadoId)
END
GO

-- Detalle_Venta
CREATE TABLE Detalle_Venta
(
    DetalleVentaId INT PRIMARY KEY IDENTITY,
    VentaId INT NOT NULL FOREIGN KEY REFERENCES Venta(VentaId),
    ProductoId INT NOT NULL FOREIGN KEY REFERENCES Producto(ProductoId),
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    PrecioUnitario DECIMAL(18,2) NOT NULL CHECK (PrecioUnitario >= 0),
    Descuento DECIMAL(18,2) NULL DEFAULT 0 CHECK (Descuento >= 0),
    Subtotal DECIMAL(18,2) NOT NULL CHECK (Subtotal >= 0),
    DiasGarantiaVenta INT NULL DEFAULT 0 CHECK (DiasGarantiaVenta >= 0),
    CHECK (Descuento <= Cantidad * PrecioUnitario)
)
GO

CREATE PROC spDetalleVentaSelectAll
AS
BEGIN
    SELECT * FROM Detalle_Venta
END
GO

CREATE PROC spDetalleVentaInsert
(
    @VentaId INT,
    @ProductoId INT,
    @Cantidad INT,
    @PrecioUnitario DECIMAL(18,2),
    @Descuento DECIMAL(18,2),
    @Subtotal DECIMAL(18,2),
    @DiasGarantiaVenta INT
)
AS
BEGIN
    INSERT INTO Detalle_Venta(VentaId, ProductoId, Cantidad, PrecioUnitario, Descuento, Subtotal, DiasGarantiaVenta)
    VALUES (@VentaId, @ProductoId, @Cantidad, @PrecioUnitario, @Descuento, @Subtotal, @DiasGarantiaVenta)
END
GO

-- Garantia
CREATE TABLE Garantia
(
    GarantiaId INT PRIMARY KEY IDENTITY,
    DetalleVentaId INT NOT NULL FOREIGN KEY REFERENCES Detalle_Venta(DetalleVentaId),
    EmpleadoId INT NOT NULL FOREIGN KEY REFERENCES Empleado(EmpleadoId),
    FechaSolicitud DATETIME NOT NULL,
    CantidadReclamada INT NOT NULL CHECK (CantidadReclamada > 0),
    DescripcionFalla VARCHAR(1000) NOT NULL,
    ResultadoEvaluacion VARCHAR(500) NULL,
    FechaResolucion DATETIME NULL,
    Observaciones VARCHAR(1000) NULL,
    EstadoId INT NOT NULL FOREIGN KEY REFERENCES Estado(EstadoId)
)
GO

CREATE PROC spGarantiaSelectAll
AS
BEGIN
    SELECT * FROM Garantia
END
GO

CREATE PROC spGarantiaInsert
(
    @DetalleVentaId INT,
    @EmpleadoId INT,
    @FechaSolicitud DATETIME,
    @CantidadReclamada INT,
    @DescripcionFalla VARCHAR(1000),
    @ResultadoEvaluacion VARCHAR(500),
    @FechaResolucion DATETIME,
    @Observaciones VARCHAR(1000),
    @EstadoId INT
)
AS
BEGIN
    INSERT INTO Garantia(DetalleVentaId, EmpleadoId, FechaSolicitud, CantidadReclamada, DescripcionFalla, ResultadoEvaluacion, FechaResolucion, Observaciones, EstadoId)
    VALUES (@DetalleVentaId, @EmpleadoId, @FechaSolicitud, @CantidadReclamada, @DescripcionFalla, @ResultadoEvaluacion, @FechaResolucion, @Observaciones, @EstadoId)
END
GO

-- ReemplazoGarantia
CREATE TABLE ReemplazoGarantia
(
    ReemplazoId INT PRIMARY KEY IDENTITY,
    GarantiaId INT NOT NULL UNIQUE FOREIGN KEY REFERENCES Garantia(GarantiaId),
    EmpleadoId INT NOT NULL FOREIGN KEY REFERENCES Empleado(EmpleadoId),
    FechaReemplazo DATETIME NOT NULL,
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    Observaciones VARCHAR(1000) NULL
)
GO

CREATE PROC spReemplazoGarantiaSelectAll
AS
BEGIN
    SELECT * FROM ReemplazoGarantia
END
GO

CREATE PROC spReemplazoGarantiaInsert
(
    @GarantiaId INT,
    @EmpleadoId INT,
    @FechaReemplazo DATETIME,
    @Cantidad INT,
    @Observaciones VARCHAR(1000)
)
AS
BEGIN
    INSERT INTO ReemplazoGarantia(GarantiaId, EmpleadoId, FechaReemplazo, Cantidad, Observaciones)
    VALUES (@GarantiaId, @EmpleadoId, @FechaReemplazo, @Cantidad, @Observaciones)
END
GO
