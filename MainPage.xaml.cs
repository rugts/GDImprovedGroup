using System;
using System.Globalization;
using GDImproved.Logica;
using Microsoft.Maui.Controls;
using System.IO;

namespace GDImproved
{
    public partial class MainPage : ContentPage
    {
        Sistema sistema = new Sistema();
        string rolActual = "";
        int[] ingredientes = new int[101];
        int[] carritoPlatos = new int[100];
        int[] carritoCantidades = new int[100];
        int totalCarrito = 0;
        string[] admin = { "Ver carta", "Agregar plato", "Editar receta", "Editar plato", "Eliminar plato", "Ver inventario", "Agregar insumo", "Editar insumo", "Eliminar insumo", "Reponer / merma", "Crear pedido", "Ver pedidos", "Cobrar mesa", "Ver boletas", "Ver PDF / imprimir", "Crear usuario", "Ver alertas", "Crear backup", "Recuperar último backup", "Ayuda" };
        string[] cocina = { "Ver carta", "Agregar plato", "Editar receta", "Ver inventario", "Agregar insumo", "Reponer / merma", "Ver pedidos", "Ver alertas", "Ayuda" };
        string[] mozo = { "Ver carta", "Crear pedido", "Ver pedidos", "Ver alertas", "Ayuda" };
        string[] caja = { "Ver pedidos", "Cobrar mesa", "Ver boletas", "Ver PDF / imprimir", "Ayuda" };
        public MainPage()
        {
            InitializeComponent();
            TipoComprobantePicker.Items.Add("Boleta simple");
            TipoComprobantePicker.Items.Add("Boleta electrónica");
            TipoComprobantePicker.Items.Add("Factura");
            TipoDocumentoPicker.Items.Add("DNI");
            TipoDocumentoPicker.Items.Add("CE");
            TipoDocumentoPicker.Items.Add("Pasaporte");
            TipoDocumentoPicker.Items.Add("RUC");
            TipoDocumentoPicker.Items.Add("Otro");
            MedioPagoPicker.Items.Add("Efectivo");
            MedioPagoPicker.Items.Add("Tarjeta");
            MedioPagoPicker.Items.Add("Yape");
            TipoComprobantePicker.SelectedIndex = 0;
            TipoDocumentoPicker.SelectedIndex = 0;
            MedioPagoPicker.SelectedIndex = 0;
            RolNuevoPicker.Items.Add("ADMIN"); RolNuevoPicker.Items.Add("COCINA");
            RolNuevoPicker.Items.Add("MOZO"); RolNuevoPicker.Items.Add("CAJA");
            if (sistema.ErrorCarga != "") Subtitulo.Text = sistema.ErrorCarga;
        }
        string Campo(Entry entrada) { return entrada.Text == null ? "" : entrada.Text.Trim(); }
        bool Entero(Entry entrada, out int resultado) { return int.TryParse(Campo(entrada), out resultado); }
        bool Dinero(Entry entrada, out decimal resultado)
        {
            return decimal.TryParse(Campo(entrada).Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out resultado);
        }
        string accionPendiente = "";
        int pedidoPendiente = 0;
        int comprobantePendiente = 0;

        void Aviso(string mensaje)
        {
            accionPendiente = "";
            AvisoTitulo.Text = "GOOD MOOV";
            AvisoMensaje.Text = mensaje;
            AvisoAceptarBoton.IsVisible = true;
            ConfirmarSiBoton.IsVisible = false;
            ConfirmarNoBoton.IsVisible = false;
            AvisoPanel.IsVisible = true;
        }
        void Confirmar(string titulo, string mensaje, string accion)
        {
            accionPendiente = accion;
            AvisoTitulo.Text = titulo;
            AvisoMensaje.Text = mensaje;
            AvisoAceptarBoton.IsVisible = false;
            ConfirmarSiBoton.IsVisible = true;
            ConfirmarNoBoton.IsVisible = true;
            AvisoPanel.IsVisible = true;
        }
        void AvisoAceptarClicked(object sender, EventArgs e)
        {
            AvisoPanel.IsVisible = false;
        }
        void ConfirmarNoClicked(object sender, EventArgs e)
        {
            accionPendiente = "";
            AvisoPanel.IsVisible = false;
        }
        void ConfirmarSiClicked(object sender, EventArgs e)
        {
            string accion = accionPendiente;
            accionPendiente = "";
            AvisoPanel.IsVisible = false;
            if (accion == "aprendizaje")
            {
                sistema = new Sistema(true);
                AbrirMenu("ADMIN");
            }
            else if (accion == "Eliminar plato" || accion == "Eliminar insumo" || accion == "Recuperar último backup")
            {
                EjecutarOperacion(accion);
            }
            else if (accion == "cancelar plato")
            {
                try
                {
                    string respuesta = sistema.EstadoPedido(pedidoPendiente, "Cancelado");
                    ActualizarListado();
                    Aviso(respuesta);
                }
                catch (Exception)
                {
                    Aviso("No se pudo guardar la cancelación. Verifica los archivos de datos.");
                }
            }
            else if (accion == "abrir comprobante")
            {
                AbrirComprobantePdf(comprobantePendiente);
            }
        }
        void LoginClicked(object sender, EventArgs e)
        {
            string rol = sistema.ObtenerRol(Campo(UsuarioEntry), Campo(ClaveEntry));
            if (rol == "") { Aviso("Usuario o contraseña incorrectos."); return; }
            AbrirMenu(rol);
        }
        void AbrirMenu(string rol)
        {
            rolActual = rol;
            PanelLogin.IsVisible = false;
            PanelTrabajo.IsVisible = true;
            Subtitulo.Text = sistema.ModoPractica ? "APRENDIZAJE - " + rol : "Área de " + rol;
            AccionPicker.Items.Clear();
            string[] opciones = rol == "ADMIN" ? admin : rol == "COCINA" ? cocina : rol == "MOZO" ? mozo : caja;
            for (int i = 0; i < opciones.Length; i++) AccionPicker.Items.Add(opciones[i]);
            AccionPicker.SelectedIndex = 0;
        }
        void PracticaClicked(object sender, EventArgs e)
        {
            Confirmar("Modo aprendizaje", "Puedes practicar todas las funciones como administrador sin modificar los archivos reales. ¿Continuar?", "aprendizaje");
        }
        void SalirClicked(object sender, EventArgs e)
        {
            sistema = new Sistema(); rolActual = "";
            PanelLogin.IsVisible = true; PanelTrabajo.IsVisible = false;
            UsuarioEntry.Text = ""; ClaveEntry.Text = ""; Subtitulo.Text = "Inicio de sesión"; totalCarrito = 0;
        }
        void LimpiarCampos()
        {
            Dato1.Text = ""; Dato2.Text = ""; Dato3.Text = "";
            Dato1.IsVisible = Dato2.IsVisible = Dato3.IsVisible = false;
            PlatoPicker.IsVisible = InsumoPicker.IsVisible = RolNuevoPicker.IsVisible = false;
            CobroPanel.IsVisible = false;
            PedidosTarjetasPanel.IsVisible = false; PedidosTarjetasPanel.Children.Clear();
            PedidoPanel.IsVisible = false; CantidadPedidoEntry.Text = ""; RecetaPanel.IsVisible = false; EjecutarBoton.IsVisible = false; AbrirPdfBoton.IsVisible = false; BoletaPicker.IsVisible = false;
            ingredientes = new int[101]; MostrarIngredientes();
        }
        void CampoVisible(Entry entrada, string titulo)
        {
            entrada.Placeholder = titulo; entrada.IsVisible = true;
        }
        void RecargarSelectores()
        {
            PlatoPicker.Items.Clear(); InsumoPicker.Items.Clear(); RecetaInsumoPicker.Items.Clear(); BoletaPicker.Items.Clear();
            for (int i = 0; i < sistema.TotalPlatos; i++)
                PlatoPicker.Items.Add(sistema.Platos[i].Nombre + " (ID " + sistema.Platos[i].Id + ")");
            for (int i = 0; i < sistema.TotalInsumos; i++)
            {
                string texto = sistema.Insumos[i].Nombre + " (stock: " + sistema.Insumos[i].Stock + ")";
                InsumoPicker.Items.Add(texto); RecetaInsumoPicker.Items.Add(texto);
            }

            for (int i = 0; i < sistema.TotalBoletas; i++)
            {
                string[] datosBoleta = sistema.Boletas[i].Split('|');
                string titulo = datosBoleta[0].Trim();
                string monto = datosBoleta.Length > 2 ? datosBoleta[2].Trim() : "Monto no disponible";
                BoletaPicker.Items.Add(titulo + " — " + monto);
            }
        }
        void SeleccionPlatoCambio(object sender, EventArgs e)
        {
            if (AccionPicker.SelectedItem == null || AccionPicker.SelectedItem.ToString() != "Editar plato") return;
            var p = sistema.BuscarPlato(PlatoSeleccionado());
            if (p != null) { Dato1.Text = p.Nombre; Dato2.Text = p.Categoria; Dato3.Text = p.Precio.ToString(CultureInfo.InvariantCulture); }
        }
        void SeleccionInsumoCambio(object sender, EventArgs e)
        {
            if (AccionPicker.SelectedItem == null || AccionPicker.SelectedItem.ToString() != "Editar insumo") return;
            var x = sistema.BuscarInsumo(InsumoSeleccionado(InsumoPicker));
            if (x != null) { Dato1.Text = x.Nombre; Dato2.Text = x.Stock.ToString(); Dato3.Text = x.Alerta.ToString(); }
        }
        int BoletaSeleccionada() { return BoletaPicker.SelectedIndex + 1; }
        int PlatoSeleccionado()
        {
            if (PlatoPicker.SelectedIndex < 0 || PlatoPicker.SelectedIndex >= sistema.TotalPlatos) return 0;
            return sistema.Platos[PlatoPicker.SelectedIndex].Id;
        }
        int InsumoSeleccionado(Picker selector)
        {
            if (selector.SelectedIndex < 0 || selector.SelectedIndex >= sistema.TotalInsumos) return 0;
            return sistema.Insumos[selector.SelectedIndex].Id;
        }
        void AccionCambio(object sender, EventArgs e)
        {
            LimpiarCampos(); RecargarSelectores();
            totalCarrito = 0; MostrarCarrito();
            string a = AccionPicker.SelectedItem == null ? "" : AccionPicker.SelectedItem.ToString();
            Instruccion.Text = "Selecciona lo que deseas consultar.";
            switch (a)
            {
                case "Agregar plato":
                    CampoVisible(Dato1, "Nombre del plato"); CampoVisible(Dato2, "Categoría"); CampoVisible(Dato3, "Precio (ej. 30.00)");
                    RecetaPanel.IsVisible = true; EjecutarBoton.IsVisible = true; EjecutarBoton.Text = "Guardar plato y receta";
                    Instruccion.Text = "Primero registra los insumos en Inventario; después selecciona los ingredientes de este plato."; break;
                case "Editar plato":
                    PlatoPicker.IsVisible = true; CampoVisible(Dato1, "Nuevo nombre"); CampoVisible(Dato2, "Nueva categoría"); CampoVisible(Dato3, "Nuevo precio");
                    EjecutarBoton.IsVisible = true; EjecutarBoton.Text = "Guardar cambios"; break;
                case "Eliminar plato":
                    PlatoPicker.IsVisible = true; EjecutarBoton.IsVisible = true; EjecutarBoton.Text = "Eliminar plato"; break;
                case "Editar insumo":
                    InsumoPicker.IsVisible = true; CampoVisible(Dato1, "Nuevo nombre"); CampoVisible(Dato2, "Stock actual"); CampoVisible(Dato3, "Alerta de stock");
                    EjecutarBoton.IsVisible = true; EjecutarBoton.Text = "Guardar cambios"; break;
                case "Eliminar insumo":
                    InsumoPicker.IsVisible = true; EjecutarBoton.IsVisible = true; EjecutarBoton.Text = "Eliminar insumo"; break;
                case "Ver PDF / imprimir":
                    BoletaPicker.IsVisible = true; AbrirPdfBoton.IsVisible = true;
                    Instruccion.Text = "Selecciona el comprobante y abre su PDF. Desde el visor de Windows puedes imprimirlo."; break;
                case "Editar receta":
                    PlatoPicker.IsVisible = true; RecetaPanel.IsVisible = true; EjecutarBoton.IsVisible = true; EjecutarBoton.Text = "Guardar nueva receta";
                    Instruccion.Text = "Selecciona un plato y añade los ingredientes de su receta. La receta anterior será sustituida."; break;
                case "Agregar insumo":
                    CampoVisible(Dato1, "Nombre del insumo"); CampoVisible(Dato2, "Stock inicial (entero)"); CampoVisible(Dato3, "Alerta de stock (entero)");
                    EjecutarBoton.IsVisible = true; EjecutarBoton.Text = "Guardar insumo"; break;
                case "Reponer / merma":
                    InsumoPicker.IsVisible = true; CampoVisible(Dato1, "Cantidad: positiva repone; negativa descuenta");
                    EjecutarBoton.IsVisible = true; EjecutarBoton.Text = "Actualizar inventario"; break;
                case "Crear pedido":
                    CampoVisible(Dato1, "Número de mesa"); PedidoPanel.IsVisible = true; PlatoPicker.IsVisible = true;
                    CampoVisible(Dato3, "Observaciones generales (opcional)");
                    EjecutarBoton.IsVisible = true; EjecutarBoton.Text = "Confirmar pedido completo"; break;
                case "Cobrar mesa":

                    actualizandoDocumento = true;
                    NumeroDocumentoEntry.Text = "";
                    TipoComprobantePicker.SelectedIndex = 0;
                    TipoDocumentoPicker.SelectedIndex = 0;
                    TipoDocumentoPicker.IsEnabled = true;
                    facturaAutomatica = false;
                    numeroEraRuc = false;
                    actualizandoDocumento = false;
                    CampoVisible(Dato1, "Número de mesa"); CobroPanel.IsVisible = true; EjecutarBoton.IsVisible = true; EjecutarBoton.Text = "Cobrar mesa"; break;
                case "Crear usuario":
                    CampoVisible(Dato1, "Nombre de usuario"); CampoVisible(Dato2, "Contraseña"); RolNuevoPicker.IsVisible = true;
                    EjecutarBoton.IsVisible = true; EjecutarBoton.Text = "Crear usuario"; break;
                case "Crear backup":
                    EjecutarBoton.IsVisible = true; EjecutarBoton.Text = "Crear backup"; break;
                case "Recuperar último backup":
                    EjecutarBoton.IsVisible = true; EjecutarBoton.Text = "Recuperar backup"; break;
                case "Ayuda": Instruccion.Text = "Selecciona una función del menú para ver sus campos. Lee la guía de esta área aquí abajo."; break;
            }
            ActualizarListado();
        }
        void MostrarIngredientes()
        {
            string salida = "";
            for (int i = 0; i < sistema.TotalInsumos; i++)
            {
                int id = sistema.Insumos[i].Id;
                if (ingredientes[id] > 0) salida += sistema.Insumos[i].Nombre + ": " + ingredientes[id] + " por plato\n";
            }
            RecetaListado.Text = salida == "" ? "Todavía no has añadido ingredientes." : salida;
        }
        void AnadirIngredienteClicked(object sender, EventArgs e)
        {
            int id = InsumoSeleccionado(RecetaInsumoPicker);
            if (id == 0 || !Entero(CantidadRecetaEntry, out int cantidad) || cantidad <= 0)
            {
                Aviso("Selecciona un insumo y escribe una cantidad entera mayor que cero."); return;
            }
            ingredientes[id] = cantidad; CantidadRecetaEntry.Text = ""; MostrarIngredientes();
        }
        void LimpiarIngredientesClicked(object sender, EventArgs e) { ingredientes = new int[101]; MostrarIngredientes(); }
        void EjecutarClicked(object sender, EventArgs e)
        {
            string a = AccionPicker.SelectedItem == null ? "" : AccionPicker.SelectedItem.ToString();
            if (a == "Eliminar plato")
            {
                Confirmar("Eliminar plato", "Se quitará de la carta pero se conservarán los pedidos históricos. ¿Continuar?", a);
                return;
            }
            if (a == "Eliminar insumo")
            {
                Confirmar("Eliminar insumo", "Un insumo utilizado por recetas no puede eliminarse. ¿Continuar?", a);
                return;
            }
            if (a == "Recuperar último backup")
            {
                Confirmar("Restaurar", "La copia más reciente reemplazará los archivos actuales. ¿Continuar?", a);
                return;
            }
            EjecutarOperacion(a);
        }
        void EjecutarOperacion(string a)
        {
            string mensaje = ""; int n, m; decimal precio;
            int boletasAntes = sistema.TotalBoletas;
            try
            {
                switch (a)
                {
                    case "Agregar plato":
                        mensaje = Dinero(Dato3, out precio) ? sistema.GuardarPlatoConReceta(Campo(Dato1), Campo(Dato2), precio, ingredientes) : "Ingresa un precio correcto, por ejemplo 30.00."; break;
                    case "Editar plato":
                        mensaje = Dinero(Dato3, out precio) ? sistema.EditarPlato(PlatoSeleccionado(), Campo(Dato1), Campo(Dato2), precio) : "Introduce un precio válido."; break;
                    case "Eliminar plato":
                        mensaje = sistema.EliminarPlato(PlatoSeleccionado()); break;
                    case "Editar insumo":
                        mensaje = Entero(Dato2, out n) && Entero(Dato3, out m) ? sistema.EditarInsumo(InsumoSeleccionado(InsumoPicker), Campo(Dato1), n, m) : "Stock y alerta deben ser enteros."; break;
                    case "Eliminar insumo":
                        mensaje = sistema.EliminarInsumo(InsumoSeleccionado(InsumoPicker)); break;
                    case "Editar receta":
                        mensaje = sistema.ActualizarReceta(PlatoSeleccionado(), ingredientes); break;
                    case "Agregar insumo":
                        mensaje = Entero(Dato2, out n) && Entero(Dato3, out m) ? sistema.AgregarInsumo(Campo(Dato1), n, m) : "Stock y alerta deben ser números enteros."; break;
                    case "Reponer / merma":
                        mensaje = Entero(Dato1, out n) ? sistema.AjustarStock(InsumoSeleccionado(InsumoPicker), n) : "Ingresa una cantidad entera."; break;
                    case "Crear pedido":
                        mensaje = Entero(Dato1, out n) ? sistema.CrearPedidoCompleto(n, carritoPlatos, carritoCantidades, totalCarrito, Campo(Dato3)) : "La mesa debe ser un número entero.";
                        if (mensaje.StartsWith("Pedido #")) { totalCarrito = 0; MostrarCarrito(); }
                        break;
                    case "Cobrar mesa":
                        mensaje = Entero(Dato1, out n) ? sistema.CobrarMesa(n, TipoComprobantePicker.SelectedItem?.ToString() ?? "Boleta simple", TipoDocumentoPicker.SelectedItem?.ToString() ?? "DNI", Campo(NumeroDocumentoEntry), MedioPagoPicker.SelectedItem?.ToString() ?? "Efectivo") : "Escribe un número de mesa correcto."; break;
                    case "Crear usuario":
                        mensaje = RolNuevoPicker.SelectedItem == null ? "Selecciona un rol." : sistema.NuevoUsuario(Campo(Dato1), Campo(Dato2), RolNuevoPicker.SelectedItem.ToString()); break;
                    case "Crear backup":
                        mensaje = sistema.CrearBackup(); break;
                    case "Recuperar último backup":
                        mensaje = sistema.RestaurarUltimoBackup(); break;
                }
            }
            catch (Exception) { mensaje = "No se pudo completar la operación. Verifica los datos o los archivos."; }

            if (a == "Cobrar mesa" && sistema.TotalBoletas == boletasAntes + 1 &&
                mensaje == sistema.Boletas[sistema.TotalBoletas - 1])
            {
                RecargarSelectores();
                ActualizarListado();

                if (sistema.ModoPractica)
                {
                    Aviso("Cobro de práctica registrado. No se genera PDF en el modo aprendizaje.");
                }
                else
                {
                    string[] datosBoleta = mensaje.Split('|');
                    string resumen = datosBoleta[0].Trim() + "\n" + datosBoleta[2].Trim();
                    comprobantePendiente = sistema.TotalBoletas;
                    Confirmar("Comprobante generado", resumen + "\n\n¿Deseas abrir el PDF para imprimirlo?", "abrir comprobante");
                }
                return;
            }

            Aviso(mensaje);
            if (mensaje.Contains("correctamente") || mensaje.StartsWith("Plato registrado") || mensaje.StartsWith("Insumo registrado") || mensaje.StartsWith("Pedido registrado") || mensaje.StartsWith("Pedido #"))
            {
                Dato1.Text = ""; Dato2.Text = ""; Dato3.Text = "";
                if (a == "Agregar plato" || a == "Editar receta") { ingredientes = new int[101]; MostrarIngredientes(); }
            }
            RecargarSelectores(); ActualizarListado();
        }

        bool actualizandoDocumento = false;
        bool facturaAutomatica = false;
        bool numeroEraRuc = false;

        bool TieneOnceDigitos(string numero)
        {
            if (numero.Length != 11) return false;
            for (int i = 0; i < numero.Length; i++)
            {
                if (numero[i] < '0' || numero[i] > '9') return false;
            }
            return true;
        }

        void NumeroDocumentoCambio(object sender, TextChangedEventArgs e)
        {
            if (actualizandoDocumento) return;
            actualizandoDocumento = true;
            try
            {
                bool esRuc = TieneOnceDigitos(Campo(NumeroDocumentoEntry));

                if (esRuc && !numeroEraRuc && TipoComprobantePicker.SelectedIndex != 2)
                {
                    facturaAutomatica = true;
                    TipoDocumentoPicker.SelectedIndex = 3; 
                    TipoComprobantePicker.SelectedIndex = 2; 
                }
                else if (!esRuc && facturaAutomatica)
                {

                    facturaAutomatica = false;
                    TipoComprobantePicker.SelectedIndex = 0; 
                    TipoDocumentoPicker.SelectedIndex = 0; 
                }
                numeroEraRuc = esRuc;
                TipoDocumentoPicker.IsEnabled = TipoComprobantePicker.SelectedIndex != 2;
            }
            finally
            {
                actualizandoDocumento = false;
            }
        }

        void DocumentoCambio(object sender, EventArgs e)
        {
            if (actualizandoDocumento) return;
            actualizandoDocumento = true;
            try
            {
                if (sender == TipoComprobantePicker)
                {

                    facturaAutomatica = false;
                    if (TipoComprobantePicker.SelectedIndex == 2)
                        TipoDocumentoPicker.SelectedIndex = 3; 
                }
                else if (sender == TipoDocumentoPicker)
                {
                    if (TipoDocumentoPicker.SelectedIndex == 3)
                    {

                        facturaAutomatica = false;
                        TipoComprobantePicker.SelectedIndex = 2;
                    }
                }
                TipoDocumentoPicker.IsEnabled = TipoComprobantePicker.SelectedIndex != 2;
            }
            finally
            {
                actualizandoDocumento = false;
            }
        }

        void MostrarCarrito()
        {
            CarritoFilasPanel.Children.Clear();
            decimal total = 0;

            for (int i = 0; i < totalCarrito; i++)
            {
                var plato = sistema.BuscarPlato(carritoPlatos[i]);
                if (plato == null) continue;
                total += plato.Precio * carritoCantidades[i];

                var fila = new VerticalStackLayout
                {
                    Spacing = 6,
                    Padding = new Thickness(10),
                    BackgroundColor = Color.FromArgb("#F8FBEF")
                };
                fila.Children.Add(new Label
                {
                    Text = plato.Nombre + "   x" + carritoCantidades[i] + "  —  S/ " +
                           (plato.Precio * carritoCantidades[i]).ToString("0.00"),
                    FontFamily = "FuenteGoodMoov",
                    FontSize = 15,
                    TextColor = Color.FromArgb("#333333")
                });
                var botones = new HorizontalStackLayout { Spacing = 8 };
                var sumar = new Button
                {
                    Text = "+1", FontFamily = "FuenteGoodMoov",
                    BackgroundColor = Color.FromArgb("#D4DFAB"),
                    TextColor = Color.FromArgb("#333333"),
                    CommandParameter = i
                };
                sumar.Clicked += SumarCantidadClicked;

                var restar = new Button
                {
                    Text = "-1", FontFamily = "FuenteGoodMoov",
                    BackgroundColor = Color.FromArgb("#D4DFAB"),
                    TextColor = Color.FromArgb("#333333"),
                    CommandParameter = i
                };
                restar.Clicked += RestarCantidadClicked;

                var quitar = new Button
                {
                    Text = "Quitar fila", FontFamily = "FuenteGoodMoov",
                    BackgroundColor = Color.FromArgb("#D4DFAB"),
                    TextColor = Color.FromArgb("#333333"),
                    CommandParameter = i
                };
                quitar.Clicked += QuitarFilaClicked;
                botones.Children.Add(restar);
                botones.Children.Add(sumar);
                botones.Children.Add(quitar);
                fila.Children.Add(botones);
                CarritoFilasPanel.Children.Add(fila);
            }
            CarritoListado.Text = totalCarrito == 0
                ? "Todavía no hay platos en este pedido."
                : "Total del pedido: S/ " + total.ToString("0.00");
        }

        void SumarCantidadClicked(object sender, EventArgs e)
        {
            Button boton = (Button)sender;
            int posicion = (int)boton.CommandParameter;
            if (posicion < 0 || posicion >= totalCarrito) return;

            if (carritoCantidades[posicion] == int.MaxValue)
            {
                Aviso("Cantidad demasiado grande.");
                return;
            }

            carritoCantidades[posicion]++;
            MostrarCarrito();
        }

        void RestarCantidadClicked(object sender, EventArgs e)
        {
            Button boton = (Button)sender;
            int posicion = (int)boton.CommandParameter;
            if (posicion < 0 || posicion >= totalCarrito) return;

            carritoCantidades[posicion]--;
            if (carritoCantidades[posicion] <= 0)
                EliminarFilaCarrito(posicion);

            MostrarCarrito();
        }

        void QuitarFilaClicked(object sender, EventArgs e)
        {
            Button boton = (Button)sender;
            int posicion = (int)boton.CommandParameter;
            EliminarFilaCarrito(posicion);
            MostrarCarrito();
        }

        void EliminarFilaCarrito(int posicion)
        {
            if (posicion < 0 || posicion >= totalCarrito) return;
            for (int i = posicion; i < totalCarrito - 1; i++)
            {
                carritoPlatos[i] = carritoPlatos[i + 1];
                carritoCantidades[i] = carritoCantidades[i + 1];
            }
            totalCarrito--;
        }
        void AgregarAlPedidoClicked(object sender, EventArgs e)
        {
            int platoId = PlatoSeleccionado();
            int cantidad;
            if (platoId == 0 || !int.TryParse(Campo(CantidadPedidoEntry), out cantidad) || cantidad <= 0)
            {
                Aviso("Selecciona un plato y escribe una cantidad entera mayor que cero."); return;
            }
            int indice = -1;
            for (int i = 0; i < totalCarrito; i++)
                if (carritoPlatos[i] == platoId) indice = i;
            if (indice >= 0)
            {
                if ((long)carritoCantidades[indice] + cantidad > int.MaxValue) { Aviso("Cantidad excesiva."); return; }
                carritoCantidades[indice] += cantidad;
            }
            else
            {
                if (totalCarrito >= carritoPlatos.Length) { Aviso("Carrito completo."); return; }
                carritoPlatos[totalCarrito] = platoId;
                carritoCantidades[totalCarrito] = cantidad;
                totalCarrito++;
            }
            CantidadPedidoEntry.Text = "";
            MostrarCarrito();
        }
        void AbrirComprobantePdf(int numero)
        {
            try
            {
                string resultado = sistema.GenerarPdfBoleta(numero);
                if (!File.Exists(resultado)) { Aviso(resultado); return; }
#if WINDOWS
                System.Diagnostics.ProcessStartInfo apertura = new System.Diagnostics.ProcessStartInfo();
                apertura.FileName = resultado;
                apertura.UseShellExecute = true;
                System.Diagnostics.Process.Start(apertura);
#else
                Aviso("El comprobante se guardó en:\n" + resultado + "\nPuedes abrirlo con un lector PDF del dispositivo.");
#endif
            }
            catch (Exception)
            {
                Aviso("No fue posible abrir el PDF. Comprueba el visor de PDF de Windows.");
            }
        }

        void AbrirPdfClicked(object sender, EventArgs e)
        {
            int numero = BoletaSeleccionada();
            if (numero <= 0) { Aviso("Selecciona un comprobante."); return; }
            AbrirComprobantePdf(numero);
        }
        void ActualizarListado()
        {
            string a = AccionPicker.SelectedItem == null ? "" : AccionPicker.SelectedItem.ToString();
            string texto = "";
            if (a == "Ver carta")
            {
                texto = sistema.CartaOrdenada();
            }
            else if (a == "Agregar plato" || a == "Editar plato" || a == "Eliminar plato" || a == "Editar receta" || a == "Crear pedido")
            {
                for (int i = 0; i < sistema.TotalPlatos; i++)
                {
                    var p = sistema.Platos[i]; texto += p.Nombre + " | " + p.Categoria + " | S/ " + p.Precio + " | Disponibles: " + sistema.Disponibles(p.Id) + "\n";
                }
                if (texto == "") texto = "Todavía no hay platos. Agrega insumos y luego crea platos con recetas.";
                if (a == "Crear pedido") texto += "\nALERTAS:\n" + sistema.Alertas();
            }
            else if (a == "Ver inventario" || a == "Agregar insumo" || a == "Editar insumo" || a == "Eliminar insumo" || a == "Reponer / merma" || a == "Ver alertas")
            {
                for (int i = 0; i < sistema.TotalInsumos; i++)
                {
                    var x = sistema.Insumos[i]; texto += x.Nombre + " | Stock: " + x.Stock + " | Alerta: " + x.Alerta + "\n";
                }
                if (texto == "") texto = "Inventario vacío.\n";
                texto += "\n" + sistema.Alertas();
            }
            else if (a == "Ver pedidos" || a == "Cobrar mesa")
            {
                for (int i = 0; i < sistema.TotalPedidos; i++)
                {
                    var p = sistema.Pedidos[i];
                    if (p.Estado == "Cancelado" || p.Estado == "Cobrado") continue;
                    int grupo = p.GrupoId == 0 ? p.Id : p.GrupoId;
                    bool primero = true;
                    for (int j = 0; j < i; j++)
                    {
                        var anterior = sistema.Pedidos[j];
                        int grupoAnterior = anterior.GrupoId == 0 ? anterior.Id : anterior.GrupoId;
                        if (grupoAnterior == grupo && anterior.Mesa == p.Mesa) primero = false;
                    }
                    if (primero) texto += "\nMESA " + p.Mesa + " — PEDIDO #" + grupo + "\n";
                    texto += "   Línea #" + p.Id + ": " + p.PlatoNombre + " x" + p.Cantidad + " | S/ " + p.Total() + " | " + p.Estado + "\n";
                    if (p.Observacion != "") texto += "   Observaciones: " + p.Observacion + "\n";
                }
                if (texto == "") texto = "No hay pedidos activos pendientes.";
                if (a == "Cobrar mesa" || (a == "Ver pedidos" && rolActual == "CAJA"))
                {
                    string esperando = "";
                    for (int i = 0; i < sistema.TotalPedidos; i++)
                    {
                        var pedido = sistema.Pedidos[i];
                        if (pedido.Estado != "Cobrado" && pedido.Estado != "Cancelado" && pedido.Estado != "Entregado")
                            esperando += "Mesa " + pedido.Mesa + ": " + pedido.PlatoNombre +
                                " x" + pedido.Cantidad + " (" + pedido.Estado + ")\n";
                    }
                    if (esperando != "") texto += "\nNO LISTOS PARA COBRAR:\n" + esperando +
                        "\nCaja debe esperar a que todos los platos activos de cada mesa estén entregados.";
                }
            }
            else if (a == "Ver boletas" || a == "Ver PDF / imprimir")
            {
                for (int i = 0; i < sistema.TotalBoletas; i++) texto += sistema.Boletas[i] + "\n";
                if (texto == "") texto = "Todavía no hay boletas.";
            }
            else if (a == "Ayuda") texto = "CUENTAS DE PRUEBA\nADMIN: admin / 1989\nCOCINA: cocina / 123\nMOZO: mozo / 123\nCAJA: caja / 123\n\n1. ADMIN administra usuarios, platos e inventario.\n2. COCINA revisa cada plato y pulsa Preparar, Listo, Entregado o Cancelar.\n3. MOZO añade varios platos al carrito y cambia cantidades antes de confirmar.\n4. CAJA solo cobra cuando todos los platos activos de la mesa están Entregados.\n5. ADMIN puede respaldar o recuperar los datos.\n\nLas cuatro cuentas de prueba tienen contraseñas fijas. Los demás usuarios creados se conservan. Los archivos .txt se guardan automáticamente. El modo aprendizaje no modifica los archivos reales.";
            else texto = "Completa el formulario de esta función.";
            bool mostrarTarjetas = (a == "Ver pedidos") &&
                (rolActual == "COCINA" || rolActual == "ADMIN");
            if (mostrarTarjetas)
            {
                Resultado.Text = "Cada plato conserva su propio estado. Los platos con el mismo Grupo pertenecen a un único pedido.";
                MostrarTarjetasPedidos();
            }
            else
            {
                PedidosTarjetasPanel.IsVisible = false;
                PedidosTarjetasPanel.Children.Clear();
                Resultado.Text = texto;
            }
        }

        void MostrarTarjetasPedidos()
        {
            PedidosTarjetasPanel.Children.Clear();
            PedidosTarjetasPanel.IsVisible = true;
            for (int i = 0; i < sistema.TotalPedidos; i++)
            {
                var basePedido = sistema.Pedidos[i];
                if (basePedido.Estado == "Entregado" || basePedido.Estado == "Cobrado" || basePedido.Estado == "Cancelado") continue;
                int grupo = basePedido.GrupoId == 0 ? basePedido.Id : basePedido.GrupoId;
                bool yaMostrado = false;
                for (int k = 0; k < i; k++)
                {
                    var previo = sistema.Pedidos[k];
                    if ((previo.GrupoId == 0 ? previo.Id : previo.GrupoId) == grupo && previo.Mesa == basePedido.Mesa &&
                        previo.Estado != "Entregado" && previo.Estado != "Cobrado" && previo.Estado != "Cancelado") yaMostrado = true;
                }
                if (yaMostrado) continue;
                var tarjeta = new VerticalStackLayout { Spacing = 9, Padding = new Thickness(12), BackgroundColor = Color.FromArgb("#F8FBEF") };
                tarjeta.Children.Add(new Label { Text = "Mesa " + basePedido.Mesa + " · Pedido #" + grupo,
                    FontFamily = "FuenteGoodMoov", FontAttributes = FontAttributes.Bold, FontSize = 18, TextColor = Color.FromArgb("#333333") });
                for (int j = i; j < sistema.TotalPedidos; j++)
                {
                    var pedido = sistema.Pedidos[j];
                    if (pedido.Mesa != basePedido.Mesa || (pedido.GrupoId == 0 ? pedido.Id : pedido.GrupoId) != grupo) continue;
                    if (pedido.Estado == "Entregado" || pedido.Estado == "Cobrado" || pedido.Estado == "Cancelado") continue;
                    var linea = new VerticalStackLayout { Spacing = 5 };
                    linea.Children.Add(new Label { Text = "Línea #" + pedido.Id + " · " + pedido.PlatoNombre + " x" + pedido.Cantidad + " · S/ " + pedido.Total().ToString("0.00"),
                        FontFamily = "FuenteGoodMoov", TextColor = Color.FromArgb("#333333") });
                    linea.Children.Add(new Label { Text = "Estado: " + pedido.Estado, FontFamily = "FuenteGoodMoov", TextColor = Color.FromArgb("#333333") });
                    if (pedido.Observacion != "") linea.Children.Add(new Label { Text = "Observaciones: " + pedido.Observacion, FontFamily = "FuenteGoodMoov", TextColor = Color.FromArgb("#333333") });
                    string textoBoton = pedido.Estado == "Pendiente" ? "Comenzar preparación" : pedido.Estado == "En preparación" ? "Marcar listo" : "Marcar entregado";
                    var boton = new Button
                    {
                        Text = textoBoton,
                        BackgroundColor = Color.FromArgb("#D4DFAB"),
                        TextColor = Color.FromArgb("#333333"),
                        FontFamily = "FuenteGoodMoov",
                        CommandParameter = pedido.Id
                    };
                    boton.Clicked += CambiarEstadoPedidoClicked;
                    linea.Children.Add(boton);

                    var cancelar = new Button
                    {
                        Text = "Cancelar plato",
                        BackgroundColor = Color.FromArgb("#F0D4CF"),
                        TextColor = Color.FromArgb("#333333"),
                        FontFamily = "FuenteGoodMoov",
                        CommandParameter = pedido.Id
                    };
                    cancelar.Clicked += CancelarPedidoClicked;
                    linea.Children.Add(cancelar);
                    tarjeta.Children.Add(linea);
                }
                PedidosTarjetasPanel.Children.Add(tarjeta);
            }
            if (PedidosTarjetasPanel.Children.Count == 0)
                PedidosTarjetasPanel.Children.Add(new Label { Text = "No hay pedidos pendientes en Cocina.", TextColor = Color.FromArgb("#333333") });
        }

        void CambiarEstadoPedidoClicked(object sender, EventArgs e)
        {
            Button boton = (Button)sender;
            int idPedido = (int)boton.CommandParameter;
            var pedido = sistema.BuscarPedido(idPedido);
            if (pedido == null)
            {
                Aviso("Este pedido ya no existe.");
                return;
            }

            string siguiente = "";
            if (pedido.Estado == "Pendiente") siguiente = "En preparación";
            else if (pedido.Estado == "En preparación") siguiente = "Listo";
            else if (pedido.Estado == "Listo") siguiente = "Entregado";
            else
            {
                Aviso("Este pedido ya no necesita preparación.");
                ActualizarListado();
                return;
            }

            string resultado = sistema.EstadoPedido(idPedido, siguiente);
            if (resultado != "Estado actualizado.") Aviso(resultado);
            ActualizarListado();
        }

        void CancelarPedidoClicked(object sender, EventArgs e)
        {
            Button boton = (Button)sender;
            int idPedido = (int)boton.CommandParameter;
            var pedido = sistema.BuscarPedido(idPedido);
            if (pedido == null) { Aviso("No se encontró el pedido."); return; }
            pedidoPendiente = idPedido;
            Confirmar("Cancelar plato", "¿Cancelar " + pedido.PlatoNombre + " de la mesa " + pedido.Mesa + "?", "cancelar plato");
        }

        void AyudaClicked(object sender, EventArgs e)
        {
            Aviso("USUARIOS DE PRUEBA\nADMIN: admin / 1989\nCOCINA: cocina / 123\nMOZO: mozo / 123\nCAJA: caja / 123\n\nEstos accesos son para la simulación. Estas cuatro cuentas de prueba tienen contraseñas fijas. Las demás cuentas se conservan y ADMIN puede crear otras.\n\nEn Modo aprendizaje puedes explorar los módulos sin guardar cambios.\n\nPrimero agrega insumos; después crea platos con su receta. El mozo podrá elegirlos de una lista.");
        }
    }
}
