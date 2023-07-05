Dictionary<string, Dictionary<string, double>> critics = new Dictionary<string, Dictionary<string, double>>()
{
    {"Lisa Rose", new Dictionary<string, double>() {{"Lady in the Water", 2.5}, {"Snakes on a Plane", 3.5}, {"Just My Luck", 3.0}, {"Superman Returns", 3.5}, {"You, Me and Dupree", 2.5}, {"The Night Listener", 3.0}}},
    {"Gene Seymour", new Dictionary<string, double>() {{"Lady in the Water", 3.0}, {"Snakes on a Plane", 3.5}, {"Just My Luck", 1.5}, {"Superman Returns", 5.0}, {"The Night Listener", 3.0}, {"You, Me and Dupree", 3.5}}},
    {"Michael Phillips", new Dictionary<string, double>() {{"Lady in the Water", 2.5}, {"Snakes on a Plane", 3.0}, {"Superman Returns", 3.5}, {"The Night Listener", 4.0}}},
    {"Claudia Puig", new Dictionary<string, double>() {{"Snakes on a Plane", 3.5}, {"Just My Luck", 3.0}, {"The Night Listener", 4.5}, {"Superman Returns", 4.0}, {"You, Me and Dupree", 2.5}}},
    {"Mick LaSalle", new Dictionary<string, double>() {{"Lady in the Water", 3.0}, {"Snakes on a Plane", 4.0}, {"Just My Luck", 2.0}, {"Superman Returns", 3.0}, {"The Night Listener", 3.0}, {"You, Me and Dupree", 2.0}}},
    {"Jack Matthews", new Dictionary<string, double>() {{"Lady in the Water", 3.0}, {"Snakes on a Plane", 4.0}, {"The Night Listener", 3.0}, {"Superman Returns", 5.0}, {"You, Me and Dupree", 3.5}}},
    {"Toby", new Dictionary<string, double>() {{"Snakes on a Plane", 4.5}, {"You, Me and Dupree", 1.0}, {"Superman Returns", 4.0}}}
};




double Sim_distance(Dictionary<string, Dictionary<string, double>> critics, string personne1, string personne2)
{
    double somme = 0;
    int nbFilms = 0;
    foreach (KeyValuePair<string, double> film in critics[personne1])
    {
        if (critics[personne2].ContainsKey(film.Key))
        {
            somme += Math.Pow(critics[personne1][film.Key] - critics[personne2][film.Key], 2);
            nbFilms++;
        }
    }
    if (nbFilms == 0)
    {
        // Console.WriteLine("Aucun film en commun entre " + personne1 + " et " + personne2);
        return 0.0;
    }
    double distance = Math.Sqrt(somme);
    double similarite = 1 / (1 + distance);
    // Console.WriteLine("Distance entre " + personne1 + " et " + personne2 + " : " + similarite);
    return similarite;
}





double Sim_person(Dictionary<string, Dictionary<string, double>> critics, string personne1, string personne2)
{
    double somme1 = 0;
    double somme2 = 0;
    double somme1Carre = 0;
    double somme2Carre = 0;
    double sommeProduit = 0;
    int nbFilms = 0;
    foreach (KeyValuePair<string, double> film in critics[personne1])
    {
        if (critics[personne2].ContainsKey(film.Key))
        {
            somme1 += critics[personne1][film.Key];
            somme2 += critics[personne2][film.Key];
            somme1Carre += Math.Pow(critics[personne1][film.Key], 2);
            somme2Carre += Math.Pow(critics[personne2][film.Key], 2);
            sommeProduit += critics[personne1][film.Key] * critics[personne2][film.Key];
            nbFilms++;
        }
    }
    if (nbFilms == 0)
    {
        // Console.WriteLine("Aucun film en commun entre " + personne1 + " et " + personne2);
        return 0.0;
    }
    double similarite = (sommeProduit - (somme1 * somme2 / nbFilms)) / (Math.Sqrt((somme1Carre - Math.Pow(somme1, 2) / nbFilms) * (somme2Carre - Math.Pow(somme2, 2) / nbFilms)));
    // Console.WriteLine("Similarite entre " + personne1 + " et " + personne2 + " : " + similarite);
    return similarite;
}

void Recommandation(Dictionary<string, Dictionary<string, double>> critics, string personne)
{
    var filmsNonRegarder = FilmNonregarderMaisRegarderParLesproches(personne, critics);
     
}


 Dictionary<string, Dictionary<string, double>> FilmNonregarderMaisRegarderParLesproches(string person, Dictionary<string, Dictionary<string, double>> listeCritic)
{
    Dictionary<string, Dictionary<string, double>> nonReviewedMovies = new Dictionary<string, Dictionary<string, double>>();
    // on recupere les  personnes similaires a la personne
    Dictionary<string, Dictionary<string, double>> similaritePersonne = new Dictionary<string, Dictionary<string, double>>();
    foreach (KeyValuePair<string, Dictionary<string, double>> critic in listeCritic)
    {
        if (critic.Key != person && Sim_person(listeCritic, person, critic.Key) > 0)
        {
            Dictionary<string, double> similarite = new Dictionary<string, double>();
            similarite.Add(critic.Key, Sim_person(listeCritic, person, critic.Key));
            similaritePersonne.Add(critic.Key, similarite);
        }
    }

    Dictionary<string, double>  listeSx = new  Dictionary<string, double>();
    Dictionary<string, double>  listeScorePondere = new  Dictionary<string, double>();
    foreach (KeyValuePair<string, Dictionary<string, double>> critic in similaritePersonne)
    {
        
        foreach (KeyValuePair<string, double> movie in listeCritic[person])
        {
            if (listeCritic[critic.Key].ContainsKey(movie.Key))
            {
                listeCritic[critic.Key].Remove(movie.Key);
            }
        }

        Console.WriteLine("Critic: " + ReturnLastWord(critic.Key ));
        Console.WriteLine("  Similarite: " + critic.Value[critic.Key]);
    
       
        foreach (KeyValuePair<string, double> movie in listeCritic[critic.Key].OrderByDescending(movie => movie.Value))
        {
            var courtNom = ReturnFirstWord(movie.Key);
            var SxFilm =  movie.Value * critic.Value[critic.Key];
            Console.WriteLine("  Film: " + movie.Key + ", Note: " + movie.Value);
            Console.WriteLine("  S.x" +courtNom + ": " + SxFilm);
            // on ajoute le film et sa valeur dans la listeSx
            if (listeSx.ContainsKey(courtNom))
            {
                listeSx[courtNom] += SxFilm;
            }
            else
            {
                listeSx.Add(courtNom, SxFilm);
            }
            // on ajoute le film et sa valeur dans la listeScorePondere
            if (listeScorePondere.ContainsKey(courtNom))
            {
                listeScorePondere[courtNom] += critic.Value[critic.Key];
            }
            else
            {
                listeScorePondere.Add(courtNom, critic.Value[critic.Key]);
            }
        }
    }
    //  on affiche la listeSx
    Console.WriteLine("Totaux des S.x de chaque film:");
    foreach (KeyValuePair<string, double> sx in listeSx.OrderByDescending(sx => sx.Value))
    {
        Console.WriteLine("  TotalSx" + sx.Key + " : " + Math.Truncate(sx.Value * 100) / 100);
    }
    //  on affiche la listeScorePondere
    Console.WriteLine("Total de score ponderé:");
    foreach (KeyValuePair<string, double> scorePondere in listeScorePondere.OrderByDescending(scorePondere => scorePondere.Value))
    {
        Console.WriteLine("  ScorePondere" + scorePondere.Key + " : " + Math.Truncate(scorePondere.Value * 100) / 100);
    }

    // calcul Total/SimSum
    Dictionary<string, double>  listeScorePondereFinal = new  Dictionary<string, double>();
    foreach (KeyValuePair<string, double> scorePondere in listeScorePondere.OrderByDescending(scorePondere => scorePondere.Value))
    {
        var courtNom = ReturnFirstWord(scorePondere.Key);
        var scorePondereFinal = listeSx[courtNom] / scorePondere.Value;
        listeScorePondereFinal.Add(scorePondere.Key, scorePondereFinal);
    }
    // Total/SimSum
    Console.WriteLine("Total/Sim.Sum:");
    foreach (KeyValuePair<string, double> scorePondereFinal in listeScorePondereFinal.OrderByDescending(scorePondereFinal => scorePondereFinal.Value))
    {
        Console.WriteLine("  Total/Sim.Sum" + scorePondereFinal.Key + " : " + Math.Truncate(scorePondereFinal.Value * 100) / 100);
    }

    return nonReviewedMovies;


}

 string ReturnFirstWord(string sentence){
    if(sentence == "The Night Listener"){
        return "Night"; 
    }
    if (sentence == "Just My Luck")
    {
        return "Luck"; 
    }
    
    string[] words = sentence.Split(' ');
    return words[0];
    }

string ReturnLastWord(string sentence){
    string[] words = sentence.Split(' ');
    return words[words.Length-1];
    }




Console.WriteLine(Sim_distance(critics, "Lisa Rose", "Gene Seymour"));
Console.WriteLine(Sim_distance(critics, "Lisa Rose", "Michael Phillips"));
Console.WriteLine(Sim_person(critics, "Lisa Rose", "Gene Seymour"));
Console.WriteLine(Sim_person(critics, "Lisa Rose", "Michael Phillips"));
Console.WriteLine(Sim_person(critics, "Lisa Rose", "Toby"));

Console.WriteLine("---- Recommandation ----");
Recommandation(critics, "Toby");

