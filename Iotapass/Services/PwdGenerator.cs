using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iotapass.Services
{
    internal class PwdGenerator
    {
        /// <summary>
        /// Generate non-wordy password. returns empty string if size is negative or 0. Max size of 30. size is the size of password. Create a new RNG and pass the instance through this method.
        /// </summary>
        internal static string GeneratePassword(int size, bool isSpecialCharsEnabled, Random rnd)
        {
            if (size < 1)
            {
                return "";
            }
            // Max of 30 chars
            if (size > 30)
            {
                size = 30;
            }
            string charslist = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Length of 62 without special chars.
            string specialcharslist = "!@#$%^&*()-_=+"; // Change me arouund if you want

            if (isSpecialCharsEnabled)
            {
                charslist += specialcharslist;
            }

            string p = "";
            for (int i = 0; i < size; i++)
            {
                p += charslist[rnd.Next(0, charslist.Length)];
            }
            return p;
        }
        /// <summary>
        /// Generate wordy password. returns empty string if size is negative or 0. Max size of 20. size is the size of password. Create a new RNG and pass the instance through this method. Follows the  format: AdjectiveNoun##
        /// </summary>
        internal static string GenerateWordyPassword(int size, bool isSpecialCharsEnabled, Random rnd)
        {
            // Return if invalid size
            if (size < 1)
            {
                return "";
            }
            // Max of 20 chars
            if (size > 20)
            {
                size = 20;
            }
            string[] adjlist = { "Accomplished", "Adaptable", "Adept", "Adventurous", "Affable", "Affectionate", "Agreeable", "Alluring", "Amazing", "Ambitious", "Amiable", "Amicable", "Ample", "Amusing", "Approachable", "Articulate", "Awesome", "Blithesome", "Brave", "Bright", "Brilliant", "Capable", "Captivating", "Careful", "Charismatic", "Charming", "Chatty", "Cheerful", "Communicative", "Compassionate", "Competitive", "Conscientious", "Considerate", "Convivial", "Courageous", "Courteous", "Creative", "Dazzling", "Decisive", "Dependable", "Determined", "Devoted", "Diligent", "Diplomatic", "Discreet", "Dynamic", "Efficient", "Elegant", "Emotional", "Enchanting", "Energetic", "Engaging", "Enthusiastic", "Excellent", "Expert", "Exuberant", "Fabulous", "Faithful", "Fantastic", "Fearless", "Flexible", "Focused", "Forceful", "Frank", "Friendly", "Funny", "Generous", "Gentle", "Giving", "Gleaming", "Glimmering", "Glistening", "Glittering", "Glowing", "Good", "Gorgeous", "Gregarious", "Helpful", "Hilarious", "Honest", "Humorous", "Imaginative", "Impartial", "Incredible", "Independent", "Inquisitive", "Insightful", "Intellectual", "Intelligent", "Intuitive", "Inventive", "Kind", "Knowledgeable", "Kooky", "Likable", "Lovely", "Loving", "Loyal", "Lustrous", "Magnificent", "Marvelous", "Mirthful", "Neat", "Nice", "Optimistic", "Organized", "Outstanding", "Passionate", "Patient", "Perfect", "Persistent", "Personable", "Philosophical", "Pioneering", "Plucky", "Powerful", "Practical", "Productive", "Proficient", "Propitious", "Qualified", "Quiet", "Rational", "Ravishing", "Relaxed", "Reliable", "Remarkable", "Reserved", "Resourceful", "Responsible", "Romantic", "Rousing", "Sensible", "Sensitive", "Sincere", "Sleek", "Sociable", "Spectacular", "Splendid", "Stellar", "Straightforward", "Stunning", "Stupendous", "Super", "Sympathetic", "Technological", "Thoughtful", "Tidy", "Tough", "Trustworthy", "Twinkling", "Unassuming", "Understanding", "Upbeat", "Versatile", "Vibrant", "Vivacious", "Vivid", "Willing", "Witty", "Wondrous" };
            string[] nounList = { "Actor", "Gold", "Painting", "Advertisement", "Grass", "Parrot", "Afternoon", "Greece", "Pencil", "Airport", "Guitar", "Piano", "Ambulance", "Hair", "Pillow", "Animal", "Hamburger", "Pizza", "Answer", "Helicopter", "Planet", "Apple", "Helmet", "Plastic", "Army", "Holiday", "Portugal", "Australia", "Honey", "Potato", "Balloon", "Horse", "Queen", "Banana", "Hospital", "Quill", "Battery", "House", "Rain", "Beach", "Hydrogen", "Rainbow", "Beard", "Ice", "Raincoat", "Bed", "Insect", "Refrigerator", "Belgium", "Insurance", "Restaurant", "Boy", "Iron", "River", "Branch", "Island", "Rocket", "Breakfast", "Jackal", "Room", "Brother", "Jelly", "Rose", "Camera", "Jewellery", "Russia", "Candle", "Jordan", "Sandwich", "Car", "Juice", "School", "Caravan", "Kangaroo", "Scooter", "Carpet", "King", "Shampoo", "Cartoon", "Kitchen", "Shoe", "China", "Kite", "Soccer", "Church", "Knife", "Spoon", "Crayon", "Lamp", "Stone", "Crowd", "Lawyer", "Sugar", "Daughter", "Leather", "Sweden", "Death", "Library", "Teacher", "Denmark", "Lighter", "Telephone", "Diamond", "Lion", "Television", "Dinner", "Lizard", "Tent", "Disease", "Lock", "Thailand", "Doctor", "London", "Tomato", "Dog", "Lunch", "Toothbrush", "Dream", "Machine", "Traffic", "Dress", "Magazine", "Train", "Easter", "Magician", "Truck", "Egg", "Manchester", "Uganda", "Eggplant", "Market", "Umbrella", "Egypt", "Match", "Van", "Elephant", "Microphone", "Vase", "Energy", "Monkey", "Vegetable", "Engine", "Morning", "Vulture", "England", "Motorcycle", "Wall", "Evening", "Nail", "Whale", "Eye", "Napkin", "Window", "Family", "Needle", "Wire", "Finland", "Nest", "Xylophone", "Fish", "Nigeria", "Yacht", "Flag", "Night", "Yak", "Flower", "Notebook", "Zebra", "Football", "Ocean", "Zoo", "Forest", "Oil", "Garden", "Fountain", "Orange", "Gas", "France", "Oxygen", "Girl", "Furniture", "Oyster", "Glass", "Garage" };
            string charslist = "1234567890";
            string specialcharslist = "!@#$%^&*()-_=+";

            if (isSpecialCharsEnabled)
            {
                charslist += specialcharslist;
            }
            string p = "";

            //Minimum size of AdjectiveNoun## format, without considering chars at the end, is 7.
            if (size > 9)
            {
                while (true)
                {
                    p += adjlist[rnd.Next(0, adjlist.Length)];
                    p += nounList[rnd.Next(0, nounList.Length)];
                    // Add in two chars at the end. e.g. GreedyPanda15
                    p += charslist[rnd.Next(0, charslist.Length)];
                    p += charslist[rnd.Next(0, charslist.Length)];
                    if (p.Length <= size)
                    {
                        return p;
                    }
                    p = "";
                }
            }
            // A size of 6 to 9 generates just a noun with numbers/specials at the end.
            else if (size >= 6)
            {
                List<string> shorts = new List<string>();
                for (int i = 0; i < adjlist.Length + nounList.Length; i++)
                {
                    if (i < adjlist.Length)
                    {
                        if (adjlist[i].Length < size)
                        {
                            shorts.Add(adjlist[i]);
                        }
                    }
                    else if (i < nounList.Length)
                    {
                        if (nounList[i].Length < size)
                        {
                            shorts.Add(nounList[i]);
                        }
                    }
                }
                while (true)
                {
                    p += shorts[rnd.Next(0, shorts.Count)];
                    while (p.Length < size)
                    {
                        p += charslist[rnd.Next(0, charslist.Length)];
                    }
                    if (p.Length <= size)
                    {
                        return p;
                    }
                    p = "";
                }
            }
            // The password manager should never recommend using a password of less than 6 characters.
            else
            {
                return "";
            }
        }
    }
}
