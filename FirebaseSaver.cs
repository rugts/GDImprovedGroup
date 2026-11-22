using Firebase.Database;
using Firebase.Database.Query;
using GDImproved.Logica;
using System;
using System.Reactive.Linq;
using GDImproved.Modelos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GDImproved
{
    public class FirebaseSaver
    {
       
        private readonly FirebaseClient _client = new FirebaseClient("https://gdimproved-default-rtdb.firebaseio.com/");

  
        public async Task SincronizarTodoAsync(Sistema sistema)
        {
            if (sistema == null || sistema.ModoPractica) return;

            try
            {
                var listaPlatos = new List<Plato>();
                for (int i = 0; i < sistema.TotalPlatos; i++)
                    if (sistema.Platos[i] != null) listaPlatos.Add(sistema.Platos[i]);

                var listaInsumos = new List<Insumo>();
                for (int i = 0; i < sistema.TotalInsumos; i++)
                    if (sistema.Insumos[i] != null) listaInsumos.Add(sistema.Insumos[i]);

                var listaPedidos = new List<Pedido>();
                for (int i = 0; i < sistema.TotalPedidos; i++)
                    if (sistema.Pedidos[i] != null) listaPedidos.Add(sistema.Pedidos[i]);

                var listaUsuarios = new List<Usuario>();
                for (int i = 0; i < sistema.TotalUsuarios; i++)
                    if (sistema.Usuarios[i] != null) listaUsuarios.Add(sistema.Usuarios[i]);

                var listaBoletas = new List<string>();
                for (int i = 0; i < sistema.TotalBoletas; i++)
                    if (!string.IsNullOrWhiteSpace(sistema.Boletas[i])) listaBoletas.Add(sistema.Boletas[i]);

                await _client.Child("good_moov").Child("platos").PutAsync(listaPlatos);
                await _client.Child("good_moov").Child("insumos").PutAsync(listaInsumos);
                await _client.Child("good_moov").Child("pedidos").PutAsync(listaPedidos);
                await _client.Child("good_moov").Child("usuarios").PutAsync(listaUsuarios);
                await _client.Child("good_moov").Child("boletas").PutAsync(listaBoletas);
            }
            catch (Exception ex)
            {
                sistema.ErrorCarga = "Error subiendo a Firebase: " + ex.Message;
            }
        }
        public void EscucharPedidosEnTiempoReal(Sistema sistema, Action onActualizarUI)
        {
            if (sistema == null || sistema.ModoPractica) return;

     
            _client
                .Child("good_moov")
                .Child("pedidos")
                .AsObservable<Pedido>()
                .Subscribe(d =>
                {
        
                    Task.Run(async () =>
                    {
                        await CargarDesdeFirebaseAsync(sistema);

     
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            onActualizarUI?.Invoke();
                        });
                    });
                });
        }

        public async Task CargarDesdeFirebaseAsync(Sistema sistema)
        {
            if (sistema == null || sistema.ModoPractica) return;

            try
            {
                var usuariosNube = await _client.Child("good_moov").Child("usuarios").OnceSingleAsync<List<Usuario>>();
                if (usuariosNube != null && usuariosNube.Count > 0)
                {
                    sistema.TotalUsuarios = 0;
                    foreach (var u in usuariosNube)
                        if (u != null && sistema.TotalUsuarios < sistema.Usuarios.Length)
                            sistema.Usuarios[sistema.TotalUsuarios++] = u;
                }

                var platosNube = await _client.Child("good_moov").Child("platos").OnceSingleAsync<List<Plato>>();
                if (platosNube != null && platosNube.Count > 0)
                {
                    sistema.TotalPlatos = 0;
                    foreach (var p in platosNube)
                        if (p != null && sistema.TotalPlatos < sistema.Platos.Length)
                            sistema.Platos[sistema.TotalPlatos++] = p;
                }

                var insumosNube = await _client.Child("good_moov").Child("insumos").OnceSingleAsync<List<Insumo>>();
                if (insumosNube != null && insumosNube.Count > 0)
                {
                    sistema.TotalInsumos = 0;
                    foreach (var ins in insumosNube)
                        if (ins != null && sistema.TotalInsumos < sistema.Insumos.Length)
                            sistema.Insumos[sistema.TotalInsumos++] = ins;
                }

                var pedidosNube = await _client.Child("good_moov").Child("pedidos").OnceSingleAsync<List<Pedido>>();
                if (pedidosNube != null && pedidosNube.Count > 0)
                {
                    sistema.TotalPedidos = 0;
                    foreach (var ped in pedidosNube)
                        if (ped != null && sistema.TotalPedidos < sistema.Pedidos.Length)
                            sistema.Pedidos[sistema.TotalPedidos++] = ped;
                }

                var boletasNube = await _client.Child("good_moov").Child("boletas").OnceSingleAsync<List<string>>();
                if (boletasNube != null && boletasNube.Count > 0)
                {
                    sistema.TotalBoletas = 0;
                    foreach (var b in boletasNube)
                        if (!string.IsNullOrWhiteSpace(b) && sistema.TotalBoletas < sistema.Boletas.Length)
                            sistema.Boletas[sistema.TotalBoletas++] = b;
                }

  
                sistema.GuardarSoloLocal();
            }
            catch (Exception ex)
            {
                sistema.ErrorCarga = "Error leyendo Firebase: " + ex.Message;
            }
        }
    }
}
