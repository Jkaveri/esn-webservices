using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace JK.Core
{
    public class DB : IDisposable
    {
        #region Fields

        /// <summary>
        /// Connection string
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Connection
        /// </summary>
        private SqlConnection _connection;

        /// <summary>
        /// Data set field
        /// </summary>
        private SqlDataAdapter _da;

        /// <summary>
        /// Transaction using excute batch query
        /// </summary>
        private SqlTransaction _transaction;

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DB()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
            ds = new DataSet();
            Connect();
            Open();
        }

        /// <summary>Constructor width connection string</summary>
        /// <param name="cs">Connection string</param>
        /// 
        public DB(string cs)
        {
            _connectionString = cs;
            ds = new DataSet();
            Connect();
            Open();
        }

        #endregion

        #region Connect db method

        /// <summary>
        /// Connect to db
        /// </summary>
        private void Connect()
        {
            _connection = new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Open Conenction
        /// </summary>
        private void Open()
        {
            if (_connection != null)
            {
                Connect();
                _connection.Open();
            }
            else
            {
                throw new Exception("Connection is null");
            }
        }

        /// <summary>
        /// Close connection
        /// </summary>
        private void Close()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                SqlConnection.ClearAllPools();
            }
            if (Reader != null)
            {
                Reader.Close();
                Reader.Dispose();
            }
        }

        #endregion

        #region Schemas

        /// <summary>
        /// This method get schema of table in database.
        /// This method execute query to get DataReader and Use GetSchemaTable() method for result
        /// </summary>
        /// <param name="tableName">Name of table</param>
        /// <returns>DataTable is schema of table</returns>
        private DataTable GetSchemaTable(string tableName)
        {
            DataTable schema = null;
            try
            {
                if (_connection == null) throw new Exception("Not connect to DB");
                Query("SELECT * FROM " + tableName);
                if (Reader != null)
                    schema = Reader.GetSchemaTable();
            }
            catch (SqlException sql)
            {
                throw new Exception(sql.Message);
            }
            return schema;
        }

        /// <summary>
        /// This method get columns collection
        /// </summary>
        /// <param name="tableName">Name of table to get schema</param>
        /// <returns>DataRowCollection; List column</returns>
        private DataRowCollection GetSchemaColumn(string tableName)
        {
            DataTable schema = GetSchemaTable(tableName);
            return schema.Rows;
        }

        #endregion

        #region Query Method

        /// <summary>
        /// Begin DB Tracsaction 
        /// </summary>
        [Obsolete("This method in contructing...", true)]
        public void BeginTransaction()
        {
            if (_connection == null) throw new Exception("Not connection to DB");
            _transaction = _connection.BeginTransaction();
        }

        /// <summary>
        /// RollBack DB Tracsaction
        /// </summary>
        [Obsolete("This method in contructing...", true)]
        public void RollBack()
        {
            if (_transaction == null) throw new Exception("Transaction is null");
            _transaction.Rollback();
        }

        /// <summary>
        /// Commit transaction
        /// </summary>
        [Obsolete("This method in contructing...", true)]
        public void Commit()
        {
            if (_transaction == null) throw new Exception("Transaction is null");
            _transaction.Commit();
        }

        /// <summary>
        /// Execute query with parameters
        /// @Bug 01: parameters null: 
        /// </summary>
        /// <param name="query">query string</param>
        /// <param name="parameters">parameters</param>
        /// <returns>SqlDataReader</returns>
        public void Query(string query, params object[] parameters)
        {
            if (_connection == null) throw new Exception("No Connection!");
            try
            {
                CloseReader(); // close current reader before execute other query
                SqlCommand com = _connection.CreateCommand();
                com.CommandText = query;
                com.Connection = _connection;
                var reg = new Regex(@"@\w+");
                MatchCollection ps = reg.Matches(query);
                if (ps.Count != parameters.Length) throw new Exception("paremeters not valid");
                int i = 0;
                foreach (Match item in ps)
                {
                    foreach (Capture capture in item.Captures)
                    {
                        com.Parameters.AddWithValue(capture.Value, parameters[i]);
                        i++;
                    }
                }
                Reader = com.ExecuteReader();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Execute query with paremeters list
        /// </summary>
        /// <param name="query">a string; query to execute</param>
        /// <param name="parameters">ArrayList; list of parameters</param>
        /// <returns>SqlDataReader; result of query string</returns>
        public void Query(string query, ArrayList parameters)
        {
            if (_connection == null) throw new Exception("Connection is null");

            try
            {
                CloseReader(); // close current reader before execute other query
                var com = new SqlCommand(query, _connection);
                var reg = new Regex(@"@\w+", RegexOptions.Multiline);

                MatchCollection ps = reg.Matches(query);

                if (ps.Count != parameters.Count) throw new Exception("paremeters not valid");

                int i = 0;
                foreach (Match item in ps)
                {
                    foreach (Capture capture in item.Captures)
                    {
                        object param = parameters[i];
                        com.Parameters.AddWithValue(capture.Value, param);
                        i++;
                    }
                }
                Reader = com.ExecuteReader();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Execute query with pagination
        /// </summary>
        /// <param name="query">
        ///     DBQuery with propety: TableName, PageNum, PageSize, WhereClause,Order, OrderBy
        /// </param>
        /// <returns>void</returns>
        public void Query(DBQuery query)
        {
            try
            {
                if (query == null) throw new Exception("query param is null");
                if (string.IsNullOrEmpty(query.TableName)) throw new Exception("Table name is null or empty");
                
                query.OrderBy = (string.IsNullOrEmpty(query.OrderBy)) ? "" : query.OrderBy;
                CloseReader(); // close current reader before execute other query
                string cmdText;
                if (query.PageNum > 0)
                {
                    string where = (!String.IsNullOrEmpty(query.WhereClause)) ? " WHERE " + query.WhereClause : "";
                    cmdText = "SELECT * FROM ( " +
                              "	SELECT TOP(" + query.PageSize + " * " + query.PageNum + ") " +
                              "	ResultNum = ROW_NUMBER() OVER (ORDER BY " + query.OrderBy + " " + query.Order + "), * " +
                              "	FROM " + query.TableName + where +
                              ") AS PAGINATED " +
                              "WHERE ResultNum > ((" + query.PageNum + " - 1) * " + query.PageSize + ")";
                }
                else
                {
                    cmdText = "SELECT * FROM " + query.TableName;
                    if (!String.IsNullOrEmpty(query.WhereClause))
                    {
                        cmdText += " WHERE " + query.WhereClause;
                    }
                    cmdText += " ORDER BY " + query.OrderBy + " " + query.Order;
                }

                if (_connection != null)
                {
                    if (Reader != null) Reader.Close();
                    var com = new SqlCommand(cmdText, _connection);
                    Reader = com.ExecuteReader();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Execute Update query (Insert, Update) with list parameters
        /// </summary>
        /// <param name="query">string; query string</param>
        /// <param name="parameters">list paramaters</param>
        /// <returns>true if success</returns>
        public bool ExecuteUpdate(string query, params object[] parameters)
        {
            if (_connection == null) throw new Exception("Connection is null");
            try
            {
                CloseReader(); // close current reader before execute other query
                var com = new SqlCommand(query, _connection);
                var reg = new Regex(@"@\w+", RegexOptions.Multiline);

                MatchCollection ps = reg.Matches(query);

                if (ps.Count != parameters.Length)
                    throw new Exception("paremeters not valid");


                int i = 0;
                foreach (Match item in ps)
                {
                    foreach (Capture capture in item.Captures)
                    {
                        com.Parameters.AddWithValue(capture.Value, parameters[i]);
                        i++;
                    }
                }
                int succ = com.ExecuteNonQuery();
                return (succ > 0);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Execute Update query 
        /// </summary>
        /// <param name="query">string; Query string</param>
        /// <param name="parameters">parameters list</param>
        /// <returns>true if success</returns>
        public bool ExecuteUpdate(string query, ArrayList parameters)
        {
            if (_connection == null) throw new Exception("Connection is null");

            try
            {
                CloseReader(); // close current reader before execute other query
                var com = new SqlCommand(query, _connection);
                var reg = new Regex(@"@\w+", RegexOptions.Multiline);

                MatchCollection ps = reg.Matches(query);

                if (ps.Count != parameters.Count) throw new Exception("paremeters not valid");

                int i = 0;
                foreach (Match item in ps)
                {
                    foreach (Capture capture in item.Captures)
                    {
                        object param = parameters[i];
                        com.Parameters.AddWithValue(capture.Value, param);
                        i++;
                    }
                }
                int succ = com.ExecuteNonQuery();
                return (succ > 0);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Create strore procedure to db
        /// </summary>
        /// <param name="name">string; name of procedure</param>
        /// <param name="script">string; script of procedure with parameters list</param>
        /// <param name="dataTypes">List data type of parameters</param>
        /// <returns>true if success</returns>
        /// <example>
        /// <code>
        ///     using(var db = new DB()){
        ///        if( db.CreateSP("GetUserByID",
        ///                     "SELECT * FROM Users Where UserID = @ID",
        ///                     "INT")){
        ///             db.ExecuteSp("GetUserByID","@ID",1);
        ///             if(db.HasResult){
        ///                 while(db.Read()){
        ///                     Console.WriteLine(db.GetInt32(0);
        ///                 }
        ///             }
        ///         }
        ///     }
        /// </code>
        /// </example>
        private bool CreateSp(string name, string script, string dataTypes)
        {
            if (_connection == null) throw new Exception("Connection is null");
            try
            {
                CloseReader(); // close current reader before execute other query
                var reg = new Regex(@"@\w+", RegexOptions.Multiline);
                MatchCollection ps = reg.Matches(script);
                int i = 0;
                var query = new StringBuilder();
                query.AppendFormat("CREATE PROC {0} ", name);
                string[] arrDataTypes = dataTypes.Split(',');
                if (arrDataTypes.Length != ps.Count) throw new Exception("Data type and params not equals");

                foreach (Match item in ps)
                {
                    foreach (Capture capture in item.Captures)
                    {
                        query.AppendLine()
                            .Append(capture.Value)
                            .Append(" ")
                            .Append(arrDataTypes[i].ToString(CultureInfo.InvariantCulture))
                            .Append(",");
                        i++;
                    }
                }
                query.Remove(query.Length - 1, 1);
                query.AppendLine(" AS ")
                    .AppendLine(script);
                var com = new SqlCommand(query.ToString(), _connection);
                int success = com.ExecuteNonQuery();
                return (success > 0);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Execute Store Procedure
        /// </summary>
        /// <param name="name">string; Name Of Procedure</param>
        /// <param name="paramsName">string; list of params, seperate by commas (,)</param>
        /// <param name="parameters">params object[], list of paramters seperate by (,)</param>
        public void ExecuteSp(string name, string paramsName, params object[] parameters)
        {
            if (_connection == null) throw new Exception("Connection is null");
            try
            {
                CloseReader(); // close current reader before execute other query
                var com = new SqlCommand(name, _connection);
                var reg = new Regex(@"@\w+", RegexOptions.Multiline);
                com.CommandType = CommandType.StoredProcedure;
                MatchCollection ps = reg.Matches(paramsName);

                if (ps.Count != parameters.Length)
                    throw new Exception("paremeters not valid");


                int i = 0;
                foreach (Match item in ps)
                {
                    foreach (Capture capture in item.Captures)
                    {
                        com.Parameters.AddWithValue(capture.Value, parameters[i]);
                        i++;
                    }
                }
                Reader = com.ExecuteReader();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Read result if db.HasResult, it same Reader.Read()
        /// </summary>
        /// <returns>true if hasresult</returns>
        public bool Read()
        {
            return Reader.Read();
        }

        /// <summary>
        /// Close Reader
        /// </summary>
        private void CloseReader()
        {
            if (Reader != null)
            {
                Reader.Close();
            }
        }

        #endregion

        #region DataMethod

        /// <summary>
        /// Check If object is update (this update is exsited in db)
        /// </summary>
        /// <param name="obj">object to check</param>
        /// <returns>true if this object is exsited</returns>
        private bool IsUpdate(object obj)
        {
            Type type = obj.GetType();
            PropertyInfo prop = type.GetProperty("IsUpdate");
            if (prop == null) return false;
            if (!prop.CanRead) return false;
            object value = prop.GetValue(obj, null);
            return (bool) value;
        }

        /// <summary>
        /// Retrieved data from db to object
        /// </summary>
        /// <param name="id">a int; object's id</param>
        /// <param name="obj">a object; object which you want to retrieve data</param>
        /// <returns>bool; true if success</returns>
        public bool Retrieve(int id, object obj)
        {
            if (_connection == null) throw new Exception("Not Connection"); //if not connection
            if (obj == null) return false; //if object is null;
            //query 
            var query = "";
            //type of this object            
            var type = obj.GetType(); 
            //get primary key name
            var propPK = type.GetProperty("PrimaryKeyName");
            var pkName = propPK.GetValue(obj, null);
            //Name of this object = name of table in db
            string tableName = type.Name;
            //query string
            query = "SELECT * FROM " + tableName + " WHERE "+pkName+" = @id"; 
            Query(query, id); //execute query with parameter ID
            if(HasResult)
            {
                //read data
                Read();
                //get schema table 
                var schemaTable = Reader.GetSchemaTable();
                if (schemaTable != null)
                {
                    //get rows(rows o day la column tren ban du lieu)
                    var rows = schemaTable.Rows;
                    //duyet tat ca cac cot
                    foreach (DataRow row in rows)
                    {
                        //lay ten cot
                        var columnName = row["ColumnName"].ToString(); 
                        //lay property
                        var property = type.GetProperty((pkName.Equals(columnName)) ? "ID" : columnName); //get property by name
                        //neu property ko ton tai thi bo qua
                        if (property == null) continue;
                        //duyet cac kieu du lieu cua property nay
                        switch (property.PropertyType.Name)
                        {
                            case "Int32":
                                property.SetValue(obj, GetInt32(columnName), null);
                                break;
                            case "Int64":
                                property.SetValue(obj, GetInt32(columnName), null);
                                break;
                            case "Decimal":
                                property.SetValue(obj, GetDecimal(columnName), null);
                                break;
                            case "Boolean":
                                property.SetValue(obj, GetBool(columnName), null);
                                break;
                            case "String":
                                property.SetValue(obj, GetString(columnName), null);
                                break;
                            case "DateTime":
                                property.SetValue(obj, GetDateTime(columnName), null);
                                break;
                            case "Float":
                                property.SetValue(obj, GetFloat(columnName), null);
                                break;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Ham Dung de save 1 doi tuong xuong db.
        /// B1: Get Schema cua bang chua doi tuong do(bang tableName) ten bang = ten duoi tuong vd: Users thi ban phai la Users
        /// B2: Duyet cac cot cua bang do va get property cua bang 
        /// #bug: update table with null value
        /// </summary>
        /// <param name="obj">obj want to save</param>
        /// <returns>true if success and otherwise</returns>
        public bool Save(object obj)
        {
            try
            {
                if (IsUpdate(obj)) //if this existed
                {
                    return UpdateObject(obj);
                }

                return InsertObject(obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Save object's data to database
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="isUpdate">bool; set true for update data, false for insert data</param>
        /// <returns>bool; true if success and otherwise</returns>
        public bool Save(object obj, bool isUpdate)
        {
            if (isUpdate)
            {
                return UpdateObject(obj);
            }
            return InsertObject(obj);
        }

        [Obsolete("This medthod is constructing...", true)]
        public bool Save(IEnumerable<object> objs, bool isUpdate)
        {
            if (isUpdate)
            {
            }
            else
            {
                return InsertObject(objs);
            }
            return false;
        }

        /// <summary>
        /// Insert object's data to database
        /// </summary>
        /// <param name="obj">object; object want to insert</param>
        /// <returns>bool; true if suceess</returns>
        private bool InsertObject(object obj)
        {
            object id = 0;
            string query = "";
            var parameters = new ArrayList();

            /** if not connection **/
            if (_connection == null) throw new Exception("Not Connection");
            //get object type for reflection
            Type type = obj.GetType();
            // table name            
            string tableName = type.Name;
            //
            DataRowCollection schemaColumn = GetSchemaColumn(tableName);
            query = "INSERT INTO " + tableName + "( "; // insert query
            string values = "", columns = "";
            //values for value range @[ColumnName]. columns string for columns name
            //duyet tat ca columns
            foreach (DataRow row in schemaColumn)
            {
                //get column name
                string columnName = row["ColumnName"].ToString();
                //lay gia tri allow null trong schema
                var allowNull = (bool) row["AllowDBNull"];
                //lay gia tri la identity
                var isIdentity = (bool) row["IsIdentity"];
                //get property by name
                PropertyInfo property = type.GetProperty(columnName);
                //if property not null va cot do' khong phai la identity 
                if (property != null && !isIdentity)
                {
                    //get column value
                    object value = property.GetValue(obj, null);
                    //xu ly null value
                    //if this columns not allow null value
                    if (!allowNull)
                    {
                        //khong cho phep null ma value null thi throw loi 
                        if (value == null) throw new Exception("Column " + columnName + " not accept null!");
                        //khi gia tri = date time (dieu kien o tren se dung mat du khong set gia tri)
                        if (property.PropertyType.Name == "DateTime")
                        {
                            // luc nay set neu value == DateTime.MinValue thi thuoc tinh do' chinh' xac la null :)
                            if (((DateTime) value) == DateTime.MinValue)
                                throw new Exception("Column " + columnName + " not accept null!");
                        }
                    }
                    //neu cho phep null thi bo qua thuoc tin nay :)
                    //@updated: 2012/06/07
                    //@fixed: not execute default on sqlserver
                    if (value == null) continue;
                    //add columnname to insert string
                    columns += "[" + columnName + "]" + ",";
                    //add column parameter to insert string
                    values += " @" + columnName + ",";
                    //xu li gia tri null
                    NullParameterHandler(property.PropertyType.Name, ref value);
                    //add gia tri vao parameters
                    parameters.Add(value);
                }
            }
            //bo dau , cuoi cung
            columns = columns.Trim(',');
            //bo day , cuoi cung
            values = values.Trim(',');
            //+ 2 gia tri o tren de co cau insert hoang chinh
            // execute them ham SELECT SCOPE_IDENTITY(); de lay identity cuoi cung cua bang (cung la id cua duoi tuong do)
            query += columns + ") VALUES (" + values + "); SELECT SCOPE_IDENTITY();";
            //execute query
            Query(query, parameters);
            //if co ket qua
            if (HasResult)
            {
                Read();
                //get new id
                int newID = GetInt32(0);
                //get id property by name
                PropertyInfo propID = type.GetProperty("ID");
                if (propID != null)
                {
                    //set value for property
                    propID.SetValue(obj, newID, null);
                    return true;
                }
                throw new Exception("Must have ID property");
            }
            return true;
        }

        [Obsolete("This medthod is constructing...", true)]
        private bool InsertObject(List<object> objs)
        {
            string query = "";
            int i;
            string tableName = "";
            DataRowCollection rows;
            bool success = false;
            if (objs != null && objs.Count > 0)
            {
                Type type;
                object o = objs[0];
                type = o.GetType();
                tableName = type.Name;
                rows = GetSchemaColumn(tableName);
                query += "INSERT INTO " + tableName + " ( "; // insert query
                var parameters = new ArrayList();
                i = 0;
                foreach (DataRow row in rows)
                {
                    string columnName = row["ColumnName"].ToString(); //get column name
                    var isIdentity = (bool) row["IsIdentity"];
                    PropertyInfo property = type.GetProperty(columnName); //get property by name
                    if (property != null && !isIdentity)
                    {
                        string comas = (i == rows.Count - 1) ? "" : ", "; //LAST COLUMN NOT COMAS
                        query += columnName + comas;
                    }
                    i++;
                }
                query += " ) ";
                query += "OUTPUT inserted.id INTO new_identities "; //put new identity into new_identities table
                i = 0;
                foreach (object obj in objs)
                {
                    /** if not connection **/
                    if (_connection == null) throw new Exception("Not Connection");
                    string values = "SELECT ";
                    //values for value range @[ColumnName]. columns string for columns name
                    //
                    int j = 0;
                    foreach (DataRow row in rows)
                    {
                        string columnName = row["ColumnName"].ToString(); //get column name
                        var allowNull = (bool) row["AllowDBNull"];
                        var isIdentity = (bool) row["IsIdentity"];
                        PropertyInfo property = type.GetProperty(columnName); //get property by name
                        if (property != null && !isIdentity)
                        {
                            object value = property.GetValue(obj, null);
                            if (!allowNull)
                            {
                                if (value == null) throw new Exception("Column " + columnName + " not accept null!");
                                if (property.PropertyType.Name == "DateTime")
                                {
                                    if (((DateTime) value) == DateTime.MinValue)
                                        throw new Exception("Column " + columnName + " not accept null!");
                                }
                            }
                            string comas = (j == rows.Count - 1) ? "" : ", "; //LAST COLUMN NOT COMAS
                            values += " @" + columnName + i + comas;
                            NullParameterHandler(property.PropertyType.Name, ref value);
                            parameters.Add(value);
                        }
                        j++;
                    }
                    i++;
                    string union = (i == objs.Count) ? "" : " UNION ALL ";
                    query += values + union;
                }
                success = ExecuteUpdate(query, parameters);
                //Query("select * from new_identities");
                /*                if (HasResult)
                                {
                                    Read();
                                    int newID = GetInt32(0);
                                    PropertyInfo propID = type.GetProperty("ID");
                                    if (propID != null)
                                    {
                                        propID.SetValue(obj, newID, null);
                                        return true;
                                    }
                                    throw new Exception("Must have ID property");
                                }*/
            }


            return success;
        }

        /// <summary>
        /// Update object's data to database
        /// </summary>
        /// <param name="obj">object; object want to update</param>
        /// <returns>bool; true if success</returns>
        private bool UpdateObject(object obj)
        {
            object id = 0;
            string query = "";
            var parameters = new ArrayList();
            /** if not connection **/
            if (_connection == null) throw new Exception("Not Connection");
            //get type for reflection
            var type = obj.GetType();
            //get primary key
            var propPrimarykey = type.GetProperty("PrimaryKeyName");
            //get priamry key name
            var primaryKeyName = propPrimarykey.GetValue(obj, null).ToString();
            // table name
            var tableName = type.Name;
            var rows = GetSchemaColumn(tableName);
            //query string
            query += "UPDATE " + tableName + " SET ";
            //duyet tat ca cac cot
            int i = 0;

            foreach (DataRow row in rows)
            {
                // column's name
                i++;
                if (row == null) continue;

                string columnName = row["ColumnName"].ToString();
                var allowNull = (bool) row["AllowDBNull"];
                //column's data type
                //  string dataType = row["DataType"].ToString();
                //get property by name
                var property = type.GetProperty((primaryKeyName.Equals(columnName)) ? "ID" : columnName);
                if (property != null)
                {
                    object value;
                    //lay primary key de update
                    if (columnName.Equals(primaryKeyName))
                    {
                        //get value
                        value = property.GetValue(obj, null);
                        id = value;
                    }
                    else
                    {
                        value = property.GetValue(obj, null); //get value
                        //if this column allow null


                        if (!allowNull)
                        {
                            if (value == null) continue;
                            if (property.PropertyType.Name == "DateTime")
                            {
                                if (((DateTime) value) == DateTime.MinValue) continue;
                            }
                        }
                        //cong column nam vao cau update
                        query += " [" + columnName + "] = @" + columnName;
                        //xu li gia tri null khi ma cot cho phep null 
                        NullParameterHandler(property.PropertyType.Name, ref value);
                        //add parameter
                        parameters.Add(value);
                        query += ",";
                    }
                }
            }
            parameters.Add(id); //add last parameter is id
            query = query.TrimEnd(',');
            query += " WHERE " + primaryKeyName + " = @ID";
            return ExecuteUpdate(query, parameters);
        }

        private bool DetectSpExisted(string name)
        {
            if (_connection == null) throw new Exception("Not connection to DB");
            Query("SELECT OBJECT_ID(@SPNAME);", name);
            if (Read())
            {
                string value = GetString(0);
                return !String.IsNullOrEmpty(value);
            }
            return false;
        }


        /// <summary>
        /// Get Replationship schema of table; Table to get schema is Reference table
        /// </summary>
        /// <param name="tableName">string; Name of <c>Reference</c> table to get schema</param>
        /// <returns>SqlDataReader; [<c>SchemaName</c>, <c>TableName</c>, <c>ColumnName</c>, <c>ReferenceTableName</c>, <c>ReferenceColumnName</c>]</returns>
        private SqlDataReader FindReplationShip(string tableName)
        {
            if (!DetectSpExisted("sp_get_replationship"))
            {
                CreateSp("sp_get_replationship", "SELECT f.name AS ForeignKey, " +
                                                 "SCHEMA_NAME(f.SCHEMA_ID) SchemaName, " +
                                                 "OBJECT_NAME(f.parent_object_id) AS TableName, " +
                                                 "COL_NAME(fc.parent_object_id,fc.parent_column_id) AS ColumnName, " +
                                                 "SCHEMA_NAME(o.SCHEMA_ID) ReferenceSchemaName, OBJECT_NAME (f.referenced_object_id) AS ReferenceTableName, " +
                                                 "COL_NAME(fc.referenced_object_id,fc.referenced_column_id) AS ReferenceColumnName " +
                                                 "FROM sys.foreign_keys AS f " +
                                                 "INNER JOIN sys.foreign_key_columns AS fc ON f.OBJECT_ID = fc.constraint_object_id " +
                                                 "INNER JOIN sys.objects AS o ON o.OBJECT_ID = fc.referenced_object_id " +
                                                 "WHERE OBJECT_NAME (f.referenced_object_id) = @RefTableName",
                         "NVARCHAR(255)");
            }
            ExecuteSp("sp_get_replationship", "@RefTableName", tableName);
            return Reader;
        }

        public bool DeleteReplationShip(object obj, bool forever = false)
        {
            try
            {
                Type type = obj.GetType();
                string tableName = type.Name;
                PropertyInfo idProp = type.GetProperty("ID");
                object id = idProp.GetValue(obj, null);
                using (SqlDataReader rd = FindReplationShip(tableName))
                {
                    while (rd.Read())
                    {
                        string childTableName = rd.GetString(2),
                               childColumnName = rd.GetString(3);
                        var query = new StringBuilder();

                        query.AppendFormat(
                            forever ? "DELETE {0} WHERE {1}=@ID" : "UPDATE {0} SET Deleted = 1 WHERE {1} = @ID",
                            childTableName, childColumnName);

                        using (var conn = new SqlConnection(_connectionString))
                        {
                            var cm = new SqlCommand(query.ToString(), conn);
                            conn.Open();
                            cm.Parameters.AddWithValue("@ID", id);
                            cm.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Delete object in db(set Deleted column = 1)
        /// </summary>
        /// <param name="obj">Object this</param>
        /// <param name="forever">bool; This object will be absolute delete if set true. in current version this function isn't implemented</param>
        /// <returns>true if success</returns>
        public bool Delete(object obj, bool forever = false)
        {
            //neu obj = null thi nem' loi~ :)
            if (obj == null) throw new Exception("Null object");
            //get type for reflection
            var type = obj.GetType();
            //get prymary key name
            var propPriamryKey = type.GetProperty("PrimaryKeyName");
            var priamryKeyName = propPriamryKey.GetValue(obj, null);
            //get tablename = ten class
            var tableName = type.Name;
            //lay id property
            var idProp = type.GetProperty("ID");
            //neu id property null thi quan loi~ :))
            if (idProp == null) throw new Exception("ID property not exist, add this or override Delete method");
            //neu property nay co the doc
            DeleteReplationShip(obj, forever); // delete replationship
            //set prop id
            var id = idProp.GetValue(obj, null);
            //query
            var query = new StringBuilder();
            //generate query string
            query.AppendFormat(
                !forever ?
                //neu khong delete vinh vien thi update lai
                "UPDATE {0} SET Deleted = 1 WHERE {1} = @ID" :
                //neu delete vinh vien thi thuc thi cau delete luon
                "DELETE {0} WHERE {1} = @ID", tableName, priamryKeyName);

            return ExecuteUpdate(query.ToString(), id);
        }

        #endregion

        #region FillDataSet Methods

        /**
         * Fill data to dataset 
         * <param name="query">SQl query string</query>
         */

        public void FillDataSet(string query)
        {
            try
            {
                _da = new SqlDataAdapter(query, _connection);
                _da.Fill(ds);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /**
         * <summary>Fill data to dataset with table name</summary>
         * <param name="query">Sql query string</param>
         * <param name="name">Table name</param>
         */

        public void FillDataSet(string query, string name)
        {
            try
            {
                _da = new SqlDataAdapter(query, _connection);
                _da.Fill(ds, name);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        /**
         * <summary>Fill data to dataset with parameter in query string</summary>
         * <param name="query">Sql query string</param>
         * <param name="parameters">parameter with param</param>
         */

        public void FillDataSet(string query, params object[] parameters)
        {
            try
            {
                var com = new SqlCommand(query, _connection);
                var reg = new Regex(@"@\w+");
                MatchCollection ps = reg.Matches(query);
                int i = 0;
                if (parameters.Length == ps.Count)
                {
                    foreach (Match item in ps)
                    {
                        foreach (Capture capture in item.Captures)
                        {
                            com.Parameters.AddWithValue(capture.Value, parameters[i]);
                            i++;
                        }
                    }
                    _da.SelectCommand = com;
                    _da.Fill(ds);
                }
                else
                {
                    throw new Exception("\"parameters\" params not valid");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void FillDataSet(string query, string name, params object[] parameters)
        {
            try
            {
                var com = new SqlCommand(query, _connection);
                var reg = new Regex(@"@\w+");
                MatchCollection ps = reg.Matches(query);
                int i = 0;
                foreach (Match item in ps)
                {
                    foreach (Capture capture in item.Captures)
                    {
                        com.Parameters.AddWithValue(capture.Value, parameters[i]);
                        i++;
                    }
                }
                _da.SelectCommand = com;
                _da.Fill(ds, name);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region Get Value

        /// <summary>
        /// Get String Value In Result Of Query (SqlDataReader)
        /// this is another way of Reader.GetString(0); but it exclude null exception
        /// </summary>
        /// <param name="index">index of result</param>
        /// <returns>a string</returns>
        public string GetString(int index)
        {
            if (Reader == null)
            {
                throw new Exception("Reader is null");
            }
            if (Reader[index] == null)
            {
                return "";
            }
            return Reader[index].ToString();
        }

        /// <summary>
        /// Get string by name
        /// </summary>
        /// <param name="name">name of column in result </param>
        /// <returns>a string;</returns>
        /// <exception cref="Exception">Reader is null</exception>
        public string GetString(string name)
        {
            try
            {
                if (Reader == null)
                {
                    throw new Exception("Reader is null");
                }
                if (Reader[name] == null)
                {
                    return "";
                }
                return Reader[name].ToString();
            }
            catch (IndexOutOfRangeException ex)
            {
                return "";
            }
        }

        /// <summary>
        /// GetInt32 Exclude null value exception by Index
        /// </summary>
        /// <param name="index">index of column</param>
        /// <returns>a number or 0 if this column not result</returns>
        public int GetInt32(int index)
        {
            string input = GetString(index);
            int output = 0;
            return Int32.TryParse(input, out output) ? output : 0;
        }

        /// <summary>
        ///  GetInt32 Exclude null value exception by Index
        /// </summary>
        /// <param name="name">Name of column</param>
        /// <returns>int; number (0 if column is null) </returns>
        public int GetInt32(string name)
        {
            string input = GetString(name);
            int output = 0;
            return Int32.TryParse(input, out output) ? output : 0;
        }

        /// <summary>
        /// Get Int64 value
        /// </summary>
        /// <param name="index">Index of column</param>
        /// <returns>Int64; number(0 if colunm is null)</returns>
        public long GetInt64(int index)
        {
            string input = GetString(index);
            long output = 0;
            return long.TryParse(input, out output) ? output : 0;
        }

        /// <summary>
        /// Get Int64 value by Name
        /// </summary>
        /// <param name="name">string; Name of column</param>
        /// <returns>long</returns>
        public long GetInt64(string name)
        {
            string input = GetString(name);
            long output = 0;
            return long.TryParse(input, out output) ? output : 0;
        }

        /// <summary>
        /// Get Float Float Value
        /// </summary>
        /// <param name="index">string; Name of index</param>
        /// <returns>float value</returns>
        public float GetFloat(int index)
        {
            string input = GetString(index);
            float output = 0;
            if (float.TryParse(input, out output))
            {
                return output;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get Float 
        /// </summary>
        /// <param name="name">string; Name of column</param>
        /// <returns>float column</returns>
        public float GetFloat(string name)
        {
            string input = GetString(name);
            float output = 0;
            if (float.TryParse(input, out output))
            {
                return output;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get Decimal Value by index
        /// </summary>
        /// <param name="index">int; index</param>
        /// <returns>decimal</returns>
        public decimal GetDecimal(int index)
        {
            string input = GetString(index);
            decimal output = 0;
            return Decimal.TryParse(input, out output) ? output : 0;
        }

        /// <summary>
        /// Get decimal value by name
        /// </summary>
        /// <param name="name">string; name</param>
        /// <returns>decimal</returns>
        public decimal GetDecimal(string name)
        {
            string input = GetString(name);
            decimal output = 0;
            return Decimal.TryParse(input, out output) ? output : 0;
        }

        /// <summary>
        /// Get bool value by index
        /// </summary>
        /// <param name="index">index of column</param>
        /// <returns>bool; true or false</returns>
        public bool GetBool(int index)
        {
            string input = GetString(index);
            bool output = false;
            return Boolean.TryParse(input, out output) && output;
        }

        /// <summary>
        /// Get bool value by name
        /// </summary>
        /// <param name="name">string; name of column</param>
        /// <returns>bool;</returns>
        public bool GetBool(string name)
        {
            string input = GetString(name);
            bool output = false;
            return Boolean.TryParse(input, out output) && output;
        }

        /// <summary>
        /// Get DateTime value by index 
        /// </summary>
        /// <param name="index">int; index</param>
        /// <returns>DateTime;</returns>
        public DateTime GetDateTime(int index)
        {
            string input = GetString(index);
            DateTime output;
            return DateTime.TryParse(input, out output) ? output : DateTime.MinValue;
        }

        /// <summary>
        /// Get DateTime value by name
        /// </summary>
        /// <param name="name">string; name</param>
        /// <returns>DateTime</returns>
        public DateTime GetDateTime(string name)
        {
            string input = GetString(name);
            DateTime output;
            return DateTime.TryParse(input, out output) ? output : DateTime.MinValue;
        }

        /// <summary>
        /// Auto set data got in db(after execute query) to object 
        /// </summary>
        /// <param name="obj">object to set</param>
        public void SetValues(object obj)
        {
            /** if not reader null **/
            if (obj == null) throw new NullReferenceException("Object is null ");
            Type type = obj.GetType();
            //get primary key name
            var propPK = type.GetProperty("PrimaryKeyName");
            var keyName = propPK.GetValue(obj, null).ToString();
            if (Reader != null && Reader.HasRows)
            {
                DataTable schemaTable = Reader.GetSchemaTable();
                if (schemaTable != null)
                {
                    DataRowCollection rows = schemaTable.Rows;
                    foreach (DataRow row in rows)
                    {
                        var columnName = row["ColumnName"].ToString(); //get column name
                        var property = type.GetProperty((keyName.Equals(columnName))?"ID":columnName); //get property by name
                        if (property == null) continue;
                        if(property.PropertyType.IsEnum)
                        {
                            property.SetValue(obj,GetInt32(columnName),null);
                            continue;
                        }
                        switch (property.PropertyType.Name)
                        {
                            case "Int32":
                                property.SetValue(obj, GetInt32(columnName), null);
                                break;
                            case "Int64":
                                property.SetValue(obj, GetInt32(columnName), null);
                                break;
                            case "Decimal":
                                property.SetValue(obj, GetDecimal(columnName), null);
                                break;
                            case "Boolean":
                                property.SetValue(obj, GetBool(columnName), null);
                                break;
                            case "String":
                                property.SetValue(obj, GetString(columnName), null);
                                break;
                            case "DateTime":
                                property.SetValue(obj, GetDateTime(columnName), null);
                                break;
                            case "Float":
                                property.SetValue(obj, GetFloat(columnName), null);
                                break;
                        }
                    }
                }
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Implement IDisposable
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        #endregion

        #region Properties

        /// <summary>
        /// DataSet
        /// </summary>
        public DataSet ds { get; set; }

        /// <summary>
        /// Data Reader contain result of query
        /// </summary>
        public SqlDataReader Reader { get; set; }

        /// <summary>
        /// Time of now
        /// </summary>
        public DateTime Now
        {
            get { return DateTime.Now; }
        }

        /// <summary>
        /// Check if connection is opening
        /// </summary>
        public bool IsOpen
        {
            get { return _connection.State == ConnectionState.Open; }
        }

        /// <summary>
        /// Check if query has the result
        /// </summary>
        public bool HasResult
        {
            get { return (Reader != null && Reader.HasRows); }
        }

        #endregion

        /// <summary>
        /// Hanlde null paramater for insert to db
        /// </summary>
        /// <param name="type">type, datatype name</param>
        /// <param name="param">Param to handle</param>
        private void NullParameterHandler(string type, ref object param)
        {
            switch (type)
            {
                case "String":
                    if (param == null)
                        param = SqlString.Null;
                    break;
                case "DateTime":
                    var d = (DateTime) param;
                    if (d < SqlDateTime.MinValue.Value)
                        param = SqlDateTime.Null;
                    break;
                default:
                    break;
            }
        }
    }
}