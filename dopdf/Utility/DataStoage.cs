using dopdf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dopdf.Utility
{
    public static class DataStorage
    {
        public static List<Employee> GetAllEmployees() =>
            new List<Employee>
            {
                new Employee { Name="Соловей", LastName="Turner", Age=35, Gender="Male"},
                new Employee { Name="Воробей", LastName="Markus", Age=22, Gender="Female"},
                new Employee { Name="Луча", LastName="Martins", Age=40, Gender="Male"},
                new Employee { Name="Чангретта", LastName="Packner", Age=30, Gender="Female"},
                new Employee { Name="Дудь", LastName="Doe", Age=45, Gender="Male"}
            };
    }

}
