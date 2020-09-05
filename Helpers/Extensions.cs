using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers
{
    public static class Extensions
    {
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            response.Headers.Add("Application-Error", message);
            response.Headers.Add("Access-Control-Expose-Header", "Application-Error");
        }

        public static void AddPagination(this HttpResponse response, int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);
            var camelCaseFormatter = new JsonSerializerSettings();
            camelCaseFormatter.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader, camelCaseFormatter));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }

        public static int CalculateAge(this DateTime DateofBirth)
        {
            var age = DateTime.Now.Year - DateofBirth.Year;
            if (DateTime.Now.AddYears(age) > DateofBirth)
                age--;
            
            return age;
        }
    }
}
