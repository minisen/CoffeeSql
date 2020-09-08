using CoffeeSql.Core.DbContexts;
using CoffeeSql.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace CoffeeSql.Core.Transaction
{
    /// <summary>
    /// Transaction manager (transaction operation in the main database)
    /// </summary>
    public class DBTransaction
    {
        internal DBTransaction(SqlDbContext DbContext)
        {
            this.DbContext = DbContext;
        }

        private SqlDbContext DbContext { get; set; }

        /// <summary>
        /// Database Transaction
        /// </summary>
        private DbTransaction DbTransaction { get; set; }

        /// <summary>
        /// Synchronize the transaction of the current command to the current transaction of the transaction manager
        /// </summary>
        internal void SyncCommandTransaction()
        {
            DbContext.DbCommand.Transaction = this.DbTransaction;
        }

        /// <summary>
        /// Open a transaction as the current transaction
        /// </summary>
        public void Begin()
        {
            //End current transaction
            this.End();

            //Take the unique write database connection from the database connection pool and create a transaction
            string writeDbConnectionString = DbContext.ConnectionManager.ConnectionString_Write;
            DbConnection writeDbConnection = DbContext.GetDbConnection(writeDbConnectionString);

            //Make sure the write database connection is open
            if (writeDbConnection.State != ConnectionState.Open)
            {
                writeDbConnection.Open();
            }

            this.DbTransaction = writeDbConnection.BeginTransaction();
        }

        /// <summary>
        /// Commit current transaction
        /// </summary>
        public void Commit()
        {
            if (null == this.DbTransaction)
            {
                throw new DbTransactionInvalidException("Current DbTransaction is null, please begin transaction.");
            }

            this.DbTransaction.Commit();
        }

        /// <summary>
        /// Rollback current transaction
        /// </summary>
        public void Rollback()
        {
            if (null == this.DbTransaction)
            {
                throw new DbTransactionInvalidException("Current DbTransaction is null, please begin transaction.");
            }

            this.DbTransaction.Rollback();
        }

        /// <summary>
        /// End the current transaction and release the resources of the current transaction
        /// </summary>
        public void End()
        {
            if (this.DbTransaction != null)
            {
                this.DbTransaction.Dispose();
                this.DbTransaction = null;
            }

            //Close write database connection
            string writeDbConnectionString = DbContext.ConnectionManager.ConnectionString_Write;
            DbConnection writeDbConnection = DbContext.GetDbConnection(writeDbConnectionString);

            if (writeDbConnection.State != ConnectionState.Closed)
            {
                writeDbConnection.Close();
            }
        }

        public void Dispose()
        {
            //Release unmanaged resources
            if (this.DbTransaction != null)
            {
                this.DbTransaction.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
