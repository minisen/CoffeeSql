using System;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CoffeeSql.Core;
using CoffeeSql.Core.EntityDesign.Attributes;
using CoffeeSql.Core.SqlDataAccess;
using System.Data.Common;
using System.Data.SqlClient;
using CoffeeSql.Core.EntityDesign;
using CoffeeSql.Mysql;
using CoffeeSql.Core.SqlStatementManagement;
using CoffeeSql.Oracle.Core.DbContext;
using CoffeeSql.Mysql.EntityDesign.FieldDef;
using CoffeeSql.Mysql.EntityDesign.FieldDef.FieldTypeDef;
using CoffeeSql.Mysql.CodeFirst;

namespace test
{
    class Program
    {
        public class EntityCommon : EntityBase
        {
            [Column][PrimaryKey][VarChar(50)][NotNull]
            public string Id { get; set; }

            [Column][VarChar(200)][Default("remarks")]
            public string RemarkInfo { get; set; }

            [Column][TinyInt(1)][NotNull][Default(0)]
            public int DelFlag { get; set; }   //oracle: 不支持bool，使用int；mysql中使用bool；

            [Column][Datetime][Default(Default.CURRENT_TIMESTAMP)][NotNull]
            public DateTime CreateTime { get; set; } = DateTime.Now;

            [Column][Datetime][Default(Default.CURRENT_TIMESTAMP)][NotNull]
            public DateTime UpdateTime { get; set; } = DateTime.Now;

            [Column][VarChar(50)][NotNull]
            public string Creater { get; set; }

            [Column][VarChar(50)][NotNull]
            public string Updater { get; set; }

        }

        [Table("b_dictionary")]
        public class Dictionary : EntityCommon
        {
            [Column]
            [VarChar(50)][NotNull]
            public string Name { get; set; }

            [Column]
            [VarChar(50)][NotNull]
            public string Value { get; set; }
        }

        public class WQSDbContext : MysqlDbContext<WQSDbContext>
        {
            public WQSDbContext() : base("User ID=root;Password=root;Host=localhost;Port=3306;Database=test;")
            {
                this.OpenQueryCache = false;
                this.Log = context =>
                {
                    Console.WriteLine($"sql:{context.SqlStatement}");
                    Console.WriteLine($"time:{DateTime.Now}");
                };
            }
        }

        static void Main(string[] args)
        {
            //codefirst
            //MysqlDDLTextGenerator CodeFirst = new MysqlDDLTextGenerator();
            //string createTableDDL = CodeFirst.CreateTable(typeof(Dictionary));

            using (var dbContext = new WQSDbContext())
            {
                var list = dbContext.Queryable<Dictionary>().Select().Where(d => d.DelFlag == 0).ToList();
            }

        }
    }
}
