using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class ShopController
    {
        public static Home Window { get; set; }
        private List<OrderedProduct> orderedProducts = new List<OrderedProduct>();
        private decimal totaalBedrag = 0;

        public void Start()
        {
            // Stel de producten in
            ShopManager.Products.Clear();
            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 10x15", Price = 2.55m, Description = "Standaard foto afdruk 10x15 cm" });
            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 15x20", Price = 3.95m, Description = "Grotere foto afdruk 15x20 cm" });
            ShopManager.Products.Add(new KioskProduct() { Name = "Foto 20x30", Price = 5.50m, Description = "Grote foto afdruk 20x30 cm" });
            ShopManager.Products.Add(new KioskProduct() { Name = "Sleutelhanger", Price = 4.95m, Description = "Foto sleutelhanger" });
            ShopManager.Products.Add(new KioskProduct() { Name = "Mok met foto", Price = 7.95m, Description = "Keramische mok met foto" });
            
            // Update dropdown met producten
            ShopManager.UpdateDropDownProducts();
            
            // Genereer de prijslijst
            UpdatePriceList();
            
            // Reset de bon
            ResetReceipt();
        }
        
        private void UpdatePriceList()
        {
            // Reset de prijslijst
            ShopManager.SetShopPriceList("Prijslijst:\n\n");
            
            // Loop door alle producten heen en voeg ze toe aan de prijslijst
            foreach (KioskProduct product in ShopManager.Products)
            {
                ShopManager.AddShopPriceList($"{product.Name}: €{product.Price:F2} - {product.Description}\n");
            }
        }
        
        private void ResetReceipt()
        {
            // Reset de variabelen
            orderedProducts.Clear();
            totaalBedrag = 0;
            
            // Reset de kassabon
            ShopManager.SetShopReceipt("Eindbedrag: €0.00\n\n");
        }
        
        private void UpdateReceipt()
        {
            // Reset huidige bon maar behoud de bestelde producten
            ShopManager.SetShopReceipt($"Eindbedrag: €{totaalBedrag:F2}\n\n");
            
            // Voeg alle producten toe aan de kassabon
            foreach (OrderedProduct product in orderedProducts)
            {
                ShopManager.AddShopReceipt($"Foto #{product.FotoNummer} - {product.ProductNaam} x{product.Aantal} = €{product.Totaalprijs:F2}\n");
            }
        }

        // Wordt uitgevoerd wanneer er op de Toevoegen knop is geklikt
        public void AddButtonClick()
        {
            // Controleer of alle velden zijn ingevuld
            KioskProduct selectedProduct = ShopManager.GetSelectedProduct();
            int? fotoId = ShopManager.GetFotoId();
            int? aantal = ShopManager.GetAmount();
            
            if (selectedProduct == null || !fotoId.HasValue || !aantal.HasValue || aantal.Value <= 0)
            {
                MessageBox.Show("Vul alle velden correct in.");
                return;
            }
            
            // Bereken de totaalprijs voor dit product
            decimal productTotaal = selectedProduct.Price * aantal.Value;
            
            // Voeg het product toe aan de lijst
            OrderedProduct orderedProduct = new OrderedProduct
            {
                FotoNummer = fotoId.Value,
                ProductNaam = selectedProduct.Name,
                Aantal = aantal.Value,
                Totaalprijs = productTotaal
            };
            
            orderedProducts.Add(orderedProduct);
            
            // Update het totaalbedrag
            totaalBedrag += productTotaal;
            
            // Update de kassabon
            UpdateReceipt();
        }

        // Wordt uitgevoerd wanneer er op de Resetten knop is geklikt
        public void ResetButtonClick()
        {
            ResetReceipt();
        }

        // Wordt uitgevoerd wanneer er op de Save knop is geklikt
        public void SaveButtonClick()
        {
            if (orderedProducts.Count == 0)
            {
                MessageBox.Show("Er zijn geen producten om op te slaan.");
                return;
            }
            
            try
            {
                // Maak een map voor bonnen als deze nog niet bestaat
                string bonnenMap = "bonnen";
                if (!Directory.Exists(bonnenMap))
                {
                    Directory.CreateDirectory(bonnenMap);
                }
                
                // Genereer een unieke bestandsnaam met tijdstip
                string bestandsnaam = $"{bonnenMap}/bon_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                
                // Maak de inhoud van de bon
                StringBuilder bonInhoud = new StringBuilder();
                bonInhoud.AppendLine("FOTOKIOSK KASSABON");
                bonInhoud.AppendLine($"Datum en tijd: {DateTime.Now}");
                bonInhoud.AppendLine("============================");
                
                foreach (OrderedProduct product in orderedProducts)
                {
                    bonInhoud.AppendLine($"Foto #{product.FotoNummer} - {product.ProductNaam}");
                    bonInhoud.AppendLine($"Aantal: {product.Aantal}");
                    bonInhoud.AppendLine($"Prijs: €{product.Totaalprijs:F2}");
                    bonInhoud.AppendLine("----------------------------");
                }
                
                bonInhoud.AppendLine($"TOTAALBEDRAG: €{totaalBedrag:F2}");
                bonInhoud.AppendLine("============================");
                bonInhoud.AppendLine("Bedankt voor uw aankoop!");
                
                // Sla de bon op
                File.WriteAllText(bestandsnaam, bonInhoud.ToString());
                
                MessageBox.Show($"Bon succesvol opgeslagen als: {bestandsnaam}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij opslaan van bon: {ex.Message}");
            }
        }
    }
}
