using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRA_B4_FOTOKIOSK.magie;

namespace PRA_B4_FOTOKIOSK.models
{
    public class KioskProduct
    {

        public string Name { get; set; }

        public int Prijs { get; set; }
        public string Beschrijving { get; set; }

        public void something()
        {
            foreach (KioskProduct product in ShopManager.Products)
            {
                ShopManager.SetShopPriceList(“”);
                ShopManager.AddShopPriceList(“”);
            }
        }
    }
}
