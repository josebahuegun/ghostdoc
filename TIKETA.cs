using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUPERMERKATUA
{
    public class TIKETA
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public decimal Kantitatea { get; set; }
        public decimal PrezioaKg { get; set; }
        public decimal Totala { get; set; }
        public int IdSaltzailea { get; set; }
        public int IdProduktua { get; set; }

        public TIKETA() { }
    }
}
