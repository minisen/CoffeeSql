using CoffeeSql.Core.EntityDesign.Attributes;
using CoffeeSql.Core.EntityDesign;
using CoffeeSql.Oracle.DbContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeSQL_UnitTest
{
    [TestClass]
    public class OracleTest
    {
        public static string oracleConnStr = "Data Source=(DESCRIPTION = (ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT = 1526))(CONNECT_DATA=(SERVER=dedicated)(SERVICE_NAME=asfc)));User Id=xxxxxxx;Password=xxxxxx;";
        

        /// <summary>
        /// 实体类
        /// </summary>
        [Table("T_Students")]
        public class Student : EntityBase
        {
            [PrimaryKeyBase]
            [ColumnBase]
            public string Id { get; set; }

            [ColumnBase("Name")]
            public string StudentName { get; set; }

            [ColumnBase]
            public int Age { get; set; }
        }

        public class WQSDbContext : OracleDbContext<WQSDbContext>
        {
            public WQSDbContext() : base(oracleConnStr)
            {
                this.OpenQueryCache = false;
                this.Log = context =>
                {
                    Console.WriteLine($"sql:{context.SqlStatement}");
                    Console.WriteLine($"time:{DateTime.Now}");
                };
            }
        }

        [TestMethod]
        public void BaseOperation()
        {
            string newId = Guid.NewGuid().ToString().Replace("-", "");
            using (var dbContext = new WQSDbContext())
            {
                //Add
                dbContext.Add<Student>(new Student { Id = newId, StudentName = "王二", Age = 20 });
                bool isExist = dbContext.Queryable<Student>().Select(s => s.Id).Where(s => s.Id.Equals(newId)).Any();
                Assert.AreEqual(true, isExist);

                //update
                string name = Guid.NewGuid().ToString().Replace("-", "");
                dbContext.Update<Student>(s => new { s.StudentName }, new Student { StudentName = name }).Where(s => s.Id.Equals(newId)).Done();
                isExist = dbContext.Queryable<Student>().Select(s => s.Id).Where(s => s.StudentName.Equals(name)).Any();
                Assert.AreEqual(true, isExist);

                //name = Guid.NewGuid().ToString().Replace("-", "");
                //dbContext.Update<Student>(new Student { Id = newId, StudentName = name });
                //isExist = dbContext.Queryable<Student>().Select(s => s.Id).Where(s => s.StudentName.Equals(name)).Any();
                //Assert.AreEqual(true, isExist);

                //select
                name = Guid.NewGuid().ToString().Replace("-", "");
                int insertCount = 5;
                for (int i = 0; i < insertCount; i++)
                {
                    dbContext.Add<Student>(new Student { Id = Guid.NewGuid().ToString().Replace("-", ""), StudentName = name, Age = 20 });
                }
                var list = dbContext.Queryable<Student>().Select(s => new { s.StudentName, s.Id }).Where(s => s.Age > 10 && s.StudentName.Equals(name)).Paging(1, 10).ToList();
                Assert.AreEqual(insertCount, list.Count);

                //delete
                dbContext.Delete<Student>(s => s.Id.Equals(newId));
                isExist = dbContext.Queryable<Student>().Select(s => s.Id).Where(s => s.Id.Equals(newId)).Any();
                Assert.AreEqual(false, isExist);

                

            }



        }
    }
}
