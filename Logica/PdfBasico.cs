using System;
using System.IO;
using System.Text;

namespace GDImproved.Logica
{
    public class PdfBasico
    {
        static string Escapar(string texto)
        {
            string limpio = "";
            for (int i = 0; i < texto.Length; i++)
            {
                char letra = texto[i];
                if (letra > 255 || letra == '\r' || letra == '\n') letra = '?';
                if (letra == '\\' || letra == '(' || letra == ')') limpio += "\\";
                limpio += letra;
            }
            return limpio;
        }
        public static void Crear(string ruta, string[] lineas, int cantidad)
        {
            string contenido = "BT /F1 11 Tf 45 785 Td 15 TL\n";
            int maximo = cantidad;
            if (maximo > 43) maximo = 43;
            for (int i = 0; i < maximo; i++)
            {
                if (i > 0) contenido += "T*\n";
                contenido += "(" + Escapar(lineas[i]) + ") Tj\n";
            }
            contenido += "ET\n";
            string pdf = "%PDF-1.4\n";
            int[] posiciones = new int[6];
            posiciones[1] = pdf.Length;
            pdf += "1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj\n";
            posiciones[2] = pdf.Length;
            pdf += "2 0 obj << /Type /Pages /Kids [3 0 R] /Count 1 >> endobj\n";
            posiciones[3] = pdf.Length;
            pdf += "3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >> endobj\n";
            posiciones[4] = pdf.Length;
            pdf += "4 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >> endobj\n";
            posiciones[5] = pdf.Length;
            pdf += "5 0 obj << /Length " + contenido.Length + " >> stream\n";
            pdf += contenido + "endstream endobj\n";
            int inicio = pdf.Length;
            pdf += "xref\n0 6\n0000000000 65535 f \n";
            for (int i = 1; i <= 5; i++) pdf += posiciones[i].ToString("D10") + " 00000 n \n";
            pdf += "trailer << /Size 6 /Root 1 0 R >>\nstartxref\n" + inicio + "\n%%EOF";
            File.WriteAllText(ruta, pdf, Encoding.Latin1);
        }
    }
}
