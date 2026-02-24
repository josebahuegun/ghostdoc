using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUPERMERKATUA
{
    /// <summary>
    /// ESTADITIKAK KLASEA
    /// </summary>
    public class ESTADISTIKAK
    {
        // 1. GEHIEN SALDU DEN PRODUKTUA
        /// <summary>
        /// Gehiens the saldu den produktua.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public static void GehienSalduDenProduktua(string connectionString)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT p.izena, SUM(t.kantitatea) as Guztira 
                        FROM ticketa t 
                        JOIN produktua p ON t.id_produktua = p.id_produktua 
                        GROUP BY p.izena 
                        ORDER BY Guztira DESC 
                        LIMIT 1";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            Console.WriteLine($"\n★ GEHIEN SALDU DEN PRODUKTUA: {rdr["izena"]} ({rdr["Guztira"]} unitate/kg)");
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Errorea: " + ex.Message); }
            }
        }

        // 2. GUTXIEN SALDU DEN PRODUKTUA
        /// <summary>
        /// Gutxiens the saldu den produktua.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public static void GutxienSalduDenProduktua(string connectionString)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT p.izena, SUM(t.kantitatea) as Guztira 
                        FROM ticketa t 
                        JOIN produktua p ON t.id_produktua = p.id_produktua 
                        GROUP BY p.izena 
                        ORDER BY Guztira ASC 
                        LIMIT 1";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            Console.WriteLine($"\n☹ GUTXIEN SALDU DEN PRODUKTUA: {rdr["izena"]} ({rdr["Guztira"]} unitate/kg)");
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Errorea: " + ex.Message); }
            }
        }
        
        // 3. GEHIEN SALDU DUEN SALTZAILEA
        /// <summary>
        /// Gehien saldu den saltzailea.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public static void GehienSalduDuenSaltzailea(string connectionString)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT s.izena, SUM(t.totala) as Dirua 
                        FROM ticketa t 
                        JOIN saltzailea s ON t.id_saltzailea = s.id_saltzailea
                        GROUP BY s.izena 
                        ORDER BY Dirua DESC 
                        LIMIT 1";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            Console.WriteLine($"\n★ SALTZAILE ONENA: {rdr["izena"]} - Irabaziak: {rdr["Dirua"]}€");
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Errorea: " + ex.Message); }
            }
        }

        // 4. GUTXIEN SALDU DUEN SALTZAILEA 
        /// <summary>
        /// Gutxiens the saldu duen saltzailea.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public static void GutxienSalduDuenSaltzailea(string connectionString)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT s.izena, IFNULL(SUM(t.totala), 0) as Dirua 
                        FROM saltzailea s 
                        LEFT JOIN ticketa t ON s.id_saltzailea = t.id_saltzailea
                        GROUP BY s.id_saltzailea, s.izena
                        ORDER BY Dirua ASC 
                        LIMIT 1";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            Console.WriteLine($"\n☹ SALTZAILE TXARRENA: {rdr["izena"]} - Irabaziak: {rdr["Dirua"]}€");
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Errorea: " + ex.Message); }
            }
        }

        // 5. GEHIEN SALDU DEN EGUNA
        /// <summary>
        /// Gehiens the saldu den eguna.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public static void GehienSalduDenEguna(string connectionString)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"
                        SELECT DATE(data) as Eguna, SUM(totala) as Dirua 
                        FROM ticketa 
                        GROUP BY DATE(data) 
                        ORDER BY Dirua DESC 
                        LIMIT 1";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            DateTime eguna = Convert.ToDateTime(rdr["Eguna"]);
                            Console.WriteLine($"\n📅 EGUN ONENA: {eguna.ToShortDateString()} - Salmentak: {rdr["Dirua"]}€");
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Errorea: " + ex.Message); }
            }
        }

        // 6. SALTZAILE BAKOITZAK ZENBAT SALDU DUEN (DENAK AGERTZEKO)
        /// <summary>
        /// Saldutakoas the saltzaileko.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public static void SaldutakoaSaltzaileko(string connectionString)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = @"
                        SELECT s.izena, IFNULL(SUM(t.totala), 0) as Dirua 
                        FROM saltzailea s
                        LEFT JOIN ticketa t ON s.id_saltzailea = t.id_saltzailea
                        GROUP BY s.id_saltzailea, s.izena
                        ORDER BY s.id_saltzailea ASC";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        Console.WriteLine("\n--- SALTZAILE BAKOITZA (GUZTIAK) ---");
                        while (rdr.Read())
                        {
                            Console.WriteLine($"Saltzailea: {rdr["izena"]} -> {rdr["Dirua"]}€");
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Errorea: " + ex.Message); }
            }
        }
    }
}