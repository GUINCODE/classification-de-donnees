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
        string[] nomsFichiers = Directory.GetFiles(dossierWiki, "*.txt");

        var articleWithListMots = new Dictionary<string, List<string>>();
        var tousLesMots = new List<string>();

        var occurenceMotsParArticle = new Dictionary<string, Dictionary<string, int>>();
        var similarityArticle = new Dictionary<string, Dictionary<string, double>>();

        foreach (string nomFichier in nomsFichiers)
        {
            string nomArticle = Path.GetFileNameWithoutExtension(nomFichier);
            string contenu = File.ReadAllText(nomFichier);
            string contenuNettoye = SupprimerCaracteresSpeciaux(contenu);
            string[] mots = contenuNettoye.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            articleWithListMots[nomArticle] = mots.Distinct().ToList();
            tousLesMots.AddRange(mots.Distinct());

            // on compte le nombre d'occurence de chaque mot dans chaque article
            var motCompteDansArticle = CompterOccurenceMotsDansArticle(nomArticle, articleWithListMots[nomArticle]);

            occurenceMotsParArticle[nomArticle] = motCompteDansArticle;
        }

        similarityArticle = CalculDeSimilarity(occurenceMotsParArticle);

        // Appliquation de K-means clustering
        int k = 5; // Nombre de groupe souhaité
        var clusteringResult = KMeansClustering(similarityArticle, k);



        // Afficher les articles par groupe
        int totalClusters = clusteringResult.Values.Max() + 1;
        List<string>[] groupes = new List<string>[totalClusters];
        for (int i = 0; i < totalClusters; i++)
        {
            groupes[i] = new List<string>();
        }

        foreach (var articleCluster in clusteringResult)
        {
            string articleName = articleCluster.Key;
            int cluster = articleCluster.Value;
            groupes[cluster].Add(articleName);
        }

        // trier les groupes par ordre decroissant sur le nombres d'articles
        Array.Sort(groupes, (x, y) => y.Count - x.Count);

        Console.WriteLine();
        Console.WriteLine(" Afficahe des groupes d'articles:");
        for (int i = 0; i < totalClusters; i++)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Groupe d'articles: {i + 1} (contient {groupes[i].Count} articles)");

            if (groupes[i].Count >= 2)
            {
                double similarity = similarityArticle[groupes[i][0]][groupes[i][1]];
               
                Console.ResetColor();
               
                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.WriteLine($"Similarité entre les articles du groupe : {similarity}");
            }

            Console.ResetColor();

            foreach (var article in groupes[i])
            {
                Console.WriteLine(article);
            }

            Console.WriteLine();
        }
        // affichage les details des similarité entres les articles
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("Affichage des details de similarités:");
        Console.ResetColor();
        Console.Write("souhaitez vous affichers les details de similarité entre les articles ? (O/N): ");
        string? reponse = Console.ReadLine();
        if (reponse != null && (reponse == "O" || reponse == "o"))
        {
            AfficherSimilarite(similarityArticle);
        }
        
        


    }

    static void AfficherSimilarite(Dictionary<string, Dictionary<string, double>> similarityArticle ){
        foreach (var article1 in similarityArticle)
        {
            string articleName1 = article1.Key;

            foreach (var article2 in article1.Value)
            {
                string articleName2 = article2.Key;
                double similarity = article2.Value;

                Console.WriteLine($"Similarité [{articleName1} - {articleName2}] : {similarity}");
            }
        }
    }

    static Dictionary<string, int> CompterOccurenceMotsDansArticle(string articleName, List<string> mots)
    {
        var result = new Dictionary<string, int>();
        foreach (var mot in mots)
        {
            if (result.ContainsKey(mot))
            {
                result[mot]++;
            }
            else
            {
                result[mot] = 1;
            }
        }
        return result;
    }

    static Dictionary<string, Dictionary<string, double>> CalculDeSimilarity(Dictionary<string, Dictionary<string, int>> articleWithListMots)
    {
        var similarityArticle = new Dictionary<string, Dictionary<string, double>>();

        foreach (var article1 in articleWithListMots)
        {
            string articleName1 = article1.Key;
            similarityArticle[articleName1] = new Dictionary<string, double>();

            foreach (var article2 in articleWithListMots)
            {
                string articleName2 = article2.Key;

                if (articleName1 == articleName2)
                {
                    similarityArticle[articleName1][articleName2] = 1.0; // La similarité avec soi-même est de 1
                }
                else
                {
                    double similarity = CalculateSimilarity(article1.Value, article2.Value);
                    similarityArticle[articleName1][articleName2] = similarity;
                }
            }
        }

        return similarityArticle;
    }

    static double CalculateSimilarity(Dictionary<string, int> article1, Dictionary<string, int> article2)
    {
        // Jaccard similarity :
        var intersection = new HashSet<string>(article1.Keys);
        intersection.IntersectWith(article2.Keys);

        var union = new HashSet<string>(article1.Keys);
        union.UnionWith(article2.Keys);

        return (double)intersection.Count / union.Count;
    }

    static string SupprimerCaracteresSpeciaux(string input)
    {
        var mots = Regex.Replace(input, "[^a-zA-Z0-9éèêëÉÈÊËàâäÂÀÄîïÎÏûùüÛÙÜôöÔÖÿŸçÇœŒ]", " ");
        var motsEnMinuscule = mots.ToLower();
        var motsSansEspacesRedondants = Regex.Replace(motsEnMinuscule, @"\s+", " ");
        return motsSansEspacesRedondants;
    }

    static Dictionary<string, int> KMeansClustering(Dictionary<string, Dictionary<string, double>> similarityArticle, int k)
    {
        var clusteringResult = new Dictionary<string, int>();

        // Initialiser les centroïdes
        var centroidArticles = similarityArticle.Keys.Take(k).ToList();

        // Initialiser les afectation des grope
        foreach (var article in similarityArticle.Keys)
        {
            clusteringResult[article] = -1; // -1 pour les articles non assignés à un cluster
        }

        bool converge = false;
        while (!converge)
        {
            // Affecter chaque article au cluster du centroïde le plus proche
            foreach (var article in similarityArticle.Keys)
            {
                double maxSimilarity = double.MinValue;
                int maxCluster = -1;

                foreach (var centroid in centroidArticles)
                {
                    if (similarityArticle[article][centroid] > maxSimilarity)
                    {
                        maxSimilarity = similarityArticle[article][centroid];
                        maxCluster = centroidArticles.IndexOf(centroid);
                    }
                }

                clusteringResult[article] = maxCluster;
            }

            // Mettre à jour les centroïdes
            var newCentroidArticles = new List<string>();

            for (int cluster = 0; cluster < k; cluster++)
            {
                var articlesInCluster = clusteringResult.Where(x => x.Value == cluster).Select(x => x.Key).ToList();
                double maxSimilaritySum = double.MinValue;
                string newCentroid = string.Empty;

                foreach (var article1 in articlesInCluster)
                {
                    double similaritySum = 0.0;

                    foreach (var article2 in articlesInCluster)
                    {
                        similaritySum += similarityArticle[article1][article2];
                    }

                    if (similaritySum > maxSimilaritySum)
                    {
                        maxSimilaritySum = similaritySum;
                        newCentroid = article1;
                    }
                }

                newCentroidArticles.Add(newCentroid);
            }

            // Vérifier la convergence
            if (centroidArticles.SequenceEqual(newCentroidArticles))
            {
                converge = true;
            }
            else
            {
                centroidArticles = newCentroidArticles;
            }
        }

        return clusteringResult;
    }
}
