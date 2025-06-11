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
    public class SearchController
    {
        // De window die we laten zien op het scherm
        public static Home Window { get; set; }
        

        // Start methode die wordt aangeroepen wanneer de zoek pagina opent.
        public void Start()
        {
            // Reset de zoekinfotekst
            SearchManager.SetSearchImageInfo("Zoek naar een foto op basis van datum en tijd.\nVoorbeeld: 'dinsdag 10:30' of '15:45'");
        }

        // Functie om de dag te extraheren uit een zoekopdracht
        private int? ExtractDayFromSearch(string search)
        {
            string searchLower = search.ToLower();
            
            if (searchLower.Contains("zondag")) return 0;
            if (searchLower.Contains("maandag")) return 1;
            if (searchLower.Contains("dinsdag")) return 2;
            if (searchLower.Contains("woensdag")) return 3;
            if (searchLower.Contains("donderdag")) return 4;
            if (searchLower.Contains("vrijdag")) return 5;
            if (searchLower.Contains("zaterdag")) return 6;
            
            return null;
        }
        
        // Functie om tijden te extraheren uit een zoekopdracht
        private Tuple<int?, int?> ExtractTimeFromSearch(string search)
        {
            try
            {
                // Zoek naar tijdsformaat zoals "10:30" of "10.30" of "10 30"
                string[] separators = new string[] { ":", ".", " " };
                
                foreach (string separator in separators)
                {
                    if (search.Contains(separator))
                    {
                        string[] parts = search.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < parts.Length - 1; i++)
                        {
                            int hours, minutes;
                            if (int.TryParse(parts[i], out hours) && int.TryParse(parts[i + 1], out minutes))
                            {
                                if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                                {
                                    return new Tuple<int?, int?>(hours, minutes);
                                }
                            }
                        }
                    }
                }
            }
            catch {}
            
            return new Tuple<int?, int?>(null, null);
        }
        
        // Functie om informatie uit de bestandsnaam te halen
        private string ExtractInfoFromFileName(string filePath)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string[] parts = fileName.Split('_');
                
                if (parts.Length >= 4)
                {
                    int hours = int.Parse(parts[0]);
                    int minutes = int.Parse(parts[1]);
                    int seconds = int.Parse(parts[2]);
                    string id = parts[3].Replace("id", "");
                    
                    return $"Foto ID: {id}\nTijd: {hours:D2}:{minutes:D2}:{seconds:D2}\nBestandsnaam: {Path.GetFileName(filePath)}";
                }
            }
            catch {}
            
            return "Geen informatie beschikbaar";
        }

        // Wordt uitgevoerd wanneer er op de Zoeken knop is geklikt
        public void SearchButtonClick()
        {
            string searchInput = SearchManager.GetSearchInput();
            
            if (string.IsNullOrWhiteSpace(searchInput))
            {
                MessageBox.Show("Voer een zoekopdracht in.");
                return;
            }
            
            // Extraheer dag en tijd uit de zoekopdracht
            int? day = ExtractDayFromSearch(searchInput);
            var timeResult = ExtractTimeFromSearch(searchInput);
            int? hours = timeResult.Item1;
            int? minutes = timeResult.Item2;
            
            // Als er geen dag is opgegeven, gebruik vandaag
            if (!day.HasValue)
            {
                day = (int)DateTime.Now.DayOfWeek;
            }
            
            // Als er geen tijd is opgegeven, toon een foutmelding
            if (!hours.HasValue || !minutes.HasValue)
            {
                MessageBox.Show("Geef een geldige tijd op, bijvoorbeeld: '10:30'");
                return;
            }
            
            // Zoek naar de foto's die overeenkomen met de zoekopdracht
            string folderPath = $"../../../fotos/{day}_";
            string foundFile = null;
            
            try
            {
                // Zoek de juiste map
                string matchingDir = null;
                foreach (string dir in Directory.GetDirectories("../../../fotos"))
                {
                    if (dir.Contains($"{day}_"))
                    {
                        matchingDir = dir;
                        break;
                    }
                }
                
                if (matchingDir != null)
                {
                    // Zoek in de geselecteerde map naar foto's rond de opgegeven tijd
                    int targetHours = hours.Value;
                    int targetMinutes = minutes.Value;
                    
                    int closestTimeDiff = int.MaxValue;
                    
                    foreach (string file in Directory.GetFiles(matchingDir))
                    {
                        try
                        {
                            string fileName = Path.GetFileNameWithoutExtension(file);
                            string[] parts = fileName.Split('_');
                            
                            if (parts.Length >= 3)
                            {
                                int fileHours = int.Parse(parts[0]);
                                int fileMinutes = int.Parse(parts[1]);
                                
                                // Bereken het tijdsverschil in minuten
                                int timeDiff = Math.Abs((fileHours * 60 + fileMinutes) - (targetHours * 60 + targetMinutes));
                                
                                // Als deze foto dichter bij de gezochte tijd is dan de vorige beste match
                                if (timeDiff < closestTimeDiff)
                                {
                                    closestTimeDiff = timeDiff;
                                    foundFile = file;
                                }
                            }
                        }
                        catch {}
                    }
                    
                    if (foundFile != null)
                    {
                        // Toon de gevonden foto
                        SearchManager.SetPicture(foundFile);
                        
                        // Toon de informatie van de foto
                        string photoInfo = ExtractInfoFromFileName(foundFile);
                        SearchManager.SetSearchImageInfo(photoInfo);
                    }
                    else
                    {
                        MessageBox.Show("Geen foto's gevonden rond de opgegeven tijd.");
                    }
                }
                else
                {
                    MessageBox.Show($"Geen foto's gevonden voor de opgegeven dag ({day}).");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij zoeken naar foto's: {ex.Message}");
            }
        }
    }
}
