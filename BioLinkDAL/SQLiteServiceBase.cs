﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using BioLink.Client.Utilities;
using System.IO;

namespace BioLink.Data {

    public class SQLiteServiceBase : IDisposable {

        public String FileName { get; set; }

        private SQLiteConnection _connection;        
        private SQLiteTransaction _transaction;

        public SQLiteServiceBase(string filename, bool persistConnection = false) {
            this.FileName = filename;

            if (!File.Exists(filename)) {
                SQLiteConnection.CreateFile(filename);
                IsNew = true;
            }

            if (persistConnection) {
                _connection = getConnection();
                _connection.Open();
            }

        }

        public void Disconnect() {
            if (_connection != null) {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected SQLiteConnection getConnection() {
            if (_connection != null) {
                return _connection;                
            }            
            return new SQLiteConnection(String.Format("Data Source={0}", FileName));            
        }

        protected void SelectReader(string sql, SqliteReaderDelegate action, params SQLiteParameter[] @params) {

            Logger.Debug("Executing SQLite SelectReader: {0}", sql);
            using (new CodeTimer("SQLite SelectReader")) {
                Command((cmd) => {
                    cmd.CommandText = sql;
                    foreach (SQLiteParameter param in @params) {
                        cmd.Parameters.Add(param);
                    }
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            if (action != null) {
                                action(reader);
                            }
                        }
                    }
                });
            }

        }

        protected T SelectScalar<T>(string sql) {
            T result = default(T);
            Command((cmd) => {
                cmd.CommandText = sql;
                result = (T) cmd.ExecuteScalar();                
            });
            return result;
        }

        protected SQLiteParameter _P(string name, object value) {
            return new SQLiteParameter(name, value);
        }

        protected int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters) {
            int result = 0;
            Command((cmd) => {
                cmd.CommandText = sql;
                foreach (SQLiteParameter p in parameters) {
                    cmd.Parameters.Add(p);
                }
                result = cmd.ExecuteNonQuery();
            });
            return result;
        }

        public void BeginTransaction() {
            if (_connection == null) {
                throw new Exception("Cannot begin a transaction unless the connection is persistent!");
            }

            if (_transaction != null) {
                throw new Exception("Cannot begin a transaction because there is already one outstanding!");
            }

            _transaction = _connection.BeginTransaction();
        }

        public void RollbackTransaction() {
            if (_connection == null) {
                throw new Exception("Cannot rollback a transaction unless the connection is persistent!");
            }

            if (_transaction == null) {
                throw new Exception("Cannot rollback because there is no transaction outstanding!");
            }

            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }

        public void CommitTransaction() {
            if (_connection == null) {
                throw new Exception("Cannot commit a transaction unless the connection is persistent!");
            }

            if (_transaction == null) {
                throw new Exception("Cannot commit because there is no transaction outstanding!");
            }

            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;

        }

        protected void Command(SqliteCommandDelegate action) {
            if (action == null) {
                return;
            }
            SQLiteConnection conn = getConnection();
            try {
                if (conn.State == System.Data.ConnectionState.Closed) {
                    conn.Open();
                }
                using (SQLiteCommand cmd = conn.CreateCommand()) {
                    action(cmd);
                }

            } finally {
                if (_connection == null) {
                    conn.Dispose();
                }
            }
        }

        public bool IsNew { get; private set; }

        public void Dispose() {
            if (_connection != null) {
                if (_transaction != null) {
                    _transaction.Rollback();
                    _transaction.Dispose();
                    _transaction = null;
                }

                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }
    }

    public delegate void SqliteCommandDelegate(SQLiteCommand command);

    public delegate void SqliteReaderDelegate(SQLiteDataReader reader);

}
