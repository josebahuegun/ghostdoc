using System;
using System.IO;

namespace SUPERMERKATUA
{
    /// <summary>
    /// A
    /// </summary>
    public class EXCELkudeatu
    {
        /// <summary>
        /// Erregistratus the specified XML path.
        /// </summary>
        /// <param name="xmlPath">The XML path.</param>
        /// <param name="rootPath">The root path.</param>
        public static void Erregistratu(string xmlPath, string rootPath)
        {
            try
            {
                Console.WriteLine("6. Excel (CSV) eguneratzen...");
                string csvPath = Path.Combine(rootPath, "Bidalketa_Erregistroa.csv");

                // XMLaren izena eta ordua lortu
                string xmlIzena = Path.GetFileName(xmlPath);
                string csvLerroa = $"{xmlIzena};{DateTime.Now}";

                // EZ BADA EXISTITZEN TITULUA JARRI
                if (!File.Exists(csvPath))
                {
                    File.AppendAllText(csvPath, "XML_Izena;Data_Ordua" + Environment.NewLine);
                }

                // LERRO BERRIA GEHITU
                File.AppendAllText(csvPath, csvLerroa + Environment.NewLine);

                Console.WriteLine("   -> Ondo: Bidalketa erregistratu da.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errorea Excel idaztean: " + ex.Message);
            }
        }
    }
}