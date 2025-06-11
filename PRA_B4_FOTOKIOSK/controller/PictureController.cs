using PRA_B4_FOTOKIOSK.magie;
using PRA_B4_FOTOKIOSK.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRA_B4_FOTOKIOSK.controller
{
    public class PictureController
    {
        // De window die we laten zien op het scherm
        public static Home Window { get; set; }


        // De lijst met fotos die we laten zien
        public List<KioskPhoto> PicturesToDisplay = new List<KioskPhoto>();
        
        
        // Start methode die wordt aangeroepen wanneer de foto pagina opent.
        public void Start()
        {
            RefreshButtonClick();
        }

        // Functie om te controleren of een foto van vandaag is
        private bool IsFromToday(string directoryPath)
        {
            try
            {
                // Huidige dag ophalen (0 = Zondag t/m 6 = Zaterdag)
                var now = DateTime.Now;
                int today = (int)now.DayOfWeek;
                
                // Mapnaam ophalen en dag extraheren (bijv. "2_Dinsdag" -> "2")
                string folderName = Path.GetFileName(directoryPath);
                string dayNumber = folderName.Split('_')[0];
                
                // Controleren of het dagnummer overeenkomt met vandaag
                return int.Parse(dayNumber) == today;
            }
            catch
            {
                return false;
            }
        }
        
        // Functie om te controleren of een foto tussen 2 en 30 minuten geleden gemaakt is
        private bool IsWithinTimeRange(string filePath)
        {
            try
            {
                // Bestandsnaam ophalen zonder pad en extensie
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                
                // Split op underscore en haal de tijd eruit (bijv. "10_05_30_id8824" -> ["10", "05", "30", "id8824"])
                string[] parts = fileName.Split('_');
                
                if (parts.Length >= 3)
                {
                    // Tijd uit bestandsnaam halen
                    int hours = int.Parse(parts[0]);
                    int minutes = int.Parse(parts[1]);
                    int seconds = int.Parse(parts[2]);
                    
                    // Huidige tijd ophalen
                    DateTime now = DateTime.Now;
                    
                    // Tijd van foto berekenen
                    DateTime photoTime = new DateTime(now.Year, now.Month, now.Day, hours, minutes, seconds);
                    
                    // Bereken het tijdsverschil in minuten
                    TimeSpan difference = now - photoTime;
                    double minutesAgo = difference.TotalMinutes;
                    
                    // Controleer of de foto tussen 2 en 30 minuten geleden is genomen
                    return minutesAgo >= 2 && minutesAgo <= 30;
                }
            }
            catch
            {
                // Bij fouten, foto niet tonen
            }
            
            return false;
        }
        
        // Functie om foto's te zoeken die 60 seconden van elkaar zijn genomen
        private void FindRelatedPictures()
        {
            List<KioskPhoto> sortedPhotos = new List<KioskPhoto>();
            Dictionary<string, List<KioskPhoto>> photoGroups = new Dictionary<string, List<KioskPhoto>>();
            
            foreach (KioskPhoto photo in PicturesToDisplay)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(photo.Source);
                    string[] parts = fileName.Split('_');
                    
                    if (parts.Length >= 4)
                    {
                        // Haal het ID uit de bestandsnaam (bijv. "10_05_30_id8824" -> "8824")
                        string idPart = parts[3];
                        string carId = idPart.Replace("id", "");
                        
                        // Groepeer foto's op dezelfde ID
                        if (!photoGroups.ContainsKey(carId))
                        {
                            photoGroups[carId] = new List<KioskPhoto>();
                        }
                        
                        photoGroups[carId].Add(photo);
                    }
                }
                catch
                {
                    // Bij fouten, foto normaal toevoegen
                    sortedPhotos.Add(photo);
                }
            }
            
            // Nu verwerk de groepen en voeg ze toe aan de gesorteerde lijst
            foreach (var group in photoGroups)
            {
                // Voeg alle foto's met dezelfde ID achter elkaar toe
                sortedPhotos.AddRange(group.Value);
            }
            
            // Update de lijst met gesorteerde foto's
            PicturesToDisplay = sortedPhotos;
        }

        // Wordt uitgevoerd wanneer er op de Refresh knop is geklikt
        public void RefreshButtonClick()
        {
            // Leeg de huidige lijst
            PicturesToDisplay.Clear();
            
            // Laad foto's van de juiste dag en tijd
            try
            {
                foreach (string dir in Directory.GetDirectories(@"../../../fotos"))
                {
                    // Controleer of de map overeenkomt met vandaag
                    if (IsFromToday(dir))
                    {
                        foreach (string file in Directory.GetFiles(dir))
                        {
                            // Controleer of de foto binnen het juiste tijdsbereik valt (2-30 minuten geleden)
                            if (IsWithinTimeRange(file))
                            {
                                int id = 0;
                                try
                                {
                                    // Probeer de ID uit de bestandsnaam te halen
                                    string fileName = Path.GetFileNameWithoutExtension(file);
                                    string[] parts = fileName.Split('_');
                                    if (parts.Length >= 4)
                                    {
                                        string idPart = parts[3].Replace("id", "");
                                        id = int.Parse(idPart);
                                    }
                                }
                                catch { }
                                
                                PicturesToDisplay.Add(new KioskPhoto() { Id = id, Source = file });
                            }
                        }
                    }
                }
                
                // Zoek gerelateerde foto's (60 seconden verschil, zelfde ID)
                FindRelatedPictures();
                
                // Update de UI met de gefilterde foto's
                PictureManager.UpdatePictures(PicturesToDisplay);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij laden van foto's: {ex.Message}");
            }
        }
    }
}
