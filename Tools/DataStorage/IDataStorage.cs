using System.Collections.Generic;
using LAB04.Models;

namespace LAB04.Tools.DataStorage
{
    internal interface IDataStorage
    {
        bool PersonExists(Person person);

        void AddPerson(Person person);

        void RemovePerson(Person person);

        void ApplyChanges();

        List<Person> PersonsList { get; }
    }
}