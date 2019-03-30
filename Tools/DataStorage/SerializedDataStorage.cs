using System;
using System.Collections.Generic;
using System.IO;
using LAB04.Models;
using LAB04.Tools.Managers;

namespace LAB04.Tools.DataStorage
{
    internal class SerializedDataStorage : IDataStorage
    {
        private readonly List<Person> _users;

        internal SerializedDataStorage()
        {
            try
            {
                _users = SerializationManager.Deserialize<List<Person>>(FileFolderHelper.StorageFilePath);
            }
            catch (FileNotFoundException)
            {
                _users = new List<Person>();
                string[] names =
                {
                    "Thad",
                    "Lottie",
                    "Bryant",
                    "Callie",
                    "Vincent",
                    "Delilah",
                    "Julieta",
                    "Tiffany",
                    "Shayne",
                    "Selena",
                    "Lori",
                    "Kaylee",
                    "Chastity",
                    "Kathrin",
                    "Ozie",
                    "Kala",
                    "Nettie",
                    "Drusilla",
                    "Kourtney",
                    "Daisey",
                    "Sasha",
                    "Chet",
                    "Elaina",
                    "Harlan",
                    "Nilda",
                    "Debby",
                    "Grazyna",
                    "Jocelyn",
                    "Ross",
                    "Shelia",
                    "Anjelica",
                    "Albertina",
                    "Marjory",
                    "Art",
                    "Frances",
                    "Thomasena",
                    "Daniela",
                    "Marisol",
                    "Kirsten",
                    "Faviola",
                    "Adriane",
                    "Rita",
                    "Breana",
                    "Jeannie",
                    "Kathy",
                    "Necole",
                    "Alfreda",
                    "Pamelia",
                    "Rubye",
                    "Lizette"
                };
                string[] surnames =
                {
                    "Guzman",
                    "Howe",
                    "Albright",
                    "Tackett",
                    "Woody",
                    "Maloney",
                    "Strong",
                    "Kelley",
                    "Mccullough",
                    "Overton",
                    "Sadler",
                    "Katz",
                    "Espinoza",
                    "Rivers",
                    "Holman",
                    "Goldman",
                    "Mead",
                    "Bland",
                    "Campbell",
                    "Geiger",
                    "Dodd",
                    "Greenwood",
                    "Cardenas",
                    "Maloney",
                    "Rushing",
                    "Waller",
                    "Higgins",
                    "Adkins",
                    "Aguilar",
                    "Tolbert",
                    "Macias",
                    "Hagan",
                    "Dailey",
                    "Aguilar",
                    "Nance",
                    "Childers",
                    "Goins",
                    "Griggs",
                    "Shipley",
                    "Caldwell",
                    "Meadows",
                    "Cowan",
                    "Reynolds",
                    "Copeland",
                    "Rodgers",
                    "Story",
                    "Forrest",
                    "Hoover",
                    "Ohara",
                    "Aldrich"
                };
                Random random = new Random();

                for (int i = 0; i < 50; ++i)
                {
                    DateTime birthday = new DateTime(random.Next(1900, 2010), random.Next(1, 12), random.Next(1, 28));
                    string email = names[i][0] + "." + surnames[i] + "@example.com";

                    Person tmp = new Person(names[i], surnames[i], email, birthday);

                    _users.Add(tmp);
                }

                SaveChanges();
            }
        }

        public bool PersonExists(Person person)
        {
            return _users.Contains(person);
        }

        public void AddPerson(Person person)
        {
            _users.Add(person);
            SaveChanges();
        }

        public void RemovePerson(Person person)
        {
            _users.Remove(person);
            SaveChanges();
        }

        public void ApplyChanges()
        {
            SaveChanges();
        }

        public List<Person> PersonsList
        {
            get { return _users; }
        }

        private void SaveChanges()
        {
            SerializationManager.Serialize(_users, FileFolderHelper.StorageFilePath);
        }
    }
}