# GMImproved (GDIMPROVED)

Prototipo en C# (consola) para mejorar la gestion de pedidos e inventario del restaurante Good Moov.
Proyecto del curso Fundamentos de Programacion - UPN.

## Integrantes
- Marycielo Gonzales Rupay
- Daphne Akemy Tamara Ponce
- Jonathan Jesus Bellido Mendez
- Dulce Antonella Barreros Mamani

## Que hace el programa
- Login por roles: admin, mozo, cocina y caja (clave de prueba: 123).
- Inventario: ver, agregar, eliminar, reponer y dar de baja stock (mermas).
- Pedidos: registrar (descuenta stock), buscar, cambiar estado, cancelar y ordenar con metodo burbuja.
- Caja: cobrar una mesa con propina, metodo de pago y boleta o factura.
- Archivos: guarda y carga el inventario y los pedidos en archivos de texto.

## Modulos
| Archivo | Funcion |
|---|---|
| Program.cs | Main, Login y menus por rol |
| Utilidades.cs | Lectura y validacion de datos |
| Inventario.cs | Productos y stock |
| Pedidos.cs | Registro, busqueda, estados y ordenamiento |
| Caja.cs | Cobro y comprobantes |
| Archivos.cs | Guardar y cargar en .txt |

## Como ejecutarlo
1. En Visual Studio crear un proyecto nuevo: Console App (C#).
2. Agregar los archivos .cs de este repositorio (clic derecho al proyecto > Add > Existing Item) y reemplazar Program.cs.
3. Presionar F5 o el boton Iniciar.
4. Entrar con admin / 123 para agregar productos primero.
