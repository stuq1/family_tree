using System;
using System.Collections.Generic;

enum Sex
{
    Male = 0,
    Female = 1
}

class Person
{

    public int Id { get; }
    public String FullName { get; }
    public int BirthYear { get; }
    public Sex Sex { get; }

    public Person (String FullName, int BirthYear, Sex Sex)
    {
        this.Id = 0;
        this.FullName = FullName;
        this.BirthYear = BirthYear;
        this.Sex = Sex;
    }

    public Person (Person Person, int Id)
    {
        this.Id = Id;
        this.FullName = Person.FullName;
        this.BirthYear = Person.BirthYear;
        this.Sex = Person.Sex;
    }

}

enum Relationship
{
    None = 0,
    Parent = 1,
    Children = 2,
    Spouse = 3
}

class PersonsGraph
{

    private List<Person> Persons;
    private Dictionary<int, Dictionary<int, Relationship>> Graph;

    public PersonsGraph()
    {
        this.Persons = new List<Person>();
        this.Graph = new Dictionary<int, Dictionary<int, Relationship>>();
    }

    public Person NewPerson(Person Person)
    {
        int Id = this.Persons.Count;
        Person NewPerson = new Person(Person, Id);
        this.Persons.Add(NewPerson);
        this.Graph[Id] = new Dictionary<int, Relationship>();
        return NewPerson;
    }

    private void AddRelationship(int Person1Id, int Person2Id, Relationship Relationship)
    {
        if (Person1Id < 0 ||
            Person1Id >= this.Persons.Count ||
            Person2Id < 0 ||
            Person2Id >= this.Persons.Count)
        {
            return;
        }

        this.Graph[Person1Id][Person2Id] = Relationship;
    }

    public bool RegChildren(int PersonChildrenId, int PersonParent1Id, int PersonParent2Id)
    {
        if (PersonParent1Id == -1 || PersonParent2Id == -1)
            return false;

        if (this.Graph[PersonChildrenId].ContainsValue(Relationship.Parent))
            return false;

        if (this.Graph[PersonChildrenId].ContainsKey(PersonParent1Id) ||
            this.Graph[PersonChildrenId].ContainsKey(PersonParent2Id))
            return false;

        if (this.Persons[PersonParent1Id].BirthYear > this.Persons[PersonChildrenId].BirthYear ||
            this.Persons[PersonParent2Id].BirthYear > this.Persons[PersonChildrenId].BirthYear)
            return false;

        if (this.Persons[PersonParent1Id].Sex == this.Persons[PersonParent2Id].Sex)
            return false;

        this.AddRelationship(PersonChildrenId, PersonParent1Id, Relationship.Children);
        this.AddRelationship(PersonParent1Id, PersonChildrenId, Relationship.Parent);

        this.AddRelationship(PersonChildrenId, PersonParent2Id, Relationship.Children);
        this.AddRelationship(PersonParent2Id, PersonChildrenId, Relationship.Parent);

        return true;
    }

    public bool RegFamily(int Person1Id, int Person2Id)
    {
        if (Person1Id == -1 || Person2Id == -1)
            return false;

        if (this.Persons[Person1Id].Sex == this.Persons[Person2Id].Sex)
            return false;

        Relationship r1, r2;
        this.Graph[Person1Id].TryGetValue(Person2Id, out r1);
        this.Graph[Person2Id].TryGetValue(Person1Id, out r2);
        if (r1 != Relationship.None || r2 != Relationship.None)
            return false;

        this.AddRelationship(Person1Id, Person2Id, Relationship.Spouse);
        this.AddRelationship(Person2Id, Person1Id, Relationship.Spouse);

        return true;
    }

    public List<Person> getParents(int PersonId)
    {
        List<Person> result = new List<Person>();

        foreach (var rec in this.Graph[PersonId])
            if (rec.Value == Relationship.Children)
                result.Add(this.Persons[rec.Key]);

        return result;
    }

    public Person getSpouse(int PersonId)
    {
        foreach (var rec in this.Graph[PersonId])
            if (rec.Value == Relationship.Spouse)
                return this.Persons[rec.Key];
        return null;
    }

    public List<Person> getChildrens(int PersonId)
    {
        List<Person> result = new List<Person>();

        foreach (var rec in this.Graph[PersonId])
            if (rec.Value == Relationship.Parent)
                result.Add(this.Persons[rec.Key]);

        return result;
    }

    public List<Person> getSiblings(int PersonId)
    {
        var parents = getParents(PersonId);
        var siblings = new List<Person>();
        foreach (var person in parents)
        {
            var sib = getChildrens(person.Id);
            foreach (var s in sib)
                if (!siblings.Contains(s))
                    siblings.Add(s);
        }
        siblings.Remove(this.Persons[PersonId]);

        return siblings;
    }

    public List<Person> getUncles(int PersonId)
    {
        var parents = getParents(PersonId);
        var uncles = new List<Person>();
        foreach (var person in parents)
        {
            var sib = getSiblings(person.Id);
            uncles.AddRange(sib);
            foreach (var s in sib)
            {
                var sp = getSpouse(s.Id);
                if (sp != null)
                    uncles.Add(sp);
            }
        }

        return uncles;
    }

    public List<Person> getCousins(int PersonId)
    {
        var uncles = getUncles(PersonId);
        var cousins = new List<Person>();
        foreach (var person in uncles)
        {
            cousins.AddRange(getChildrens(person.Id));
        }

        return cousins;
    }

    public List<Person> getInlaws(int PersonId)
    {
        var spouse = getSpouse(PersonId);
        if (spouse != null)
            return getParents(spouse.Id);
        else
            return new List<Person>();
    }

    public List<Person> GetAll()
    {
        return new List<Person>(this.Persons);
    }

}

class Program
{

    static PersonsGraph graph = new PersonsGraph();

    static void WritePerson(Person Person)
    {
        if (Person == null)
        {
            Console.WriteLine("Никого нет");
            return;
        }

        Console.Write("Номер: " + Person.Id.ToString());
        Console.Write(", ФИО: " + Person.FullName);
        Console.Write(", год рождения: " + Person.BirthYear.ToString());
        Console.WriteLine(", пол: " + Person.Sex.ToString());
    }

    static Person ReadPerson()
    {
        Console.Write("ФИО: ");
        String FullName = Console.ReadLine().ToString();
        Console.Write("Год рождения: ");
        int BirthYear = int.Parse(Console.ReadLine());
        Console.Write("Пол (0 - М, 1 - Ж): ");
        Sex Sex = (Sex)int.Parse(Console.ReadLine());

        return new Person(FullName, BirthYear, Sex);
    }

    static void Main(string[] args)
    {
        Console.WriteLine("Список команд:");
        Console.WriteLine("1: Добавить нового человека");
        Console.WriteLine("2: Показать список всех людей");
        Console.WriteLine("3: Указать супругов");
        Console.WriteLine("4: Добавить паре ребенка");
        Console.WriteLine("5: Показать родителей");
        Console.WriteLine("6: Показать дeтей");
        Console.WriteLine("7: Показать братьев и сестер");
        Console.WriteLine("8: Показать дядей и теть");
        Console.WriteLine("9: Показать двоюродных братьев и сестер");
        Console.WriteLine("10: Показать родителей супруга");

        while (true)
        {
            Console.Write("> ");
            int command = int.Parse(Console.ReadLine());

            switch (command)
            {
                case 1:
                    {
                        Person p = ReadPerson();
                        Console.WriteLine("Номер добавленного человека: " + graph.NewPerson(p).Id.ToString());
                        break;
                    }

                case 2:
                    {
                        foreach (var Persona in graph.GetAll())
                            WritePerson(Persona);
                        break;
                    }

                case 3:
                    {
                        int persona1 = int.Parse(Console.ReadLine());
                        int persona2 = int.Parse(Console.ReadLine());
                        graph.RegFamily(persona1, persona2);
                        break;
                    }

                case 4:
                    {
                        int personac = int.Parse(Console.ReadLine());
                        int persona1 = int.Parse(Console.ReadLine());
                        int persona2 = int.Parse(Console.ReadLine());
                        graph.RegChildren(personac, persona1, persona2);
                        break;
                    }

                case 5:
                    {
                        int persona = int.Parse(Console.ReadLine());
                        foreach (var Persona in graph.getParents(persona))
                            WritePerson(Persona);
                        break;
                    }

                case 6:
                    {
                        int persona = int.Parse(Console.ReadLine());
                        foreach (var Persona in graph.getChildrens(persona))
                            WritePerson(Persona);
                        break;
                    }

                case 7:
                    {
                        int persona = int.Parse(Console.ReadLine());
                        foreach (var Persona in graph.getSiblings(persona))
                            WritePerson(Persona);
                        break;
                    }

                case 8:
                    {
                        int persona = int.Parse(Console.ReadLine());
                        foreach (var Persona in graph.getUncles(persona))
                            WritePerson(Persona);
                        break;
                    }

                case 9:
                    {
                        int persona = int.Parse(Console.ReadLine());
                        foreach (var Persona in graph.getCousins(persona))
                            WritePerson(Persona);
                        break;
                    }

                case 10:
                    {
                        int persona = int.Parse(Console.ReadLine());
                        foreach (var Persona in graph.getInlaws(persona))
                            WritePerson(Persona);
                        break;
                    }

                case 0:
                default:
                    return;
            }


        }

    }
}
