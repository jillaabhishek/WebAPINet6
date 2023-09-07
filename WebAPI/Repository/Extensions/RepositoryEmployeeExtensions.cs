﻿using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Repository.Extensions.Utility;

namespace Repository.Extensions
{
    public static class RepositoryEmployeeExtensions
    {
        public static IQueryable<Employee> FilterEmployee(this IQueryable<Employee> employees, uint minAge, uint maxAge) =>
            employees.Where(x => x.Age >= minAge && x.Age <= maxAge);

        public static IQueryable<Employee> Search(this IQueryable<Employee> employees, string searchTerm)
        {
            if(string.IsNullOrWhiteSpace(searchTerm))
                return employees;

            return employees.Where(x => x.Name.ToLower().Contains(searchTerm.Trim().ToLower()));
        }          
        
        public static IQueryable<Employee> Sort(this IQueryable<Employee> employees, string orderByQueryString)
        {
            if(string.IsNullOrWhiteSpace(orderByQueryString)) return employees.OrderBy(x => x.Name);

            var orderQuery = OrderQueryBuilder.CreateOrderQuery<Employee>(orderByQueryString);

            if (string.IsNullOrWhiteSpace(orderQuery))
                return employees.OrderBy(e => e.Name);

            return employees.OrderBy(orderQuery);
        }
    }
}
