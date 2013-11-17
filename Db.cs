using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;
using System.Data;


namespace PowerDotNet {
    public class Db : IDisposable {

        public class Request : IDisposable {
            private Db db;
            private bool disposed;
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            public Request() {
                db = new Db();
            }

            public int ExecNonQueryProc(string proc) {
                return db.ExecNonQueryProc(proc, sqlParams.ToArray());
            }

            public object ExecScalarProc(string proc)
            {
                return db.ExecScalarProc(proc, sqlParams.ToArray());
            }

            public DataSet ExecDataSetProc(string proc)
            {
                DataSet ds = db.ExecDataSetProc(proc, sqlParams.ToArray());
                return ds;
            }

            public Dictionary<string, object> ExecDataToDictionary(string proc)
            {
                Dictionary<string, object> results = new Dictionary<string, object>();
                DataSet ds = db.ExecDataSetProc(proc, sqlParams.ToArray());
                if (ds != null && ds.Tables[0] != null)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataColumn dc in ds.Tables[0].Columns)
                        {
                            results.Add(dc.ColumnName, ds.Tables[0].Rows[0][dc.ColumnName]);
                        }
                    }
                }
                ds.Dispose();

                return results;
            }

            public List<T> ExecDataToModel<T>(string proc) {
                List<T> result = new List<T>();
                SqlDataReader dr = db.ExecDataReaderProc(proc, sqlParams.ToArray());

                while (dr.Read()) {

                    var item = Activator.CreateInstance(typeof(T));

                    foreach (System.Reflection.FieldInfo info in item.GetType().GetFields())
                    {

                        //var cols = dr.FieldCount;
                        //int colCounter = 0;
                        //while (colCounter < cols)
                        //{
                        //var type = dr[colCounter].GetType();
                        //var name = dr.GetName(colCounter);
                        object rowColumn = DBNull.Value;
                        try
                        {
                            rowColumn = dr[info.Name];
                        }
                        catch { }
                        if (rowColumn != DBNull.Value)
                        {
                            var fieldTypeName = info.FieldType.Name;
                            switch (fieldTypeName)
                            {
                                case "String":
                                    info.SetValue(item, dr[info.Name].GetString());
                                    break;
                                case "Int32":
                                    info.SetValue(item, dr[info.Name].GetInt());
                                    break;
                                default:
                                    info.SetValue(item, dr[info.Name]);
                                    break;
                            }

                        }

                        //colCounter++;
                        //}
                    }

                    foreach (System.Reflection.PropertyInfo info in item.GetType().GetProperties())
                    {

                        //var cols = dr.FieldCount;
                        //int colCounter = 0;
                        //while (colCounter < cols)
                        //{
                        //var type = dr[colCounter].GetType();
                        //var name = dr.GetName(colCounter);
                        object rowColumn = DBNull.Value;
                        try
                        {
                            rowColumn = dr[info.Name];
                        }
                        catch { }
                        if (rowColumn != DBNull.Value)
                        {
                            var fieldTypeName = info.PropertyType.Name;
                            switch (fieldTypeName)
                            {
                                case "String":
                                    info.SetValue(item, dr[info.Name].GetString());
                                    break;
                                case "Int32":
                                    info.SetValue(item, dr[info.Name].GetInt());
                                    break;
                                default:
                                    info.SetValue(item, dr[info.Name]);
                                    break;
                            }

                        }

                        //colCounter++;
                        //}
                    }

                    result.Add((T)item);
                }

                dr.Close();
                dr.Dispose();

                return result;
            }

            public SqlDataReader ExecDataReaderProc(string proc) {
                return db.ExecDataReaderProc(proc, sqlParams.ToArray());
            }

            public void AddParam(string name, object value, System.Data.DbType type) {
                SqlParameter param = new SqlParameter(name, value);
                param.DbType = type;
                sqlParams.Add(param);
            }

            public void Dispose() {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing) {
                if (!disposed) {
                    if (disposing) {

                        db.Dispose();
                    }
                    disposed = true;
                }
            }
        }
        protected string _connectionString;
        protected SqlConnection Connection;
        protected SqlTransaction _transasction;
        protected bool Disposed;

        public static string ConnectionString { get; set; }

        public SqlTransaction Transaction {
            get { return _transasction; }
        }

        public Db() {
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnectionString"].ConnectionString;
            Connect();
        }

        public Db(string connString) {
            _connectionString = connString;
            Connect();
        }

        protected void Connect() {
            Connection = new SqlConnection(_connectionString);
            Connection.Open();
        }

        public SqlCommand CreateCommand(string qry, CommandType type, params object[] args) {
            SqlCommand cmd = new SqlCommand(qry, Connection);

            // Associate with current transaction, if any
            if (_transasction != null)
                cmd.Transaction = _transasction;

            // Set command type
            cmd.CommandType = type;

            // Construct SQL parameters
            if (args != null) {
                for (int i = 0; i < args.Length; i++) {
                    if (args[i] is string && i < (args.Length - 1)) {
                        SqlParameter parm = new SqlParameter();
                        parm.ParameterName = (string)args[i];
                        parm.Value = args[++i];
                        cmd.Parameters.Add(parm);
                    } else if (args[i] is SqlParameter) {
                        cmd.Parameters.Add((SqlParameter)args[i]);
                    } else throw new ArgumentException("Invalid number or type of arguments supplied");
                }
            }
            return cmd;
        }

        public int ExecNonQuery(string qry, params object[] args) {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args)) {
                return cmd.ExecuteNonQuery();
            }
        }

        public int ExecNonQueryProc(string proc, params object[] args) {
            using (SqlCommand cmd = CreateCommand(proc, CommandType.StoredProcedure, args)) {
                return cmd.ExecuteNonQuery();
            }
        }

        public object ExecScalar(string qry, params object[] args) {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args)) {
                return cmd.ExecuteScalar();
            }
        }

        public object ExecScalarProc(string qry, params object[] args) {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.StoredProcedure, args)) {
                return cmd.ExecuteScalar();
            }
        }

        public SqlDataReader ExecDataReader(string qry, params object[] args) {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args)) {
                return cmd.ExecuteReader();
            }
        }

        public SqlDataReader ExecDataReaderProc(string qry, params object[] args) {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.StoredProcedure, args)) {
                return cmd.ExecuteReader();
            }
        }

        public DataSet ExecDataSet(string qry, params object[] args) {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args)) {
                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapt.Fill(ds);
                return ds;
            }
        }

        public DataSet ExecDataSetProc(string qry, params object[] args) {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.StoredProcedure, args)) {
                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapt.Fill(ds);
                return ds;
            }
        }

        public SqlTransaction BeginTransaction() {
            Rollback();
            _transasction = Connection.BeginTransaction();
            return Transaction;
        }

        public void Commit() {
            if (_transasction != null) {
                _transasction.Commit();
                _transasction = null;
            }
        }

        public void Rollback() {
            if (_transasction != null) {
                _transasction.Rollback();
                _transasction = null;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!Disposed) {
                if (disposing) {
                    if (Connection != null) {
                        Rollback();
                        Connection.Dispose();
                        Connection = null;
                    }
                }
                Disposed = true;
            }
        }

    }
}