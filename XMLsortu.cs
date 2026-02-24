using System;
using System.Collections.Generic;
using System.IO; // Directory eta Path erabiltzeko
using System.Xml.Serialization;
using System.Xml; // XmlReader erabiltzeko

namespace SUPERMERKATUA
{
    public class XMLsortu
    {
        //rootPath jasotzen du parametro bezala
        public static string Sortu(List<TIKETA> zerrenda, string rootPath)
        {
            Console.WriteLine("   -> XML sortzen...");

            // 1. XSD ESKEMA SORTU (Balidaziorako beharrezkoa)
            SortuXSD(rootPath);

            // 2. IZENA ETA DATA
            string xmlIzena = $"TicketBAI_Bidalketa_{DateTime.Now:yyyyMMdd_HHmmss}.xml";
            string bidea = Path.Combine(rootPath, xmlIzena);

            try
            {
                XmlSerializer serializadorea = new XmlSerializer(typeof(List<TIKETA>));
                using (TextWriter writer = new StreamWriter(bidea))
                {
                    serializadorea.Serialize(writer, zerrenda);
                }

                Console.WriteLine($"      Eginda: {xmlIzena}");
                return bidea; // Fitxategiaren helbidea itzultzen du
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROREA XML sortzean: " + ex.Message);
                return null;
            }
        }

        //  XMLa balidatzeko
        public static bool Balidatu(string xmlPath, string rootPath)
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas.Add(null, Path.Combine(rootPath, "eskema.xsd"));
                settings.ValidationType = ValidationType.Schema;

                using (XmlReader reader = XmlReader.Create(xmlPath, settings))
                {
                    while (reader.Read()) { } // Irakurri fitxategi osoa ea errorerik dagoen
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("XSD ERROREA: " + ex.Message);
                return false;
            }
        }

        //XSD fitxategia sortzeko
        private static void SortuXSD(string rootPath)
        {
            string xsdTestua = @"<?xml version=""1.0"" encoding=""utf-8""?>
<xs:schema elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""ArrayOfTIKETA"">
    <xs:complexType>
      <xs:sequence>
        <xs:element minOccurs=""0"" maxOccurs=""unbounded"" name=""TIKETA"">
          <xs:complexType>
            <xs:sequence>
              <xs:element minOccurs=""1"" maxOccurs=""1"" name=""Id"" type=""xs:int"" />
              <xs:element minOccurs=""1"" maxOccurs=""1"" name=""Data"" type=""xs:dateTime"" />
              <xs:element minOccurs=""1"" maxOccurs=""1"" name=""Kantitatea"" type=""xs:decimal"" />
              <xs:element minOccurs=""1"" maxOccurs=""1"" name=""PrezioaKg"" type=""xs:decimal"" />
              <xs:element minOccurs=""1"" maxOccurs=""1"" name=""Totala"" type=""xs:decimal"" />
              <xs:element minOccurs=""1"" maxOccurs=""1"" name=""IdSaltzailea"" type=""xs:int"" />
              <xs:element minOccurs=""1"" maxOccurs=""1"" name=""IdProduktua"" type=""xs:int"" />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>";
            try
            {
                File.WriteAllText(Path.Combine(rootPath, "eskema.xsd"), xsdTestua);
            }
            catch { }
        }
    }
}