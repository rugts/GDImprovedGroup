namespace GDImproved.Modelos
{
    public class Plato
    {
        public int Id; public string Nombre = ""; public string Categoria = ""; public decimal Precio;
    }
    public class Insumo
    {
        public int Id; public string Nombre = ""; public int Stock; public int Alerta = 3;
    }
    public class Pedido
    {
        public int Id; public int GrupoId; public int Mesa; public int PlatoId; public string PlatoNombre = "";
        public int Cantidad; public decimal Precio; public string Observacion = "";
        public string Estado = "Pendiente"; public string Fecha = "";
        public decimal Total() { return Precio * Cantidad; }
    }
    public class Usuario
    {
        public string Nombre = ""; public string Clave = ""; public string Rol = "";
    }
}
