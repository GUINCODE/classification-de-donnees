using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
public class Solution
{
    public Dictionary<string, Flight>? VolsAller { get; set; }
    public Dictionary<string, Flight>? VolsRetour { get; set; }

    public decimal CoutAller => VolsAller?.Sum(x => x.Value.GetCostWithWaitingTimeAller()) ?? 0;
    public decimal CoutRetour => VolsRetour?.Sum(x => x.Value.GetCostWithWaitingTimeRetour()) ?? 0;
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

        if (VolsAller != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Vols Aller ({VolsAller.Count} vols) :");
            Console.ResetColor();

            foreach (var (participant, flight) in VolsAller)
            {
                Console.WriteLine($"{participant} : {flight.Origin} - {flight.Destination}: date d'arrivée: {flight.Arrival}, - Cout du vol: {flight.GetCostWithWaitingTimeAller()} $, soit (Prix: {flight.Price} $  et Temps d'attente: {(flight.Temps_Attente_Aller > 30 ? flight.Temps_Attente_Aller - 30 : 0)} min)");
            }

            Console.WriteLine();
        }

        if (VolsRetour != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Vols Retour ({VolsRetour.Count} vols) :");
            Console.ResetColor();

            foreach (var (participant, flight) in VolsRetour)
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
    public Flight? Precedent { get; set; }
    public Flight? Suivant { get; set; }
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

            var volsAller = VolsAllerPourChaqueParticipants(ValidDatesAller);
            var volsRetour = VolsRetourPourChaqueParticipants(ValidDatesRetour);

            var ramdomUniqueVolsAller = RandomlyVolsAllerParParticipant(volsAller);
            var ramdomUniqueVolsRetour = RamdomlyVolsRetourParParticipant(volsRetour);

            var firsSolution = new Solution
            {
                VolsAller = ramdomUniqueVolsAller,
                VolsRetour = ramdomUniqueVolsRetour
            };

            var espaceDeSolution = CreerAutresSolutionAvecSuivantEtPrecedentDeChaqueVolsDelaSolution(firsSolution);

            AfficherMeuilleurSolution(espaceDeSolution);

        }

        private static void AfficherMeuilleurSolution(List<Solution> espaceDeSolution)
        {
            // affiche la taille de l'espace de solution
            Console.WriteLine($"Taille de l'espace de solution : {espaceDeSolution.Count}");

            var meuilleurSolution = espaceDeSolution.OrderBy(s => s.Cout).First();
            meuilleurSolution.Print();

        }


        private static List<Solution> CreerAutresSolutionAvecSuivantEtPrecedentDeChaqueVolsDelaSolution(Solution firstSolution)
        {
            var listeSolutionFinale = new List<Solution>();
            listeSolutionFinale.Add(firstSolution);

            if (firstSolution.VolsAller != null)
            {
                var volsAller = firstSolution.VolsAller;
                for (int i = 0; i < volsAller.Count; i++)
                {
                    var vol = volsAller.ElementAt(i);
                    var volPrecedent = i > 0 ? volsAller.ElementAt(i - 1).Value : null;
                    var volSuivant = i < volsAller.Count - 1 ? volsAller.ElementAt(i + 1).Value : null;

                    if (volPrecedent != null)
                    {
                        var oneSolution = CreateSolutionCopy(firstSolution);
                        oneSolution.VolsAller[volsAller.ElementAt(i).Key] = volPrecedent;
                        oneSolution.VolsAller[volsAller.ElementAt(i).Key].Precedent = null;
                        oneSolution.VolsAller[volsAller.ElementAt(i).Key].Suivant = null;
                        listeSolutionFinale.Add(oneSolution);
                    }

                    if (volSuivant != null)
                    {
                        
                        var oneSolution = CreateSolutionCopy(firstSolution);
                        oneSolution.VolsAller[volsAller.ElementAt(i).Key] = volSuivant;
                        oneSolution.VolsAller[volsAller.ElementAt(i).Key].Precedent = null;
                        oneSolution.VolsAller[volsAller.ElementAt(i).Key].Suivant = null;
                        listeSolutionFinale.Add(oneSolution);
                    }
                }
            }

            if (firstSolution.VolsRetour != null)
            {
                var volsRetour = firstSolution.VolsRetour;
                for (int i = 0; i < volsRetour.Count; i++)
                {
                    var vol = volsRetour.ElementAt(i);
                    var volPrecedent = i > 0 ? volsRetour.ElementAt(i - 1).Value : null;
                    var volSuivant = i < volsRetour.Count - 1 ? volsRetour.ElementAt(i + 1).Value : null;

                    if (volPrecedent != null)
                    {
                        var oneSolution = CreateSolutionCopy(firstSolution);
                        oneSolution.VolsRetour[volsRetour.ElementAt(i).Key] = volPrecedent;
                        oneSolution.VolsRetour[volsRetour.ElementAt(i).Key].Precedent = null;
                        oneSolution.VolsRetour[volsRetour.ElementAt(i).Key].Suivant = null;
                        listeSolutionFinale.Add(oneSolution);
                    }

                    if (volSuivant != null)
                    {
                        var oneSolution = CreateSolutionCopy(firstSolution);
                        oneSolution.VolsRetour[volsRetour.ElementAt(i).Key] = volSuivant;
                        oneSolution.VolsRetour[volsRetour.ElementAt(i).Key].Precedent = null;
                        oneSolution.VolsRetour[volsRetour.ElementAt(i).Key].Suivant = null;
                        listeSolutionFinale.Add(oneSolution);
                    }
                }
            }

            return listeSolutionFinale;
        }


        private static Solution CreateSolutionCopy(Solution solution)
        {
            var copiedSolution = new Solution();

            if (solution.VolsAller != null)
            {
                copiedSolution.VolsAller = new Dictionary<string, Flight>(solution.VolsAller);
            }

            if (solution.VolsRetour != null)
            {
                copiedSolution.VolsRetour = new Dictionary<string, Flight>(solution.VolsRetour);
            }

            return copiedSolution;
        }



        // private static Flight? GetPrecedentVolsByKey(string key, Dictionary<string, List<Flight>> vols)
        // {
        //     int index = -1;
        //     for (int i = 0; i < vols.Count; i++)
        //     {
        //         if (vols.ElementAt(i).Key == key)
        //         {
        //             index = i;
        //             break;
        //         }
        //     }

        //     if (index > 0)
        //     {
        //         var previousKey = vols.ElementAt(index - 1).Key;
        //         var previousFlights = vols[previousKey];
        //         if (previousFlights.Count > 0)
        //         {
        //             return previousFlights[0];
        //         }
        //     }

        //     return null;
        // }

        // private static Flight? GetSuivantVolsByKey(string key, Dictionary<string, List<Flight>> vols)
        // {
        //     int index = -1;
        //     for (int i = 0; i < vols.Count; i++)
        //     {
        //         if (vols.ElementAt(i).Key == key)
        //         {
        //             index = i;
        //             break;
        //         }
        //     }

        //     if (index < vols.Count - 1)
        //     {
        //         var nextKey = vols.ElementAt(index + 1).Key;
        //         var nextFlights = vols[nextKey];
        //         if (nextFlights.Count > 0)
        //         {
        //             return nextFlights[0];
        //         }
        //     }

        //     return null;
        // }


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

        private static Dictionary<string, List<Flight>> VolsAllerPourChaqueParticipants(List<DateTime> validDates)
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
                            .OrderBy(x => x.Arrival)
                            .ToList();

                            // Ajout de vols precedent et suivant pour chaque vols 
                            for (int i = 0; i < flights.Count; i++)
                            {
                                if (i > 0)
                                {
                                    flights[i].Precedent = flights[i - 1];
                                }

                                if (i < flights.Count - 1)
                                {
                                    flights[i].Suivant = flights[i + 1];
                                }
                            }


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

        private static Dictionary<string, List<Flight>> VolsRetourPourChaqueParticipants(List<DateTime> validDates)
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
                            .OrderBy(x => x.Departure)
                            .ToList();

                        // Ajout de vols precedent et suivant pour chaque vols
                        for (int i = 0; i < flights.Count; i++)
                        {
                            if (i > 0)
                            {
                                flights[i].Precedent = flights[i - 1];
                            }

                            if (i < flights.Count - 1)
                            {
                                flights[i].Suivant = flights[i + 1];
                            }
                        }

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


        private static Dictionary<string, Flight> RandomlyVolsAllerParParticipant(Dictionary<string, List<Flight>> volsAller)
        {
            var random = new Random();
            var vols = new Dictionary<string, Flight>();
            var participants = GetParticipants();

            foreach (var participant in participants)
            {
                var volsParticipant = volsAller[participant.Name];
                int randomIndex = random.Next(0, volsParticipant.Count);
                Flight volAleatoire = volsParticipant[randomIndex];

                vols[participant.Name] = volAleatoire;
            }

            return vols;
        }

        private static Dictionary<string, Flight> RamdomlyVolsRetourParParticipant(Dictionary<string, List<Flight>> volsRetour)
        {
            var random = new Random();
            var vols = new Dictionary<string, Flight>();
            var participants = GetParticipants();

            foreach (var participant in participants)
            {
                var volsParticipant = volsRetour[participant.Name];
                int randomIndex = random.Next(0, volsParticipant.Count);
                Flight volAleatoire = volsParticipant[randomIndex];

                vols[participant.Name] = volAleatoire;
            }

            return vols;

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


