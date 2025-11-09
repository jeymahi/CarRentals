using CarRentals.API.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentals.API.Test.Test_Helpers
{
    public static class DbHelper
    {
        public static CarRentalContext GetInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<CarRentalContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new CarRentalContext(options);
        }
    }
}
