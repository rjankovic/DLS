//using System;
//using System.Collections.Generic;
//using System.Data.Common;
//using System.Data.Entity;
//using System.Data.Entity.Migrations.History;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CD.BIDoc.Core.DataLayer
//{
//    public class BIDocHistoryContext : HistoryContext
//    {
//        public BIDocHistoryContext(DbConnection dbConnection, string defaultSchema)
//            : base(dbConnection, defaultSchema)
//        {
//        }

//        protected override void OnModelCreating(DbModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);
//            modelBuilder.HasDefaultSchema("BIDoc");
//        }
//    }
//}
