using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidYouMeanService
{
    public class ServiceProgram
    {
        private static List<String> suggestions;
        static void Main(string[] args)
        {
            Console.WriteLine("Hello dear user! Here you can see all activity on all services.");
            PopulateSuggestions();
            MessageHandler handler = new MessageHandler(0);
        }

        private static void PopulateSuggestions()
        {
            suggestions = new List<String>()
            {
                "vask op", "ryd op", "vask gulv", "tøm skraldespand", "gå ud med skrald", "ryd op på værelset",
                "ryd op på dit værelse", "støvsug", "støvsug værelse", "støvsug stue", "støvsug gang" , "støvsug køkken",
                "støvsug køkkenet", "lav mad", "lav lektier", "vask tøj", "slå græs", "slå græsset", "giv hunden mad",
                "giv hund mad", "giv kat mad", "giv katten mad", "fodr katten", "fodr hunden", "fodr dyrene", "dæk bord",
                "dæk bordet", "tøm opvaskemaskine", "sæt i opvaskemaskine", "sæt i opvaskemaskinen", "tøm opvaskemaskinen",
                "red seng", "hæng vasketøj op", "gå tur med hunden", "læg vasketøj sammen", "smør madpakke"
            };
        }

        //Checks if there is a suitable suggestion in the list for the given word.s
        public static String CheckForSuggestions(String word)
        {
            int maxDistance;
            if (word.Length < 5)
                maxDistance = 1;
            else if (word.Length < 8)
                maxDistance = 2;
            else
                maxDistance = 3;
            int bestDistance = 4;
            String bestSuggestion = null;
            //Checks if any suggestions matches the given word
            foreach (var suggestion in suggestions)
            {
                int distance = 0;
                String tmpSuggestion = suggestion;
                    String tmpWord = word;

                    //Checks if the suggestion matches the length of the given word
                    if (suggestion.Length >= word.Length - maxDistance && suggestion.Length <= word.Length + maxDistance)
                    {
                        //Adds spaces to the shortest of either suggestion or the given word
                        if (suggestion.Length < word.Length)
                        {
                            for (int i = 0; i < word.Length - suggestion.Length; i++)
                            {
                                tmpSuggestion += " ";
                            }
                        }
                        else
                        {
                            for (int i = 0; i < suggestion.Length - word.Length; i++)
                            {
                                tmpWord += " ";
                            }
                        }
                        //Checks if the characters in the given word matches the characters at the same place in the suggested word. Distance is incremented if not.
                        for (int i = 0; i < tmpWord.Length; i++)
                        {
                            if (tmpWord.ElementAt(i) != tmpSuggestion.ElementAt(i))
                            {
                                //Checks for switcharoos, skips next word if found. Distance is incremented.
                                if (i < tmpWord.Length - 1 && tmpWord.ElementAt(i) == tmpSuggestion.ElementAt(i + 1) && tmpWord.ElementAt(i + 1) == tmpSuggestion.ElementAt(i))
                                {
                                    distance++;
                                    i++;
                                }
                                else
                                    distance++;
                            }
                        }
                        //Returns null if a perfect match is found
                        if (distance == 0)
                            return null;
                        //If distance is less than best distance, the suggestion is saved as the best suggestion
                        else if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            bestSuggestion = suggestion;
                        }
                    }
            }
            return bestSuggestion;

        }

    }
}
