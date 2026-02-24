using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;

namespace SUPERMERKATUA
{
    /// <summary>
    /// 
    /// </summary>
    public class FitxategiaKargatu
    {
        // Karpeta nagusia
        /// <summary>
        /// The root path
        /// </summary>
        static string rootPath = @"C:\TicketBAI";
        /// <summary>
        /// The backup path
        /// Backup non dagoen
        static string backupPath = Path.Combine(rootPath, "BACKUP_HISTORIAL");

        /// <summary>Datuaks the inportatu.</summary>
        /// <param name="connectionString">The connection string.</param>
        public static void DatuakInportatu(string connectionString)
        {
            // Karpetak sortu
            SortuKarpetak();

            List<TIKETA> tiketaGuztiak = new List<TIKETA>();
            List<string> prozesatutakoFitxategiak = new List<string>();


            // 0. PRODUKTUEN KATALOGOA KARGATU DATU BASETIK

            Console.WriteLine("0. Produktuen katalogoa Datu Basetik kargatzen...");
            Dictionary<string, int> dbProduktuMapaketa = new Dictionary<string, int>();

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT izena, id_produktua FROM produktua";
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            string dbIzena = rdr["izena"].ToString().ToLower().Trim();
                            int dbId = Convert.ToInt32(rdr["id_produktua"]);
                            if (!dbProduktuMapaketa.ContainsKey(dbIzena))
                            {
                                dbProduktuMapaketa.Add(dbIzena, dbId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROREA KONEXIOAN: " + ex.Message);
                    return;
                }
            }


            // 1. FITXATEGIAK IRAKURRI

            Console.WriteLine("1. Fitxategiak bilatzen...");

            try
            {
                string[] fitxategiGuztiak = Directory.GetFiles(rootPath, "*.txt", SearchOption.AllDirectories);
                int nireId = 1;

                foreach (string fitxategia in fitxategiGuztiak)
                {
                    if (fitxategia.Contains("BACKUP_HISTORIAL")) continue;

                    try
                    {
                        string[] lerroak = File.ReadAllLines(fitxategia);
                        bool fitxategiaOndo = false;

                        string izenGarbia = Path.GetFileNameWithoutExtension(fitxategia);
                        DateTime fitxategiData;
                        bool dataOndo = DateTime.TryParseExact(izenGarbia, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out fitxategiData);
                        if (!dataOndo) fitxategiData = DateTime.Now;

                        if (lerroak.Length > 0)
                        {
                            string lerroa = lerroak[0];
                            string[] zatiak = lerroa.Split('$');

                            if (zatiak.Length >= 5)
                            {
                                TIKETA t = new TIKETA();
                                t.Id = nireId;
                                nireId++;
                                t.Data = fitxategiData;

                                // PRODUKTUA IDENTIFIKATU
                                string izenaFitxategian = zatiak[0].ToLower().Trim();

                                if (dbProduktuMapaketa.ContainsKey(izenaFitxategian))
                                {
                                    t.IdProduktua = dbProduktuMapaketa[izenaFitxategian];
                                }
                                else
                                {
                                    int idAurkitua = 1;
                                    foreach (var item in dbProduktuMapaketa)
                                    {
                                        if (izenaFitxategian.Contains(item.Key))
                                        {
                                            idAurkitua = item.Value;
                                            break;
                                        }
                                    }
                                    t.IdProduktua = idAurkitua;
                                }

                                // SALTZAILEA
                                string saltzaileTestua = zatiak[1].ToLower().Trim();

                                if (saltzaileTestua == "autosalmenta" || saltzaileTestua == "0")
                                {
                                    t.IdSaltzailea = 0;
                                }
                                else if (int.TryParse(saltzaileTestua, out int idFitxategitik))
                                {
                                    t.IdSaltzailea = idFitxategitik;
                                }
                                else
                                {
                                    t.IdSaltzailea = 0;
                                }

                                t.PrezioaKg = decimal.Parse(zatiak[2]);
                                t.Kantitatea = decimal.Parse(zatiak[3]);
                                t.Totala = decimal.Parse(zatiak[4]);

                                tiketaGuztiak.Add(t);
                                fitxategiaOndo = true;
                            }
                        }

                        if (fitxategiaOndo)
                        {
                            prozesatutakoFitxategiak.Add(fitxategia);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errorea fitxategiak bilatzean: " + ex.Message);
            }

            if (tiketaGuztiak.Count == 0)
            {
                Console.WriteLine("Ez da daturik prozesatu.");
                Console.ReadKey();
                return;
            }


            // 2. ERABILTZAILEAREN ALDAKETA 

            var aldatzekoZerrenda = tiketaGuztiak.Where(t =>
                t.IdSaltzailea == 0 && (
                    t.IdProduktua >= 7 && t.IdProduktua <= 18
                )).ToList();

            if (aldatzekoZerrenda.Count > 0)
            {
                // saltzaileak kargatu db tik erakusteko
                Dictionary<int, string> saltzaileZerrenda = new Dictionary<int, string>();
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand("SELECT id_saltzailea, izena FROM saltzailea", conn);
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                saltzaileZerrenda.Add(Convert.ToInt32(rdr["id_saltzailea"]), rdr["izena"].ToString());
                            }
                        }
                    }
                    catch { } // Akatsa badago, ez du zerrenda erakutsiko 
                }

                Console.WriteLine($"\n⚠️ ADI: {aldatzekoZerrenda.Count} tiket aurkitu dira HARATEGIAN edo TXARKUTERIAN saltzailerik gabe.");
                Console.Write("Nahi dituzu tiket hauek BANAKA esleitu? (Bai/Ez): ");
                string erantzuna = Console.ReadLine().ToUpper();

                if (erantzuna.StartsWith("B") || erantzuna.StartsWith("Y"))
                {
                    int kont = 1;
                    foreach (var tiketa in aldatzekoZerrenda)
                    {
                        Console.Clear();

                        // TAULA ERAKUTSI
                        Console.WriteLine("--------------------------------");
                        Console.WriteLine(" ID  |  IZENA");
                        Console.WriteLine("--------------------------------");
                        foreach (var s in saltzaileZerrenda)
                        {
                            Console.WriteLine($" {s.Key.ToString().PadRight(3)} |  {s.Value}");
                        }
                        Console.WriteLine("--------------------------------\n");

                        string saila = "EZEZAGUNA";
                        if (tiketa.IdProduktua >= 7 && tiketa.IdProduktua <= 12) saila = "TXARKUTEGIA";
                        if (tiketa.IdProduktua >= 13 && tiketa.IdProduktua <= 18) saila = "HARATEGIA";

                        Console.WriteLine($"TIKETA KUDEATZEN: {kont} / {aldatzekoZerrenda.Count}");
                        Console.WriteLine($"BASKULA: {saila} | PRODUKTU ID: {tiketa.IdProduktua} | TOTALA: {tiketa.Totala}€");
                        Console.Write("Sartu Saltzailearen IDa (Utzi hutsik '0' izateko): ");

                        string sarrera = Console.ReadLine();
                        if (int.TryParse(sarrera, out int idBerria))
                        {
                            tiketa.IdSaltzailea = idBerria;
                        }
                        kont++;
                    }
                }
            }


            // 3. XML SORTU ETA BALIDATU 

            string xmlPath = XMLsortu.Sortu(tiketaGuztiak, rootPath);
            if (string.IsNullOrEmpty(xmlPath)) return;

            Console.WriteLine("4. XML Balidatzen...");
            if (!XMLsortu.Balidatu(xmlPath, rootPath))
            {
                Console.WriteLine("ERROREA: XMLak ez du balidazioa pasatu. Prozesua gelditzen.");
                Console.ReadKey();
                return;
            }

            // 5. EMAILA BIDALI
            
            Console.WriteLine("5. Emaila bidaltzen...");
            KudeatuEmail.Bidali(xmlPath);

            // 6. EXCEL LOG 

            EXCELkudeatu.Erregistratu(xmlPath, rootPath);
            
            // 7. DATU BASEAN GORDE
            
            Console.WriteLine("7. Datu basean gordetzen...");
            bool dbOndo = false;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO ticketa (data, kantitatea, prezioakg, totala, id_saltzailea, id_produktua, id_baskula) VALUES (@data, @kantitatea, @prezioakg, @totala, @idsaltzailea, @idproduktua, @idbaskula)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        foreach (TIKETA t in tiketaGuztiak)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@data", t.Data);
                            cmd.Parameters.AddWithValue("@kantitatea", t.Kantitatea);
                            cmd.Parameters.AddWithValue("@prezioakg", t.PrezioaKg);
                            cmd.Parameters.AddWithValue("@totala", t.Totala);
                            cmd.Parameters.AddWithValue("@idsaltzailea", t.IdSaltzailea);
                            cmd.Parameters.AddWithValue("@idproduktua", t.IdProduktua);

                            // BASKULA LOGIKA
                            int baskulaID = 1;
                            if (t.IdProduktua >= 1 && t.IdProduktua <= 6) baskulaID = 1;
                            else if (t.IdProduktua >= 7 && t.IdProduktua <= 12) baskulaID = 2;
                            else if (t.IdProduktua >= 13 && t.IdProduktua <= 18) baskulaID = 3;
                            else if (t.IdProduktua >= 19) baskulaID = 4;

                            cmd.Parameters.AddWithValue("@idbaskula", baskulaID);
                            cmd.ExecuteNonQuery();
                        }
                        Console.WriteLine("   -> Datuak ondo gorde dira!");
                        dbOndo = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROREA DATU BASEAN: " + ex.Message);
                    dbOndo = false;
                }
            }

            // 8. BACKUP

            if (dbOndo)
            {
                Console.WriteLine("Fitxategiak Backup karpetara mugitzen...");
                foreach (string f in prozesatutakoFitxategiak)
                {
                    try
                    {
                        string izena = Path.GetFileName(f);
                        string destinoa = Path.Combine(backupPath, izena);
                        if (File.Exists(destinoa)) File.Delete(destinoa);
                        File.Move(f, destinoa);
                    }
                    catch { }
                }
            }

            Console.WriteLine("\nPROZESUA AMAITUTA. Sakatu tekla bat.");
            Console.ReadKey();
        }

        /// <summary>
        /// Sortus the karpetak.
        /// </summary>
        private static void SortuKarpetak()
        {
            if (!Directory.Exists(rootPath)) Directory.CreateDirectory(rootPath);
            if (!Directory.Exists(backupPath)) Directory.CreateDirectory(backupPath);
        }
    }
}