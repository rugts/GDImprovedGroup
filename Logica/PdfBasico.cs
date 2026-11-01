using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace GDImproved.Logica
{
    public class PdfBasico
    {
        static string Escapar(string texto)
        {
            if (texto == null) return "";
            StringBuilder limpio = new StringBuilder();
            foreach (char original in texto)
            {
                char letra = original;
                if (letra > 255 || letra == '\r' || letra == '\n' || char.IsControl(letra)) letra = ' ';
                if (letra == '\\' || letra == '(' || letra == ')') limpio.Append('\\');
                limpio.Append(letra);
            }
            return limpio.ToString();
        }

        public static void Crear(string ruta, string[] lineas, int cantidad)
        {
            string contenido = "BT /F1 11 Tf 45 785 Td 15 TL\n";
            int maximo = Math.Min(cantidad, 43);
            for (int i = 0; i < maximo; i++)
            {
                if (i > 0) contenido += "T*\n";
                contenido += "(" + Escapar(lineas[i]) + ") Tj\n";
            }
            contenido += "ET\n";
            GuardarPdf(ruta, contenido, 595, 842);
        }

        public static void CrearComprobante(
            string ruta,
            string tipoComprobante,
            string referencia,
            string fecha,
            string mesa,
            string documento,
            string cliente,
            string medioPago,
            string total,
            string[] detalleItems)
        {
            const double ancho = 595;
            const double margen = 35;
            const double altoFila = 23;
            int cantidadItems = detalleItems == null ? 0 : detalleItems.Length;
            if (cantidadItems == 0) cantidadItems = 1;

            double inicioFilas = 438;
            double finFilas = inicioFilas + cantidadItems * altoFila;
            double totalTop = finFilas + 15;
            double pieTop = totalTop + 78;
            double altoPagina = Math.Max(842, pieTop + 60);

            StringBuilder c = new StringBuilder();

            Rect(c, 0, 0, ancho, 108, 0.15, 0.34, 0.21, altoPagina);
            Rect(c, 0, 108, ancho, 5, 0.76, 0.92, 0.39, altoPagina);
            Texto(c, 37, 28, "GOOD MOOV", 27, true, 1, 1, 1, altoPagina);
            Texto(c, 39, 65, "RESTAURANTE | SISTEMA DE GESTION", 9, false, 0.91, 0.96, 0.86, altoPagina);
            Texto(c, 376, 31, "COMPROBANTE", 15, true, 1, 1, 1, altoPagina);
            Texto(c, 376, 58, Acortar(tipoComprobante, 30), 9, true, 0.84, 0.94, 0.68, altoPagina);

           

            Tarjeta(c, 35, 166, 164, 58, altoPagina);
            Tarjeta(c, 210, 166, 164, 58, altoPagina);
            Tarjeta(c, 385, 166, 175, 58, altoPagina);
            Texto(c, 47, 176, "REFERENCIA", 8, true, 0.40, 0.47, 0.40, altoPagina);
            Texto(c, 47, 192, "N. " + Acortar(referencia, 18), 12, true, 0.15, 0.34, 0.21, altoPagina);
            Texto(c, 222, 176, "MESA", 8, true, 0.40, 0.47, 0.40, altoPagina);
            Texto(c, 222, 192, Acortar(mesa, 18), 12, true, 0.15, 0.34, 0.21, altoPagina);
            Texto(c, 397, 176, "FECHA Y HORA", 8, true, 0.40, 0.47, 0.40, altoPagina);
            Texto(c, 397, 192, Acortar(fecha, 23), 8.5, true, 0.15, 0.34, 0.21, altoPagina);

            Texto(c, 35, 241, "DATOS DEL CLIENTE", 10, true, 0.15, 0.34, 0.21, altoPagina);
            Rect(c, 35, 258, ancho - margen * 2, 70, 0.98, 0.99, 0.97, altoPagina);
            BordeRect(c, 35, 258, ancho - margen * 2, 70, 0.81, 0.86, 0.75, altoPagina);
            Texto(c, 47, 268, "NOMBRE / RAZON SOCIAL", 8, false, 0.40, 0.47, 0.40, altoPagina);
            Texto(c, 47, 284, Acortar(string.IsNullOrWhiteSpace(cliente) ? "Consumidor final" : cliente, 43), 10, true, 0.18, 0.22, 0.18, altoPagina);
            Texto(c, 390, 268, "DOCUMENTO", 8, false, 0.40, 0.47, 0.40, altoPagina);
            Texto(c, 390, 284, Acortar(string.IsNullOrWhiteSpace(documento) ? "No consignado" : documento, 21), 9, true, 0.18, 0.22, 0.18, altoPagina);

            Rect(c, 35, 345, ancho - margen * 2, 36, 0.92, 0.96, 0.86, altoPagina);
            Texto(c, 48, 357, "MEDIO DE PAGO", 8, true, 0.40, 0.47, 0.40, altoPagina);
            Texto(c, 145, 354, Acortar(string.IsNullOrWhiteSpace(medioPago) ? "No especificado" : medioPago, 35), 10, true, 0.15, 0.34, 0.21, altoPagina);

            Texto(c, 35, 397, "DETALLE DE CONSUMO", 10, true, 0.15, 0.34, 0.21, altoPagina);
            Rect(c, 35, 414, ancho - margen * 2, 24, 0.19, 0.40, 0.24, altoPagina);
            Texto(c, 47, 421, "DESCRIPCION", 8, true, 1, 1, 1, altoPagina);
            Texto(c, 484, 421, "IMPORTE", 8, true, 1, 1, 1, altoPagina);

            if (detalleItems == null || detalleItems.Length == 0)
            {
                DibujarFila(c, 35, 438, ancho - margen * 2, altoFila, "Detalle no disponible para este comprobante.", "", true, altoPagina);
            }
            else
            {
                for (int i = 0; i < detalleItems.Length; i++)
                {
                    string detalle = detalleItems[i] ?? "";
                    string descripcion = detalle.Trim();
                    string importe = "";
                    int posImporte = detalle.LastIndexOf("S/ ", StringComparison.OrdinalIgnoreCase);
                    if (posImporte >= 0)
                    {
                        descripcion = detalle.Substring(0, posImporte).Trim();
                        importe = "S/ " + detalle.Substring(posImporte + 3).Trim();
                    }
                    DibujarFila(c, 35, inicioFilas + i * altoFila, ancho - margen * 2, altoFila,
                        Acortar(descripcion, 64), Acortar(importe, 16), i % 2 == 0, altoPagina);
                }
            }

            Rect(c, 328, totalTop, 232, 55, 0.88, 0.94, 0.78, altoPagina);
            Texto(c, 344, totalTop + 11, "TOTAL", 10, true, 0.15, 0.34, 0.21, altoPagina);
            Texto(c, 344, totalTop + 28, "IMPORTE A PAGAR", 8, false, 0.35, 0.42, 0.35, altoPagina);
            Texto(c, 470, totalTop + 17, Acortar(total, 16), 15, true, 0.12, 0.29, 0.17, altoPagina);

            Linea(c, 35, pieTop, ancho - 35, 0.81, 0.86, 0.75, altoPagina);
            Texto(c, 35, pieTop + 10, "Gracias por tu visita.", 10, true, 0.15, 0.34, 0.21, altoPagina);
            Texto(c, 35, pieTop + 28, "Este documento es un registro interno de Good Moov.", 8, false, 0.40, 0.47, 0.40, altoPagina);

            GuardarPdf(ruta, c.ToString(), ancho, altoPagina);
        }

        static void DibujarFila(StringBuilder c, double x, double top, double ancho, double alto,
            string descripcion, string importe, bool alternada, double altoPagina)
        {
            if (alternada) Rect(c, x, top, ancho, alto, 0.97, 0.98, 0.95, altoPagina);
            Linea(c, x, top + alto, x + ancho, 0.86, 0.89, 0.82, altoPagina);
            Texto(c, x + 12, top + 6, Acortar(descripcion, 64), 9, false, 0.18, 0.22, 0.18, altoPagina);
            if (!string.IsNullOrWhiteSpace(importe))
            {
                double xImporte = Math.Max(x + ancho - 80, x + ancho - 16 - importe.Length * 5.1);
                Texto(c, xImporte, top + 6, importe, 9, true, 0.18, 0.22, 0.18, altoPagina);
            }
        }

        static void Tarjeta(StringBuilder c, double x, double top, double ancho, double alto, double altoPagina)
        {
            Rect(c, x, top, ancho, alto, 0.98, 0.99, 0.97, altoPagina);
            BordeRect(c, x, top, ancho, alto, 0.81, 0.86, 0.75, altoPagina);
        }

        static string Acortar(string texto, int maximo)
        {
            texto = (texto ?? "").Replace('\r', ' ').Replace('\n', ' ').Trim();
            if (texto.Length <= maximo) return texto;
            if (maximo <= 3) return texto.Substring(0, maximo);
            return texto.Substring(0, maximo - 3) + "...";
        }

        static void Texto(StringBuilder c, double x, double top, string texto, double tamano, bool negrita,
            double r, double g, double b, double altoPagina)
        {
            double y = altoPagina - top - tamano;
            c.AppendFormat(CultureInfo.InvariantCulture,
                "BT /{0} {1:0.##} Tf {2:0.###} {3:0.###} {4:0.###} rg 1 0 0 1 {5:0.##} {6:0.##} Tm ({7}) Tj ET\n",
                negrita ? "F2" : "F1", tamano, r, g, b, x, y, Escapar(texto));
        }

        static void Rect(StringBuilder c, double x, double top, double ancho, double alto,
            double r, double g, double b, double altoPagina)
        {
            double y = altoPagina - top - alto;
            c.AppendFormat(CultureInfo.InvariantCulture, "{0:0.###} {1:0.###} {2:0.###} rg {3:0.##} {4:0.##} {5:0.##} {6:0.##} re f\n",
                r, g, b, x, y, ancho, alto);
        }

        static void BordeRect(StringBuilder c, double x, double top, double ancho, double alto,
            double r, double g, double b, double altoPagina)
        {
            double y = altoPagina - top - alto;
            c.AppendFormat(CultureInfo.InvariantCulture, "{0:0.###} {1:0.###} {2:0.###} RG 0.7 w {3:0.##} {4:0.##} {5:0.##} {6:0.##} re S\n",
                r, g, b, x, y, ancho, alto);
        }

        static void Linea(StringBuilder c, double x, double top, double x2,
            double r, double g, double b, double altoPagina)
        {
            double y = altoPagina - top;
            c.AppendFormat(CultureInfo.InvariantCulture, "{0:0.###} {1:0.###} {2:0.###} RG 0.65 w {3:0.##} {4:0.##} m {5:0.##} {6:0.##} l S\n",
                r, g, b, x, y, x2, y);
        }

        static void GuardarPdf(string ruta, string contenido, double ancho, double alto)
        {
            string stream = contenido;
            List<string> objetos = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                string.Format(CultureInfo.InvariantCulture,
                    "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {0:0.##} {1:0.##}] /Resources << /Font << /F1 4 0 R /F2 5 0 R >> >> /Contents 6 0 R >>", ancho, alto),
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>",
                "<< /Length " + Encoding.Latin1.GetByteCount(stream).ToString(CultureInfo.InvariantCulture) + " >>\nstream\n" + stream + "endstream"
            };

            StringBuilder pdf = new StringBuilder("%PDF-1.4\n");
            List<int> posiciones = new List<int> { 0 };
            for (int i = 0; i < objetos.Count; i++)
            {
                posiciones.Add(Encoding.Latin1.GetByteCount(pdf.ToString()));
                pdf.Append((i + 1).ToString(CultureInfo.InvariantCulture)).Append(" 0 obj\n")
                    .Append(objetos[i]).Append("\nendobj\n");
            }
            int inicioXref = Encoding.Latin1.GetByteCount(pdf.ToString());
            pdf.Append("xref\n0 ").Append((objetos.Count + 1).ToString(CultureInfo.InvariantCulture)).Append("\n")
                .Append("0000000000 65535 f \n");
            for (int i = 1; i < posiciones.Count; i++)
                pdf.Append(posiciones[i].ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
            pdf.Append("trailer << /Size ").Append((objetos.Count + 1).ToString(CultureInfo.InvariantCulture))
                .Append(" /Root 1 0 R >>\nstartxref\n").Append(inicioXref.ToString(CultureInfo.InvariantCulture)).Append("\n%%EOF");

            Directory.CreateDirectory(Path.GetDirectoryName(ruta) ?? ".");
            File.WriteAllBytes(ruta, Encoding.Latin1.GetBytes(pdf.ToString()));
        }
    }
}
