using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string dossierWiki = "wiki";
        var dictionnaireCible = new Dictionary<string, int>();

        string[] nomsFichiers = Directory.GetFiles(dossierWiki, "*.txt");

        foreach (string nomFichier in nomsFichiers)
        {
            string contenu = File.ReadAllText(nomFichier);

            string contenuNettoye = SupprimerCaracteresSpeciaux(contenu);

            Dictionary<string, int> occurencesMots = CompterOccurencesMots(contenuNettoye);

            Dictionary<string, int> occurencesMotsFiltres = FiltrerOccurencesMots(occurencesMots);

            dictionnaireCible = FusionnerDictionnaires(occurencesMotsFiltres, dictionnaireCible);
        }

        var dictionnaireTrier = dictionnaireCible.OrderByDescending(entry => entry.Value);
        foreach (var entry in dictionnaireTrier)
        {
            Console.WriteLine($"{entry.Key}: {entry.Value}");
        }
    }

    static string SupprimerCaracteresSpeciaux(string input)
    {
        var mots = Regex.Replace(input, "[^a-zA-Z0-9]", " ");
        var motsEnMinuscule = mots.ToLower();
        var motsSansEspacesRedondants = Regex.Replace(motsEnMinuscule, @"\s+", " ");
        return motsSansEspacesRedondants;
    }

    static Dictionary<string, int> CompterOccurencesMots(string input)
    {
        Dictionary<string, int> occurencesMots = new Dictionary<string, int>();

        string[] mots = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string mot in mots)
        {
            if (occurencesMots.ContainsKey(mot))
            {
                occurencesMots[mot]++;
            }
            else
            {
                occurencesMots[mot] = 1;
            }
        }

        return occurencesMots;
    }

    static Dictionary<string, int> FiltrerOccurencesMots(Dictionary<string, int> occurencesMots)
    {
        int totalCount = occurencesMots.Sum(entry => entry.Value);
        int minCount = (int)(totalCount * 0.03); // ici j'ai utilisé 3%, car 10% était trop élevé et retounait rien
        int maxCount = (int)(totalCount * 0.5);

        var occurencesMotsFiltres = occurencesMots.Where(entry => entry.Key.Length > 3)
                                                  .Where(entry => entry.Value >= minCount && entry.Value <= maxCount)
                                                  .ToDictionary(entry => entry.Key, entry => entry.Value);

        return occurencesMotsFiltres;
    }

    static Dictionary<string, int> FusionnerDictionnaires(Dictionary<string, int> sources, Dictionary<string, int> cible)
    {
        foreach (var source in sources)
        {
            if (cible.ContainsKey(source.Key))
            {
                cible[source.Key] += source.Value;
            }
            else
            {
                cible[source.Key] = source.Value;
            }
        }

        return cible;
    }
}
