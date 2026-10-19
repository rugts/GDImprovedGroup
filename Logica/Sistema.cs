using System;
using System.IO;
using System.Globalization;
using GDImproved.Modelos;
using Microsoft.Maui.Storage;

namespace GDImproved.Logica
{

    public class Sistema
    {
        public Plato[] Platos = new Plato[100]; public int TotalPlatos;
        public Insumo[] Insumos = new Insumo[100]; public int TotalInsumos;
        public Pedido[] Pedidos = new Pedido[500]; public int TotalPedidos;

        public int[,] ReservaIngredientes = new int[500, 101];
        public Usuario[] Usuarios = new Usuario[30]; public int TotalUsuarios;
        public int[,] Recetas = new int[101, 101];
        public string[] Boletas = new string[500]; public int TotalBoletas;
        Random azar = new Random();
        public string Carpeta = Path.Combine(FileSystem.Current.AppDataDirectory, "Datos");

        public string CarpetaBackup = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GOOD_MOOV_Backup");
        public bool ModoPractica = false;
        public string ErrorCarga = "";

        public Sistema(bool practica = false)
        {
            ModoPractica = practica;
            if (practica) { InicializarUsuarios(); return; }
            Directory.CreateDirectory(Carpeta);
            InicializarUsuarios();
            try { CargarTodo(); } catch (Exception e) { ErrorCarga = "No se pudo cargar: " + e.Message; }
        }
        public void InicializarUsuarios()
        {
            Usuarios[0] = new Usuario { Nombre = "admin", Clave = "1989", Rol = "ADMIN" };
            Usuarios[1] = new Usuario { Nombre = "cocina", Clave = "123", Rol = "COCINA" };
            Usuarios[2] = new Usuario { Nombre = "mozo", Clave = "123", Rol = "MOZO" };
            Usuarios[3] = new Usuario { Nombre = "caja", Clave = "123", Rol = "CAJA" };
            TotalUsuarios = 4;
        }
        bool AsegurarUsuarioDePrueba(string nombre, string clave, string rol)
        {
            bool encontrado = false;
            bool cambiado = false;
            for (int i = 0; i < TotalUsuarios; i++)
            {
                if (Usuarios[i].Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase))
                {
                    encontrado = true;
                    if (Usuarios[i].Clave != clave || Usuarios[i].Rol != rol) cambiado = true;
                    Usuarios[i].Clave = clave;
                    Usuarios[i].Rol = rol;
                }
            }
            if (encontrado) return cambiado;
            if (TotalUsuarios >= Usuarios.Length) return false;
            Usuarios[TotalUsuarios] = new Usuario { Nombre = nombre, Clave = clave, Rol = rol };
            TotalUsuarios++;
            return true;
        }
        void GuardarUsuarios()
        {
            if (ModoPractica) return;
            string texto = "";
            for (int i = 0; i < TotalUsuarios; i++)
            {
                Usuario usuario = Usuarios[i];
                texto += usuario.Nombre + "|" + usuario.Clave + "|" + usuario.Rol + "\n";
            }
            File.WriteAllText(Ruta("usuarios.txt"), texto);
        }
        public string NuevoUsuario(string nombre, string clave, string rol)
        {
            rol = rol.ToUpperInvariant();
            if (!TextoValido(nombre) || !TextoValido(clave))
                return "Introduce un usuario y una contraseña válidos, sin separadores ni saltos de línea.";
            if (rol != "ADMIN" && rol != "MOZO" && rol != "COCINA" && rol != "CAJA") return "Rol incorrecto.";
            for (int i = 0; i < TotalUsuarios; i++) if (Usuarios[i].Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)) return "Ese usuario ya existe.";
            if (TotalUsuarios >= Usuarios.Length) return "Límite de usuarios alcanzado.";
            Usuarios[TotalUsuarios++] = new Usuario { Nombre = nombre, Clave = clave, Rol = rol };
            GuardarTodo(); return "Usuario registrado.";
        }
        public string ObtenerRol(string usuario, string clave)
        {
            for (int i = 0; i < TotalUsuarios; i++)
                if (Usuarios[i].Nombre.Equals(usuario, StringComparison.OrdinalIgnoreCase) && Usuarios[i].Clave == clave)
                    return Usuarios[i].Rol.ToUpperInvariant();
            return "";
        }
        public string GuardarPlatoConReceta(string nombre, string categoria, decimal precio, int[] cantidades)
        {
            if (cantidades == null) return "Debes indicar los ingredientes.";
            bool alguno = false;
            for (int i = 0; i < TotalInsumos; i++)
                if (cantidades[Insumos[i].Id] > 0) alguno = true;
            if (!alguno) return "Agrega al menos un ingrediente antes de guardar.";
            string resultado = AgregarPlato(nombre, categoria, precio);
            if (!resultado.StartsWith("Plato registrado")) return resultado;
            int id = Platos[TotalPlatos - 1].Id;
            for (int i = 0; i < TotalInsumos; i++)
                Recetas[id, Insumos[i].Id] = cantidades[Insumos[i].Id];
            GuardarTodo();
            return "Plato y receta guardados correctamente.";
        }
        public string ActualizarReceta(int idPlato, int[] cantidades)
        {
            if (BuscarPlato(idPlato) == null) return "Selecciona un plato válido.";
            bool alguno = false;
            for (int i = 0; i < TotalInsumos; i++)
                if (cantidades[Insumos[i].Id] > 0) alguno = true;
            if (!alguno) return "Agrega al menos un ingrediente.";
            for (int i = 0; i < TotalInsumos; i++)
                Recetas[idPlato, Insumos[i].Id] = cantidades[Insumos[i].Id];
            GuardarTodo(); return "Receta actualizada correctamente.";
        }
        public Plato BuscarPlato(int id)
        {
            for (int i = 0; i < TotalPlatos; i++) if (Platos[i].Id == id) return Platos[i];
            return null;
        }
        public Insumo BuscarInsumo(int id)
        {
            for (int i = 0; i < TotalInsumos; i++) if (Insumos[i].Id == id) return Insumos[i];
            return null;
        }
        public Pedido BuscarPedido(int id)
        {
            for (int i = 0; i < TotalPedidos; i++) if (Pedidos[i].Id == id) return Pedidos[i];
            return null;
        }
        public int NuevoIdPlato() { for (int id = 1; id <= 100; id++) { bool usado = false; for (int i = 0; i < TotalPlatos; i++) if (Platos[i].Id == id) usado = true; for (int i = 0; i < TotalPedidos; i++) if (Pedidos[i].PlatoId == id) usado = true; if (!usado) return id; } return 101; }
        public int NuevoIdInsumo() { int m = 0; for (int i = 0; i < TotalInsumos; i++) if (Insumos[i].Id > m) m = Insumos[i].Id; return m + 1; }
        public int NuevoIdPedido() { int m = 0; for (int i = 0; i < TotalPedidos; i++) if (Pedidos[i].Id > m) m = Pedidos[i].Id; return m + 1; }
        public string AgregarPlato(string nombre, string categoria, decimal precio)
        {
            if (!TextoValido(nombre) || !TextoValido(categoria) || precio <= 0) return "Comprueba nombre, categoría y precio mayor que cero.";
            for (int i = 0; i < TotalPlatos; i++) if (Platos[i].Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase) && Platos[i].Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase)) return "El plato ya existe en esa categoría.";
            if (TotalPlatos >= Platos.Length) return "Carta llena.";
            int id = NuevoIdPlato(); if (id > 100) return "Máximo 100 identificadores de platos.";
            Platos[TotalPlatos++] = new Plato { Id = id, Nombre = nombre, Categoria = categoria, Precio = precio };
            GuardarTodo(); return "Plato registrado con ID " + id;
        }
        public string AgregarInsumo(string nombre, int cantidad, int alerta)
        {
            if (!TextoValido(nombre) || cantidad < 0 || alerta < 0) return "Revisa nombre, stock y alerta.";
            for (int i = 0; i < TotalInsumos; i++) if (Insumos[i].Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)) return "Ese insumo ya existe: utiliza Reponer.";
            if (TotalInsumos >= Insumos.Length) return "Inventario lleno.";
            int id = NuevoIdInsumo(); if (id > 100) return "Máximo 100 identificadores de insumos.";
            Insumos[TotalInsumos++] = new Insumo { Id = id, Nombre = nombre, Stock = cantidad, Alerta = alerta };
            GuardarTodo(); return "Insumo registrado con ID " + id;
        }

        public string EditarPlato(int id, string nombre, string categoria, decimal precio)
        {
            Plato p = BuscarPlato(id);
            if (p == null) return "Selecciona un plato.";
            if (!TextoValido(nombre) || !TextoValido(categoria) || precio <= 0) return "Revisa nombre, categoría y precio.";
            for (int i = 0; i < TotalPlatos; i++)
                if (Platos[i].Id != id && Platos[i].Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase) && Platos[i].Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase))
                    return "Ya existe un plato con ese nombre en esa categoría.";
            p.Nombre = nombre; p.Categoria = categoria; p.Precio = precio;
            GuardarTodo(); return "Plato modificado correctamente.";
        }
        public string EditarInsumo(int id, string nombre, int stock, int alerta)
        {
            Insumo x = BuscarInsumo(id);
            if (x == null) return "Selecciona un insumo.";
            if (!TextoValido(nombre) || stock < 0 || alerta < 0) return "Revisa el nombre, stock y alerta.";
            for (int i = 0; i < TotalInsumos; i++)
                if (Insumos[i].Id != id && Insumos[i].Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase))
                    return "Ya existe otro insumo con ese nombre.";
            x.Nombre = nombre; x.Stock = stock; x.Alerta = alerta;
            GuardarTodo(); return "Insumo modificado correctamente.";
        }
        public string EliminarPlato(int id)
        {
            Plato p = BuscarPlato(id);
            if (p == null) return "Plato no encontrado.";
            for (int i = 0; i < TotalPlatos; i++)
                if (Platos[i].Id == id)
                {
                    for (int j = i; j < TotalPlatos - 1; j++) Platos[j] = Platos[j + 1];
                    Platos[--TotalPlatos] = null;
                    for (int k = 1; k <= 100; k++) Recetas[id, k] = 0;
                    GuardarTodo(); return "Plato retirado de la carta. Pedidos anteriores conservados.";
                }
            return "Plato no encontrado.";
        }
        public string EliminarInsumo(int id)
        {
            Insumo x = BuscarInsumo(id);
            if (x == null) return "Insumo no encontrado.";
            for (int i = 0; i < TotalPlatos; i++)
                if (Recetas[Platos[i].Id, id] > 0)
                    return "No se puede eliminar: una receta usa este insumo. Edita primero esa receta.";
            for (int i = 0; i < TotalInsumos; i++)
                if (Insumos[i].Id == id)
                {
                    for (int j = i; j < TotalInsumos - 1; j++) Insumos[j] = Insumos[j + 1];
                    Insumos[--TotalInsumos] = null;
                    GuardarTodo(); return "Insumo eliminado correctamente.";
                }
            return "Insumo no encontrado.";
        }

        int OrdenCategoria(string categoria)
        {
            string nombre = categoria.Trim().ToLowerInvariant();
            if (nombre == "entrada" || nombre == "entradas") return 0;
            if (nombre == "plato fuerte" || nombre == "platos fuertes" || nombre == "fondo" || nombre == "fondos") return 1;
            if (nombre == "postre" || nombre == "postres") return 2;
            if (nombre == "bebida" || nombre == "bebidas") return 3;
            return 4;
        }
        public string CartaOrdenada()
        {
            if (TotalPlatos == 0) return "Carta vacía.";
            Plato[] ordenados = new Plato[TotalPlatos];
            for (int i = 0; i < TotalPlatos; i++) ordenados[i] = Platos[i];

            for (int i = 0; i < TotalPlatos - 1; i++)
            {
                for (int j = 0; j < TotalPlatos - i - 1; j++)
                {
                    Plato primero = ordenados[j];
                    Plato segundo = ordenados[j + 1];
                    int categoria1 = OrdenCategoria(primero.Categoria);
                    int categoria2 = OrdenCategoria(segundo.Categoria);
                    bool intercambiar = categoria1 > categoria2;
                    if (categoria1 == categoria2 && primero.Precio > segundo.Precio)
                        intercambiar = true;
                    if (intercambiar)
                    {
                        ordenados[j] = segundo;
                        ordenados[j + 1] = primero;
                    }
                }
            }

            string texto = "";
            int grupoAnterior = -1;
            for (int i = 0; i < TotalPlatos; i++)
            {
                Plato plato = ordenados[i];
                int grupo = OrdenCategoria(plato.Categoria);
                if (grupo != grupoAnterior)
                {
                    grupoAnterior = grupo;
                    string titulo = "OTROS";
                    if (grupo == 0) titulo = "ENTRADAS";
                    else if (grupo == 1) titulo = "PLATOS FUERTES";
                    else if (grupo == 2) titulo = "POSTRES";
                    else if (grupo == 3) titulo = "BEBIDAS";
                    texto += "\n" + titulo + "\n";
                }
                texto += "- " + plato.Nombre + " | S/ " + plato.Precio.ToString("0.00", CultureInfo.InvariantCulture)
                    + " | Disponibles: " + Disponibles(plato.Id);
                if (grupo == 4) texto += " | " + plato.Categoria;
                texto += "\n";
            }
            return texto;
        }

        public string NumeroComprobante(int indice)
        {
            if (indice < 1 || indice > TotalBoletas) return "";
            string encabezado = Boletas[indice - 1].Split('|')[0].Trim();
            int espacio = encabezado.LastIndexOf(' ');
            return espacio < 0 ? indice.ToString("D5") : encabezado.Substring(espacio + 1);
        }
        string GenerarNumeroComprobante()
        {
            string numero;
            bool existe;
            do
            {
                numero = azar.Next(1, 10).ToString();
                for (int d = 1; d < 12; d++) numero += azar.Next(0, 10).ToString();
                existe = false;
                for (int i = 0; i < TotalBoletas; i++)
                {
                    string encabezado = Boletas[i].Split('|')[0].Trim();
                    if (encabezado.EndsWith(" " + numero, StringComparison.Ordinal)) existe = true;
                }
            } while (existe);
            return numero;
        }
        public string RutaComprobante(int indice)
        {
            string referencia = NumeroComprobante(indice);

            if (referencia.Length != 12) referencia = indice.ToString("D5");
            return Path.Combine(Carpeta, "Comprobantes", "comprobante_" + referencia + ".pdf");
        }
        public string GenerarPdfBoleta(int numero)
        {
            if (ModoPractica) return "No se generan PDF en modo aprendizaje.";
            if (numero < 1 || numero > TotalBoletas) return "Selecciona una boleta válida.";
            string[] c = Boletas[numero - 1].Split('|');
            if (c.Length < 4) return "Registro de boleta no compatible.";
            int mesa; if (!int.TryParse(c[1].Trim().Replace("Mesa ", ""), out mesa)) return "No se reconoce la mesa.";
            string[] lineas = new string[550]; int n = 0;
            lineas[n++] = "GOOD MOOV - COMPROBANTE INTERNO";
            lineas[n++] = "Simulacion academica - sin validez tributaria";
            lineas[n++] = c[0].Trim();
            if (c.Length >= 7) { lineas[n++] = "Documento: " + c[4].Trim() + " " + c[5].Trim(); lineas[n++] = "Medio de pago: " + c[6].Trim(); }
            lineas[n++] = "Referencia interna: " + NumeroComprobante(numero);
            lineas[n++] = "Mesa: " + mesa;
            lineas[n++] = c[3].Trim();
            lineas[n++] = "---------------------------------------------";
            string detalle = Ruta("detalle_boleta_" + numero + ".txt");
            if (File.Exists(detalle))
            {
                string[] items = File.ReadAllLines(detalle);
                for (int i = 0; i < items.Length && n < lineas.Length - 5; i++) lineas[n++] = items[i];
            }
            else
            {
                lineas[n++] = "Detalle no disponible para esta boleta antigua.";
            }
            lineas[n++] = "---------------------------------------------";
            lineas[n++] = c[2].Trim();
            lineas[n++] = "Gracias por su visita.";
            Directory.CreateDirectory(Path.Combine(Carpeta, "Comprobantes"));
            string ruta = RutaComprobante(numero);
            PdfBasico.Crear(ruta, lineas, n);
            return ruta;
        }
        public string AjustarStock(int id, int cantidad)
        {
            Insumo x = BuscarInsumo(id); if (x == null) return "Insumo inexistente.";
            if ((long)x.Stock + cantidad < 0 || (long)x.Stock + cantidad > int.MaxValue) return "Cantidad fuera del stock permitido.";
            x.Stock += cantidad; GuardarTodo(); return "Stock actualizado: " + x.Stock;
        }
        public int Disponibles(int platoId)
        {
            if (BuscarPlato(platoId) == null) return 0;
            int maximo = 9999; bool usa = false;
            for (int i = 0; i < TotalInsumos; i++)
            {
                Insumo ins = Insumos[i]; int necesita = Recetas[platoId, ins.Id];
                if (necesita > 0) { usa = true; int posibles = ins.Stock / necesita; if (posibles < maximo) maximo = posibles; }
            }

            return usa ? maximo : 0;
        }
        public string CrearPedidoCompleto(int mesa, int[] platos, int[] cantidades, int total, string observacion)
        {
            if (mesa <= 0 || total <= 0 || total > 100 || !TextoValido(observacion, true))
                return "Revisa el número de mesa y el pedido.";
            if (TotalPedidos + total > Pedidos.Length) return "No hay espacio para más pedidos.";
            int[] consumo = new int[101];
            string platosSinReceta = "";

            for (int j = 0; j < total; j++)
            {
                Plato p = BuscarPlato(platos[j]);
                if (p == null || cantidades[j] <= 0)
                    return "Hay un plato o cantidad incorrectos.";

                bool tieneReceta = false;
                for (int i = 0; i < TotalInsumos; i++)
                {
                    int id = Insumos[i].Id;
                    int porPlato = Recetas[p.Id, id];
                    if (porPlato > 0) tieneReceta = true;

                    long cantidadNueva = (long)consumo[id] + (long)porPlato * cantidades[j];
                    if (cantidadNueva > int.MaxValue) return "Cantidad demasiado grande.";
                    consumo[id] = (int)cantidadNueva;
                }

                if (!tieneReceta)
                    platosSinReceta += "- " + p.Nombre + "\n";
            }

            string insumosFaltantes = "";
            for (int i = 0; i < TotalInsumos; i++)
            {
                Insumo ins = Insumos[i];
                if (consumo[ins.Id] > ins.Stock)
                {
                    int faltan = consumo[ins.Id] - ins.Stock;
                    insumosFaltantes += "- " + ins.Nombre + ": necesita "
                        + consumo[ins.Id] + ", disponible " + ins.Stock
                        + ", faltan " + faltan + ".\n";
                }
            }

            if (platosSinReceta != "" || insumosFaltantes != "")
            {
                string aviso = "No se puede procesar el pedido completo.\n\n";
                if (insumosFaltantes != "")
                    aviso += "INSUMOS INSUFICIENTES:\n" + insumosFaltantes + "\n";
                if (platosSinReceta != "")
                    aviso += "PLATOS SIN RECETA:\n" + platosSinReceta + "\n";
                aviso += "No se registró el pedido ni se descontaron ingredientes.";
                return aviso;
            }

            for (int i = 0; i < TotalInsumos; i++) Insumos[i].Stock -= consumo[Insumos[i].Id];
            int primerId = NuevoIdPedido();
            int grupoId = primerId;
            string fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            for (int j = 0; j < total; j++)
            {
                Plato p = BuscarPlato(platos[j]);
                int indice = TotalPedidos;
                Pedidos[TotalPedidos++] = new Pedido
                {
                    Id = primerId + j, GrupoId = grupoId, Mesa = mesa,
                    PlatoId = p.Id, PlatoNombre = p.Nombre, Cantidad = cantidades[j],
                    Precio = p.Precio, Observacion = observacion, Fecha = fecha
                };
                for (int i = 0; i < TotalInsumos; i++)
                {
                    int insumoId = Insumos[i].Id;
                    ReservaIngredientes[indice, insumoId] = Recetas[p.Id, insumoId] * cantidades[j];
                }
            }
            GuardarTodo();
            return "Pedido #" + grupoId + " registrado con " + total + " platos diferentes.";
        }

        public string EstadoPedido(int id, string estado)
        {
            Pedido p = BuscarPedido(id);
            if (p == null) return "Pedido inexistente.";
            if (p.Estado == "Cobrado" || p.Estado == "Cancelado") return "Este pedido ya está cerrado.";
            if (estado != "Pendiente" && estado != "En preparación" && estado != "Listo" &&
                estado != "Entregado" && estado != "Cancelado") return "Estado no permitido.";

            if (p.Estado == "Entregado") return "El plato ya fue entregado. No se puede cancelar desde Cocina.";

            if (estado != "Cancelado")
            {
                bool permitido =
                    (p.Estado == "Pendiente" && estado == "En preparación") ||
                    (p.Estado == "En preparación" && estado == "Listo") ||
                    (p.Estado == "Listo" && estado == "Entregado");
                if (!permitido) return "Sigue el orden: Pendiente, En preparación, Listo y Entregado.";
            }

            if (estado == "Cancelado")
            {

                if (p.Estado == "Pendiente")
                {
                    int indice = -1;
                    for (int i = 0; i < TotalPedidos; i++)
                        if (Pedidos[i].Id == id) indice = i;
                    bool hayReserva = false;
                    if (indice >= 0)
                    {
                        for (int i = 0; i < TotalInsumos; i++)
                            if (ReservaIngredientes[indice, Insumos[i].Id] > 0) hayReserva = true;
                    }
                    if (hayReserva)
                    {
                        for (int i = 0; i < TotalInsumos; i++)
                        {
                            int insumoId = Insumos[i].Id;
                            long nuevoStock = (long)Insumos[i].Stock + ReservaIngredientes[indice, insumoId];
                            if (nuevoStock > int.MaxValue)
                                return "No se puede cancelar: revisa el stock del insumo " + Insumos[i].Nombre + ".";
                        }
                        for (int i = 0; i < TotalInsumos; i++)
                        {
                            int insumoId = Insumos[i].Id;
                            Insumos[i].Stock += ReservaIngredientes[indice, insumoId];
                        }
                    }
                    p.Estado = "Cancelado";
                    GuardarTodo();
                    if (!hayReserva)
                        return "Pedido cancelado. Es un pedido antiguo sin registro de ingredientes: revisa el inventario manualmente.";
                    return "Pedido cancelado. Los ingredientes reservados se devolvieron al inventario.";
                }
                p.Estado = "Cancelado";
                GuardarTodo();
                return "Pedido cancelado. No se repuso stock porque la preparación ya había comenzado.";
            }

            p.Estado = estado;
            GuardarTodo();
            return "Estado actualizado.";
        }

        bool SoloDigitos(string valor)
        {
            for (int i = 0; i < valor.Length; i++) if (valor[i] < '0' || valor[i] > '9') return false;
            return true;
        }
        public string CobrarMesa(int mesa)
        {
            return CobrarMesa(mesa, "Boleta simple", "DNI", "", "Efectivo");
        }
        public string CobrarMesa(int mesa, string tipo, string documentoTipo, string numeroDocumento, string medioPago)
        {
            numeroDocumento = numeroDocumento.Trim();
            if (mesa <= 0) return "La mesa debe ser mayor que cero.";
            if (numeroDocumento.Contains("|") || numeroDocumento.Contains("\n")) return "Documento no válido.";
            if (numeroDocumento.Length > 20) return "Documento demasiado largo.";

            if (tipo != "Factura" && (documentoTipo == "RUC" ||
                (numeroDocumento.Length == 11 && SoloDigitos(numeroDocumento))))
                return "Para usar RUC selecciona Factura. Si deseas boleta, cambia el tipo y número de documento.";
            if (tipo == "Factura" && documentoTipo != "RUC") return "Para factura selecciona RUC.";
            if (tipo == "Factura" && (numeroDocumento.Length != 11 || !SoloDigitos(numeroDocumento))) return "La factura requiere RUC de 11 dígitos.";
            if (documentoTipo == "Otro" && tipo == "Factura") return "Otro documento no permite factura.";
            if (tipo != "Boleta simple" && tipo != "Boleta electrónica" && tipo != "Factura") return "Selecciona un comprobante.";
            if (medioPago != "Efectivo" && medioPago != "Tarjeta" && medioPago != "Yape") return "Selecciona un medio de pago.";
            if (numeroDocumento != "")
            {
                if (documentoTipo == "DNI" && (numeroDocumento.Length != 8 || !SoloDigitos(numeroDocumento))) return "El DNI debe tener 8 dígitos.";
                if (documentoTipo == "RUC" && (numeroDocumento.Length != 11 || !SoloDigitos(numeroDocumento))) return "El RUC debe tener 11 dígitos.";
                if ((documentoTipo == "CE" || documentoTipo == "Pasaporte") && (numeroDocumento.Length < 6 || numeroDocumento.Length > 20)) return "Revisa el número de documento.";
            }
            else if (tipo != "Boleta simple") return "Ingresa el documento para esta simulación de comprobante.";

            string pendientes = "";
            for (int i = 0; i < TotalPedidos; i++)
            {
                Pedido p = Pedidos[i];
                if (p.Mesa == mesa && p.Estado != "Cobrado" &&
                    p.Estado != "Cancelado" && p.Estado != "Entregado")
                    pendientes += "- " + p.PlatoNombre + " x" + p.Cantidad + " (" + p.Estado + ")\n";
            }
            if (pendientes != "")
                return "No se puede cobrar la mesa " + mesa + ".\n\nPlatos que faltan entregar:\n" + pendientes + "\nEspera a que Cocina termine todos los platos activos.";

            decimal total = 0; int cantidad = 0;
            for (int i = 0; i < TotalPedidos; i++)
            {
                Pedido p = Pedidos[i];
                if (p.Mesa == mesa && p.Estado == "Entregado") { total += p.Total(); cantidad++; }
            }
            if (cantidad == 0) return "No hay pedidos entregados pendientes de cobro para esta mesa.";
            if (TotalBoletas >= Boletas.Length) return "Límite de boletas alcanzado.";
            string comprobante = tipo.ToUpperInvariant() + " " + GenerarNumeroComprobante() + " | Mesa " + mesa + " | S/ " + total.ToString("0.00", CultureInfo.InvariantCulture) + " | " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + documentoTipo + " | " + numeroDocumento + " | " + medioPago;
            Boletas[TotalBoletas++] = comprobante;
            if (!ModoPractica)
            {
                string detalle = "";
                for (int i = 0; i < TotalPedidos; i++)
                {
                    Pedido p = Pedidos[i];
                    if (p.Mesa == mesa && p.Estado == "Entregado")
                        detalle += p.PlatoNombre + " x" + p.Cantidad + "   S/ " + p.Total().ToString("0.00", CultureInfo.InvariantCulture) + "\n";
                }
                File.WriteAllText(Ruta("detalle_boleta_" + TotalBoletas + ".txt"), detalle);
            }
            for (int i = 0; i < TotalPedidos; i++) if (Pedidos[i].Mesa == mesa && Pedidos[i].Estado == "Entregado") Pedidos[i].Estado = "Cobrado";
            GuardarTodo(); return comprobante;
        }
        public string Alertas()
        {
            string t = "";
            for (int i = 0; i < TotalInsumos; i++)
            {
                Insumo x = Insumos[i];
                if (x.Stock == 0) t += "Agotado: " + x.Nombre + "\n";
                else if (x.Stock <= x.Alerta) t += "Stock bajo: " + x.Nombre + " (" + x.Stock + ")\n";
            }
            return t == "" ? "No hay alertas de stock." : t;
        }
        public static bool TextoValido(string t, bool permiteVacio = false)
        {
            if (t == null) return false;
            if (!permiteVacio && t.Trim() == "") return false;
            return !t.Contains("|") && !t.Contains("\n") && !t.Contains("\r");
        }
        string Ruta(string archivo) { return Path.Combine(Carpeta, archivo); }
        public void GuardarTodo()
        {
            if (ModoPractica) return;
            Directory.CreateDirectory(Carpeta);
            string texto = "";
            for (int i = 0; i < TotalPlatos; i++) { Plato p = Platos[i]; texto += p.Id + "|" + p.Nombre + "|" + p.Categoria + "|" + p.Precio.ToString(CultureInfo.InvariantCulture) + "\n"; }
            File.WriteAllText(Ruta("platos.txt"), texto);
            texto = ""; for (int i = 0; i < TotalInsumos; i++) { Insumo x = Insumos[i]; texto += x.Id + "|" + x.Nombre + "|" + x.Stock + "|" + x.Alerta + "\n"; }
            File.WriteAllText(Ruta("insumos.txt"), texto);
            texto = ""; for (int p = 1; p <= 100; p++) for (int x = 1; x <= 100; x++) if (Recetas[p, x] > 0) texto += p + "|" + x + "|" + Recetas[p, x] + "\n";
            File.WriteAllText(Ruta("recetas.txt"), texto);
            texto = ""; for (int i = 0; i < TotalPedidos; i++) { Pedido p = Pedidos[i]; texto += p.Id + "|" + p.Mesa + "|" + p.PlatoId + "|" + p.PlatoNombre + "|" + p.Cantidad + "|" + p.Precio.ToString(CultureInfo.InvariantCulture) + "|" + p.Observacion + "|" + p.Estado + "|" + p.Fecha + "|" + (p.GrupoId == 0 ? p.Id : p.GrupoId) + "\n"; }
            File.WriteAllText(Ruta("pedidos.txt"), texto);

            texto = "";
            for (int i = 0; i < TotalPedidos; i++)
                for (int x = 1; x <= 100; x++)
                    if (ReservaIngredientes[i, x] > 0)
                        texto += Pedidos[i].Id + "|" + x + "|" + ReservaIngredientes[i, x] + "\n";
            File.WriteAllText(Ruta("reservas_pedidos.txt"), texto);
            GuardarUsuarios();
            texto = ""; for (int i = 0; i < TotalBoletas; i++) texto += Boletas[i] + "\n";
            File.WriteAllText(Ruta("boletas.txt"), texto);
        }
        public void CargarTodo()
        {
            TotalPlatos = 0; TotalInsumos = 0; TotalPedidos = 0; TotalBoletas = 0;
            Recetas = new int[101, 101]; ReservaIngredientes = new int[500, 101]; InicializarUsuarios();
            string[] lineas; string[] c;
            if (File.Exists(Ruta("platos.txt")))
            {
                lineas = File.ReadAllLines(Ruta("platos.txt"));
                for (int i = 0; i < lineas.Length && TotalPlatos < 100; i++) { c = lineas[i].Split('|'); if (c.Length != 4) continue; Platos[TotalPlatos++] = new Plato { Id = int.Parse(c[0]), Nombre = c[1], Categoria = c[2], Precio = decimal.Parse(c[3], CultureInfo.InvariantCulture) }; }
            }
            if (File.Exists(Ruta("insumos.txt")))
            {
                lineas = File.ReadAllLines(Ruta("insumos.txt"));
                for (int i = 0; i < lineas.Length && TotalInsumos < 100; i++) { c = lineas[i].Split('|'); if (c.Length != 4) continue; Insumos[TotalInsumos++] = new Insumo { Id = int.Parse(c[0]), Nombre = c[1], Stock = int.Parse(c[2]), Alerta = int.Parse(c[3]) }; }
            }
            if (File.Exists(Ruta("recetas.txt")))
            {
                lineas = File.ReadAllLines(Ruta("recetas.txt"));
                for (int i = 0; i < lineas.Length; i++) { c = lineas[i].Split('|'); if (c.Length != 3) continue; int p = int.Parse(c[0]); int x = int.Parse(c[1]); if (p >= 1 && p <= 100 && x >= 1 && x <= 100) Recetas[p, x] = int.Parse(c[2]); }
            }
            if (File.Exists(Ruta("pedidos.txt")))
            {
                lineas = File.ReadAllLines(Ruta("pedidos.txt"));
                for (int i = 0; i < lineas.Length && TotalPedidos < 500; i++) { c = lineas[i].Split('|'); if (c.Length != 9 && c.Length != 10) continue; Pedidos[TotalPedidos++] = new Pedido { Id = int.Parse(c[0]), GrupoId = c.Length == 10 ? int.Parse(c[9]) : int.Parse(c[0]), Mesa = int.Parse(c[1]), PlatoId = int.Parse(c[2]), PlatoNombre = c[3], Cantidad = int.Parse(c[4]), Precio = decimal.Parse(c[5], CultureInfo.InvariantCulture), Observacion = c[6], Estado = c[7], Fecha = c[8] }; }
            }

            if (File.Exists(Ruta("reservas_pedidos.txt")))
            {
                lineas = File.ReadAllLines(Ruta("reservas_pedidos.txt"));
                for (int i = 0; i < lineas.Length; i++)
                {
                    c = lineas[i].Split('|');
                    if (c.Length != 3) continue;
                    int pedidoId, insumoId, cantidad;
                    if (!int.TryParse(c[0], out pedidoId) ||
                        !int.TryParse(c[1], out insumoId) ||
                        !int.TryParse(c[2], out cantidad)) continue;
                    if (insumoId < 1 || insumoId > 100 || cantidad <= 0) continue;
                    for (int p = 0; p < TotalPedidos; p++)
                        if (Pedidos[p].Id == pedidoId)
                            ReservaIngredientes[p, insumoId] = cantidad;
                }
            }
            bool usuariosVacios = false;
            if (File.Exists(Ruta("usuarios.txt")))
            {
                lineas = File.ReadAllLines(Ruta("usuarios.txt")); TotalUsuarios = 0;
                for (int i = 0; i < lineas.Length && TotalUsuarios < 30; i++) { c = lineas[i].Split('|'); if (c.Length != 3) continue; Usuarios[TotalUsuarios++] = new Usuario { Nombre = c[0], Clave = c[1], Rol = c[2] }; }
                if (TotalUsuarios == 0)
                {
                    InicializarUsuarios();
                    usuariosVacios = true;
                }
            }
            bool cuentasModificadas = usuariosVacios;
            if (AsegurarUsuarioDePrueba("admin", "1989", "ADMIN")) cuentasModificadas = true;
            if (AsegurarUsuarioDePrueba("mozo", "123", "MOZO")) cuentasModificadas = true;
            if (AsegurarUsuarioDePrueba("cocina", "123", "COCINA")) cuentasModificadas = true;
            if (AsegurarUsuarioDePrueba("caja", "123", "CAJA")) cuentasModificadas = true;
            if (cuentasModificadas || !File.Exists(Ruta("usuarios.txt"))) GuardarUsuarios();
            if (File.Exists(Ruta("boletas.txt")))
            {
                lineas = File.ReadAllLines(Ruta("boletas.txt"));
                for (int i = 0; i < lineas.Length && TotalBoletas < 500; i++) Boletas[TotalBoletas++] = lineas[i];
            }
        }
        void CopiarArchivoTexto(string origen, string destino)
        {
            string[] lineas = File.ReadAllLines(origen);
            string texto = "";
            for (int i = 0; i < lineas.Length; i++) texto += lineas[i] + "\n";
            File.WriteAllText(destino, texto);
        }
        public string CrearBackup()
        {
            if (ModoPractica) return "No se crean respaldos en el modo práctica.";
            GuardarTodo(); Directory.CreateDirectory(CarpetaBackup);
            string destino = Path.Combine(CarpetaBackup, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(destino);
            string[] archivos = Directory.GetFiles(Carpeta, "*.txt");
            for (int i = 0; i < archivos.Length; i++) CopiarArchivoTexto(archivos[i], Path.Combine(destino, Path.GetFileName(archivos[i])));
            return "Backup creado: " + destino;
        }
        public string RestaurarUltimoBackup()
        {
            if (ModoPractica) return "No hay restauración en modo práctica.";
            if (!Directory.Exists(CarpetaBackup)) return "No existe una carpeta de respaldos.";
            string[] carpetas = Directory.GetDirectories(CarpetaBackup); Array.Sort(carpetas);
            if (carpetas.Length == 0) return "No hay respaldos disponibles.";
            string ultimo = carpetas[carpetas.Length - 1];
            string[] archivos = Directory.GetFiles(ultimo, "*.txt");
            if (archivos.Length == 0) return "El respaldo no contiene archivos.";
            Directory.CreateDirectory(Carpeta);

            string archivoReservas = Path.Combine(ultimo, "reservas_pedidos.txt");
            if (!File.Exists(archivoReservas) && File.Exists(Ruta("reservas_pedidos.txt")))
                File.WriteAllText(Ruta("reservas_pedidos.txt"), "");
            for (int i = 0; i < archivos.Length; i++) CopiarArchivoTexto(archivos[i], Ruta(Path.GetFileName(archivos[i])));
            CargarTodo(); return "Restaurado desde: " + ultimo;
        }
    }
}
