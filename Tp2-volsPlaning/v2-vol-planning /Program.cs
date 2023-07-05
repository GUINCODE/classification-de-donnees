using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
public class Solution
{
    public Dictionary<string, Flight>? MeuilleurVolPourChaqueParticipantAller { get; set; }
    public Dictionary<string, Flight>? MeuilleurVolPourChaqueParticipantRetour { get; set; }

    public decimal CoutAller => MeuilleurVolPourChaqueParticipantAller?.Sum(x => x.Value.GetCostWithWaitingTimeAller()) ?? 0;
    public decimal CoutRetour => MeuilleurVolPourChaqueParticipantRetour?.Sum(x => x.Value.GetCostWithWaitingTimeRetour()) ?? 0;
    public decimal Cout => CoutAller + CoutRetour;
    public void Print()
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("---- Best Solution ---- :");
        Console.ResetColor();

        // Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"Coût total de la solution : {Cout} $");
        // Console.ResetColor();
        Console.WriteLine($"Coût total Aller : {CoutAller} $");
        Console.WriteLine($"Coût total Retour : {CoutRetour} $");
        Console.WriteLine();

        if (MeuilleurVolPourChaqueParticipantAller != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Vols Aller ({MeuilleurVolPourChaqueParticipantAller.Count} vols) :");
            Console.ResetColor();

            foreach (var (participant, flight) in MeuilleurVolPourChaqueParticipantAller)
            {
                Console.WriteLine($"{participant} : {flight.Origin} - {flight.Destination}: date d'arrivée: {flight.Arrival}, - Cout du vol: {flight.GetCostWithWaitingTimeAller()} $, soit (Prix: {flight.Price} $  et Temps d'attente: {(flight.Temps_Attente_Aller > 30 ? flight.Temps_Attente_Aller - 30 : 0)} min)");
            }

            Console.WriteLine();
        }

        if (MeuilleurVolPourChaqueParticipantRetour != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Vols Retour ({MeuilleurVolPourChaqueParticipantRetour.Count} vols) :");
            Console.ResetColor();

            foreach (var (participant, flight) in MeuilleurVolPourChaqueParticipantRetour)
            {
                Console.WriteLine($"{participant} : {flight.Origin} - {flight.Destination}: date de départ: {flight.Departure}, - Cout du vol: {flight.GetCostWithWaitingTimeRetour()} $, soit (Prix: {flight.Price} $  et Temps d'attente: {(flight.Temps_Attente_Retour > 30 ? flight.Temps_Attente_Retour - 30 : 0)} min)");
            }



            Console.WriteLine();
        }
    }
}

public class Flight
{
    public decimal Price { get; set; }
    public int Stops { get; set; }
    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";
    public DateTime Departure { get; set; }
    public DateTime Arrival { get; set; }
    public string Airline { get; set; } = "";
    public double Duration => (Arrival - Departure).TotalHours;
    public double Temps_Attente_Aller => (new DateTime(2010, 7, 27, 17, 0, 0) - Arrival).TotalMinutes;
    public double Temps_Attente_Retour => (Departure - new DateTime(2010, 8, 3, 15, 0, 0)).TotalMinutes;

    public decimal GetCostWithWaitingTimeAller()
    {
        decimal cost = Price;

        // Convertir le temps d'attente en coût,au delas de 30 min, chaque minute d'attente coûte 10€ 
        if (Temps_Attente_Aller > 30)
        {
            cost += ((decimal)Temps_Attente_Aller - 30) * 10;
        }

        return cost;
    }
    public decimal GetCostWithWaitingTimeRetour()
    {
        decimal cost = Price;

        // Convertir le temps d'attente en coût,au delas de 30 min, chaque minute d'attente coûte 10€ 
        if (Temps_Attente_Retour > 30)
        {
            cost += ((decimal)Temps_Attente_Retour - 30) * 10;
        }

        return cost;
    }
}

public class Participant
{
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
}

namespace agb.dev.vol_planning
{
    public static class ConfererenceOrganisation
    {
        private const string FlightDataPath = "./ThirdParty/FlightData";
        private static readonly List<DateTime> ValidDatesAller = new List<DateTime>()
        {
            new DateTime(2010, 7, 26),
            new DateTime(2010, 7, 27)
        };

        private static readonly List<DateTime> ValidDatesRetour = new List<DateTime>()
        {
            new DateTime(2010, 8, 3),
            new DateTime(2010, 8, 4)
        };

        public static void Main()
        {
            ShowRamdomSolution();
        }

        private static void ShowRamdomSolution()
        {
            var listeSolution = new List<Solution>();

           

                var possibleSolutionsAller = VolsPourChaqueParticipantsAller(ValidDatesAller);
                var possibleSolutionsRetour = VolsPourChaqueParticipantsRetour(ValidDatesRetour);

                var meuilleurVolByParticipant_Aller = MeuilleurVolPourChaqueParticipantAller(possibleSolutionsAller);
                var meuilleurVolByParticipant_Retour = MeuilleurVolPourChaqueParticpantRetour(possibleSolutionsRetour);

               
                var solution = new Solution
                {
                    MeuilleurVolPourChaqueParticipantAller = meuilleurVolByParticipant_Aller,
                    MeuilleurVolPourChaqueParticipantRetour = meuilleurVolByParticipant_Retour
                };

                listeSolution.Add(solution);
         

            var listeSolutionSorted = listeSolution.OrderBy(x => x.Cout).ToList();
            var bestSolution = listeSolutionSorted.First();
            bestSolution.Print();
            
        }


        private static List<Participant> GetParticipants()
        {
            return new List<Participant> {
                new Participant { Name = "John Berlin", City = "TXL" },
                new Participant { Name = "Paul Paris", City = "CDG" },
                new Participant { Name = "Ringo Lyon", City = "MRS" },
                new Participant { Name = "George Lyon", City = "LYS" },
                new Participant { Name = "Mick Manchester", City = "MAN" },
                new Participant { Name = "Keith Bilbao", City = "BIO" },
                new Participant { Name = "Charlie New York", City = "JFK" },
                new Participant { Name = "Ronnie Tunis", City = "TUN" },
                new Participant { Name = "Barry Milan", City = "MXP" },
            };
        }

        private static Dictionary<string, List<Flight>> VolsPourChaqueParticipantsAller(List<DateTime> validDates)
        {
            var flightsDictionary = new Dictionary<string, List<Flight>>();

            foreach (var date in validDates)
            {
                var folderPath = Path.Combine(FlightDataPath, date.ToString("yyyy/MM-dd"));

                try
                {
                    var files = Directory.GetFiles(folderPath, "*.txt");

                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        var xml = File.ReadAllText(file);
                        var flightsXml = XElement.Parse(xml);

                        var flights = flightsXml.Elements("flight")
                            .Select(flight => new Flight
                            {
                                Price = decimal.Parse(flight.Element("price")!.Value),
                                Stops = int.Parse(flight.Element("stops")!.Value),
                                Origin = flight.Element("orig")!.Value,
                                Destination = flight.Element("dest")!.Value,
                                Departure = DateTime.ParseExact(flight.Element("depart")!.Value, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                                Arrival = DateTime.ParseExact(flight.Element("arrive")!.Value, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                                Airline = flight.Element("airline_display")!.Value
                            })
                            .Where(x => x.Arrival.Date == DateTime.ParseExact("2010-07-27", "yyyy-MM-dd", CultureInfo.InvariantCulture) && x.Arrival.TimeOfDay <= new TimeSpan(17, 0, 0))
                            .ToList();
                       

                        flightsDictionary[fileName] = flights;
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    Console.WriteLine($"Aucune donnée de vol disponible pour la date {date.ToString("yyyy-MM-dd")}");
                }
            }

            var vols = CustomParticipantNameAller(flightsDictionary);

            return vols;
        }

        private static Dictionary<string, List<Flight>> VolsPourChaqueParticipantsRetour(List<DateTime> validDates)
        {
            var flightsDictionary = new Dictionary<string, List<Flight>>();

            foreach (var date in validDates)
            {
                var folderPath = Path.Combine(FlightDataPath, date.ToString("yyyy/MM-dd"));

                try
                {
                    var files = Directory.GetFiles(folderPath, "*.txt");

                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        var xml = File.ReadAllText(file);
                        var flightsXml = XElement.Parse(xml);

                        var flights = flightsXml.Elements("flight")
                            .Select(flight => new Flight
                            {
                                Price = decimal.Parse(flight.Element("price")!.Value),
                                Stops = int.Parse(flight.Element("stops")!.Value),
                                Origin = flight.Element("orig")!.Value,
                                Destination = flight.Element("dest")!.Value,
                                Departure = DateTime.ParseExact(flight.Element("depart")!.Value, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                                Arrival = DateTime.ParseExact(flight.Element("arrive")!.Value, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                                Airline = flight.Element("airline_display")!.Value
                            })
                            .Where(x => x.Departure.Date == DateTime.ParseExact("2010-08-03", "yyyy-MM-dd", CultureInfo.InvariantCulture) && x.Departure.TimeOfDay >= new TimeSpan(15, 0, 0))
                            .ToList();

                        flightsDictionary[fileName] = flights;
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    Console.WriteLine($"Aucune donnée de vol disponible pour la date {date.ToString("yyyy-MM-dd")}");
                }
            }

            var vols = CustomParticipantNameRetour(flightsDictionary);

            return vols;
        }

   

        private static Dictionary<string, Flight> MeuilleurVolPourChaqueParticipantAller(Dictionary<string, List<Flight>> volsAller)
        {
            Dictionary<string, Flight> meilleursVols = new Dictionary<string, Flight>();

            // Parcours de chaque participant
            foreach (var participant in volsAller.Keys)
            {
                var volsParticipant = volsAller[participant];
                // ici on verifie les prochains et le precedents vol pour voir s'il offre pas un cout plus bas
               
                // on recuper un vol de facon ramdom 
                var random = new Random();
                var randomVol = volsParticipant.OrderBy(x => random.Next()).Take(1).ToList();
                var meilleurVolRamdomly = randomVol[0];
              

                
                var volsParJourEtHeureArriver = volsParticipant.OrderBy(f => f.Arrival.TimeOfDay);
                var indexMeilleurVol = volsParJourEtHeureArriver.ToList().IndexOf(meilleurVolRamdomly);

                var meuilleurVolTemporel1 =meilleurVolRamdomly;

                // Vérification des vols precedents
                while (indexMeilleurVol > 1)
                {
                    var volPrecedent = volsParticipant[indexMeilleurVol - 2];
                    var volSelected = volPrecedent;
                    if (volPrecedent.GetCostWithWaitingTimeAller() < meilleurVolRamdomly.GetCostWithWaitingTimeAller())
                    {
                        if(volPrecedent.GetCostWithWaitingTimeAller() < volSelected.GetCostWithWaitingTimeAller()){
                            volSelected = volPrecedent;
                        } 
                        meuilleurVolTemporel1 = volSelected;
                    }
                  

                    indexMeilleurVol--;
                }
                    // Console.WriteLine($"volPrecedent selectionner: {meuilleurVolTemporel1.GetCostWithWaitingTimeAller()} pour le vol {meuilleurVolTemporel1.Airline}");

                var meuilleurVolTemporel2 = meilleurVolRamdomly;
                // Vérification des vols suivants
                while (indexMeilleurVol < volsParticipant.Count - 2)
                {
                    var volSuivant = volsParticipant[indexMeilleurVol + 2];
                    var volSelected = volSuivant;
                    if (volSuivant.GetCostWithWaitingTimeAller() < meilleurVolRamdomly.GetCostWithWaitingTimeAller())
                    {
                        if(volSuivant.GetCostWithWaitingTimeAller() < volSelected.GetCostWithWaitingTimeAller()){
                            volSelected = volSuivant;
                        }
                        meuilleurVolTemporel2 = volSelected;
                    }

                    indexMeilleurVol++;
                }

                var meilleurVol = meuilleurVolTemporel2.GetCostWithWaitingTimeAller() < meuilleurVolTemporel1.GetCostWithWaitingTimeAller() ?  meuilleurVolTemporel2 : meuilleurVolTemporel1 ;



                // Ajout du participant avec son meilleur vol dans le dictionnaire
                meilleursVols.Add(participant, meilleurVol);
            }

           

            // Tri des meilleurs vols par heure d'arrivée
            var volsTries = meilleursVols.OrderBy(v => v.Value.Arrival.TimeOfDay);

           

            return meilleursVols;
        }

        private static Dictionary<string, Flight> MeuilleurVolPourChaqueParticpantRetour(Dictionary<string, List<Flight>> vols)
        {
            Dictionary<string, Flight> meilleursVols = new Dictionary<string, Flight>();

            // Parcours de chaque participant
            foreach (var participant in vols.Keys)
            {
                // Console.WriteLine("Participant : " + participant);
                var volsParticipant = vols[participant];


                // on recuper un vol de facon ramdom 
                var random = new Random();
                var randomVol = volsParticipant.OrderBy(x => random.Next()).Take(1).ToList();
                var meilleurVol = randomVol[0];

                // var meilleurVol = volsParticipant.OrderBy(f => f.GetCostWithWaitingTimeRetour()).First();
                var volsParJourEtHeureArriver = volsParticipant.OrderBy(f => f.Departure.TimeOfDay);    
                var indexMeilleurVol = volsParJourEtHeureArriver.ToList().IndexOf(meilleurVol);

                // Vérification des vols precedents
                while (indexMeilleurVol > 1)
                {
                    var volPrecedent = volsParticipant[indexMeilleurVol - 2];
                    if (volPrecedent.GetCostWithWaitingTimeRetour() < meilleurVol.GetCostWithWaitingTimeRetour())
                    {
                        meilleurVol = volPrecedent;
                    }

                    indexMeilleurVol--;
                }
                
                //  verification des vols suivants
                while (indexMeilleurVol < volsParticipant.Count - 2)
                {
                    var volSuivant = volsParticipant[indexMeilleurVol + 2];
                    if (volSuivant.GetCostWithWaitingTimeRetour() < meilleurVol.GetCostWithWaitingTimeRetour())
                    {
                        meilleurVol = volSuivant;
                    }

                    indexMeilleurVol++;
                }



                if (meilleurVol != null)
                {
                    meilleursVols.Add(participant, meilleurVol);
                }
            }

            // Tri des meilleurs vols par heure de départ
            var volsTries = meilleursVols.OrderBy(v => v.Value.Departure.TimeOfDay);
            
            return meilleursVols;
        }


        private static IEnumerable<Flight> GetFlyByOriginNamAller(string origin, Dictionary<string, List<Flight>> all_Aller_fly)
        {
            var flys = all_Aller_fly.Where(fly => fly.Value.Any(f => f.Origin == origin))
                                    .SelectMany(fly => fly.Value);

            // Triez les vols par coût total croissant
            var sortedFlights = flys.OrderBy(f => f.GetCostWithWaitingTimeAller());

            return sortedFlights;
        }

        private static IEnumerable<Flight> GetFlyByOriginNamRetour(string origin, Dictionary<string, List<Flight>> vols)
        {
            var flys = vols.Where(fly => fly.Value.Any(f => f.Destination == origin))
                                     .SelectMany(fly => fly.Value);



            var sortedFlights = flys.OrderBy(f => f.GetCostWithWaitingTimeRetour());

            return sortedFlights;
        }

        private static Dictionary<string, List<Flight>> CustomParticipantNameAller(Dictionary<string, List<Flight>> vols)
        {
            var participants = GetParticipants();

            var allFlightsFavorites = new Dictionary<string, List<Flight>>();

            foreach (Participant participant in participants)
            {
                var flights = GetFlyByOriginNamAller(participant.City, vols).ToList();
                allFlightsFavorites.Add(participant.Name, flights);
            }

            return allFlightsFavorites;
        }
        private static Dictionary<string, List<Flight>> CustomParticipantNameRetour(Dictionary<string, List<Flight>> vols)
        {
            var participants = GetParticipants();

            var allFlightsFavorites = new Dictionary<string, List<Flight>>();

            foreach (Participant participant in participants)
            {
                var flights = GetFlyByOriginNamRetour(participant.City, vols).ToList();
                allFlightsFavorites.Add(participant.Name, flights);
            }
            return allFlightsFavorites;
        }
    }
}


