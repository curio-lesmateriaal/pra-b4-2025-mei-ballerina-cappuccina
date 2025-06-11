using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRA_B4_FOTOKIOSK.models
{
    public class OrderedProduct
    {
        public int FotoNummer { get; set; }
        public string ProductNaam { get; set; }
        public int Aantal { get; set; }
        public decimal Totaalprijs { get; set; }
    }
} 