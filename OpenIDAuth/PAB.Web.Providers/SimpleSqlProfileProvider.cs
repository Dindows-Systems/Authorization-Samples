
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Profile;
using System.Configuration;
using System.Configuration.Provider;

namespace PAB.Web.Providers {

    public sealed class SimpleSqlProfileProvider : ProfileProvider {

        private const string DbObjectNameFormat = "^[a-zA-Z0-9_]{1,}$";
        private const string CustomProviderDataFormat = "^[a-zA-Z0-9_]+;[a-zA-Z0-9_]+(;[0-9]{1,})?$";

        // Initialization and configuration

        private string applicationName, connectionString, tableName, keyColumnName, lastUpdateColumnName;

        private System.Collections.Specialized.NameValueCollection configuration;

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config) {
            // Validate arguments
            if (config == null) throw new ArgumentNullException("config");
            if (string.IsNullOrEmpty(name)) name = "SimpleSqlProfileProvider";
            if (String.IsNullOrEmpty(config["description"])) {
                config.Remove("description");
                config.Add("description", "PAB simple SQL profile provider");
            }

            // Initialize base class
            base.Initialize(name, config);

            // Basic init
            this.configuration = config;
            this.applicationName = GetConfig("applicationName", "");

            // Initialize connection string
            ConnectionStringSettings ConnectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];
            if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "") throw new ProviderException("Connection string cannot be blank.");
            this.connectionString = ConnectionStringSettings.ConnectionString;

            // Initialize table name
            this.tableName = GetConfig("tableName", "Profiles");
            if (!IsValidDbObjectName(this.tableName)) throw new ProviderException("Table name contains illegal characters.");

            // Initialize key column name
            this.keyColumnName = GetConfig("keyColumnName", "UserName");
            if (!IsValidDbObjectName(this.keyColumnName)) throw new ProviderException("Key column name contains illegal characters.");

            // Initialize last update column name
            this.lastUpdateColumnName = GetConfig("lastUpdateColumnName", "LastUpdate");
            if (!IsValidDbObjectName(this.lastUpdateColumnName)) throw new ProviderException("Last update column name contains illegal characters.");
        }

        public override string ApplicationName {
            get { return this.applicationName; }
            set { this.applicationName = value; }
        }

        public string TableName {
            get { return tableName; }
        }

        public string KeyColumnName {
            get { return this.keyColumnName; }
        }

        public string LastUpdateColumnName {
            get { return this.lastUpdateColumnName; }
        }

        // Profile provider implementation

        public override int DeleteProfiles(string[] usernames) {
            if (usernames == null) throw new ArgumentNullException();
            if (usernames.Length == 0) return 0; // no work here

            int count = 0;
            SqlConnection db = OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand(this.ExpandCommand("DELETE FROM $Profiles WHERE $UserName=@UserName"), db);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100);
                foreach (string userName in usernames) {
                    cmd.Parameters["@UserName"].Value = userName;
                    count += cmd.ExecuteNonQuery();
                }
            }
            finally {
                db.Close();
            }
            return count;
        }

        public override int DeleteProfiles(ProfileInfoCollection profiles) {
            if (profiles == null) throw new ArgumentNullException();
            if (profiles.Count == 0) return 0; // no work here

            int count = 0;
            SqlConnection db = OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand(this.ExpandCommand("DELETE FROM $Profiles WHERE $UserName=@UserName"), db);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100);
                foreach (ProfileInfo pi in profiles) {
                    cmd.Parameters["@UserName"].Value = pi.UserName;
                    count += cmd.ExecuteNonQuery();
                }
            }
            finally {
                db.Close();
            }
            return count;
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection) {
            SettingsPropertyValueCollection svc = new SettingsPropertyValueCollection();

            // Validate arguments
            if (collection == null || collection.Count < 1 || context == null) return svc;
            string userName = (string)context["UserName"];
            if (String.IsNullOrEmpty(userName)) return svc;

            // Get profile row from db
            SqlConnection db = OpenDatabase();
            DataTable dt = new DataTable();
            try {
                SqlCommand cmd = new SqlCommand(this.ExpandCommand("SELECT * FROM $Profiles WHERE $UserName=@UserName"), db);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = userName;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                da.Dispose();
            }
            finally {
                db.Close();
            }

            // Process properties
            foreach (SettingsProperty prop in collection) {
                SettingsPropertyValue value = new SettingsPropertyValue(prop);
                if (dt.Rows.Count == 0) {
                    value.PropertyValue = Convert.ChangeType(value.Property.DefaultValue, value.Property.PropertyType);

                    value.IsDirty = false;
                    value.Deserialized = true;
                }
                else {
                    string columnName = GetPropertyMapInfo(prop).ColumnName; if (dt.Columns.IndexOf(columnName) == -1) throw new ProviderException(string.Format("Column '{0}' required for property '{1}' was not found in table '{2}'.", columnName, prop.Name, this.TableName));
                    object columnValue = dt.Rows[0][columnName];

                    if (!(columnValue is DBNull || columnValue == null)) {
                        value.PropertyValue = columnValue;
                        value.IsDirty = false;
                        value.Deserialized = true;
                    }
                }
                svc.Add(value);
            }
            return svc;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection) {
            // Validate arguments
            if (!(bool)context["IsAuthenticated"]) throw new NotSupportedException("This provider does not support anonymous profiles");
            string userName = (string)context["UserName"];
            if (string.IsNullOrEmpty(userName) || collection.Count == 0 || !this.HasDirtyProperties(collection)) return; // no work here

            // Construct command
            StringBuilder insertCommandText1 = new StringBuilder("INSERT INTO $Profiles ($UserName, $LastUpdate");
            StringBuilder insertCommandText2 = new StringBuilder(" VALUES (@UserName, GETDATE()");
            StringBuilder updateCommandText = new StringBuilder("UPDATE $Profiles SET $LastUpdate=GETDATE()");
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = userName;

            // Cycle trough collection
            int i = 0;
            foreach (SettingsPropertyValue propVal in collection) {
                PropertyMapInfo pmi = GetPropertyMapInfo(propVal.Property);

                // Always add parameter
                SqlParameter p = new SqlParameter("@Param" + i, pmi.Type);
                if (pmi.Length != 0) p.Size = pmi.Length;
                if (propVal.Deserialized && propVal.PropertyValue == null) p.Value = DBNull.Value;
                else p.Value = propVal.PropertyValue;
                cmd.Parameters.Add(p);

                // Always add to insert
                insertCommandText1.Append(", " + pmi.ColumnName);
                insertCommandText2.Append(", @Param" + i);

                // Add dirty properties to update
                if (propVal.IsDirty) updateCommandText.Append(", " + pmi.ColumnName + "=@Param" + i);

                i++;
            }

            // Complete command
            insertCommandText1.Append(")");
            insertCommandText2.Append(")");
            updateCommandText.Append(" WHERE $UserName=@UserName");
            cmd.CommandText = this.ExpandCommand("IF EXISTS (SELECT * FROM $Profiles WHERE $UserName=@UserName) BEGIN " + updateCommandText.ToString() + " END ELSE BEGIN " + insertCommandText1.ToString() + insertCommandText2.ToString() + " END");

            // Execute command
            cmd.Connection = this.OpenDatabase();
            try {
                cmd.ExecuteNonQuery();
            }
            finally {
                cmd.Connection.Close();
            }
        }

        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) {
            // Validate arguments
            if (pageIndex < 0) throw new ArgumentOutOfRangeException("pageIndex");
            if (pageSize < 1) throw new ArgumentOutOfRangeException("pageSize");
            if (authenticationOption == ProfileAuthenticationOption.Anonymous) {
                // Anonymous profiles not supported
                totalRecords = 0;
                return new ProfileInfoCollection();
            }

            // Prepare sql command
            SqlConnection db = this.OpenDatabase();
            DataTable dt = new DataTable();
            try {
                SqlDataAdapter da;
                if (string.IsNullOrEmpty(usernameToMatch)) {
                    da = new SqlDataAdapter(this.ExpandCommand("SELECT $UserName AS UserName, $LastUpdate AS LastUpdate FROM $Profiles ORDER BY $UserName"), db);
                }
                else {
                    SqlCommand cmd = new SqlCommand(this.ExpandCommand("SELECT $UserName AS UserName, $LastUpdate AS LastUpdate FROM $Profiles WHERE $UserName LIKE @UserName ORDER BY $UserName"), db);
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = usernameToMatch;
                    da = new SqlDataAdapter(cmd);
                }
                da.Fill(dt);
                da.Dispose();
            }
            finally {
                db.Close();
            }

            // Prepare paging
            ProfileInfoCollection pic = new ProfileInfoCollection();
            totalRecords = dt.Rows.Count;
            int minIndex = pageIndex * pageSize; if (minIndex > totalRecords - 1) return pic;
            int maxIndex = minIndex + pageSize - 1; if (maxIndex > totalRecords - 1) maxIndex = totalRecords - 1;

            // Populate collection from data table
            for (int i = minIndex; i <= maxIndex; i++) pic.Add(new ProfileInfo(Convert.ToString(dt.Rows[i]["UserName"]), false, DateTime.Now, Convert.ToDateTime(dt.Rows[i]["LastUpdate"]), 0));
            return pic;
        }

        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords) {
            return FindProfilesByUserName(authenticationOption, string.Empty, pageIndex, pageSize, out totalRecords);
        }

        // Private support functions

        private struct PropertyMapInfo {
            public string ColumnName;
            public SqlDbType Type;
            public int Length;
        }

        private PropertyMapInfo GetPropertyMapInfo(SettingsProperty prop) {
            // Perform general validation
            if (prop == null) throw new ArgumentNullException();
            string cpd = Convert.ToString(prop.Attributes["CustomProviderData"]);
            if (string.IsNullOrEmpty(cpd)) throw new ProviderException(string.Format("CustomProviderData is missing or empty for property {0}.", prop.Name));
            if (!System.Text.RegularExpressions.Regex.IsMatch(cpd, CustomProviderDataFormat)) throw new ProviderException(string.Format("Invalid format of CustomProviderData for property {0}.", prop.Name));
            string[] parts = cpd.Split(';');

            PropertyMapInfo pmi = new PropertyMapInfo();
            pmi.ColumnName = parts[0];
            try {
                pmi.Type = (SqlDbType)Enum.Parse(typeof(SqlDbType), parts[1], true);
            }
            catch {
                throw new ProviderException(string.Format("SqlDbType '{0}' specified for property {1} is invalid.", parts[1], prop.Name));
            }
            if (parts.Length == 3) pmi.Length = Convert.ToInt32(parts[2]);
            return pmi;
        }

        private bool HasDirtyProperties(SettingsPropertyValueCollection props) {
            foreach (SettingsPropertyValue prop in props) {
                if (prop.IsDirty) return true;
            }
            return false;
        }

        private string ExpandCommand(string sql) {
            sql = sql.Replace("$Profiles", this.TableName);
            sql = sql.Replace("$UserName", this.KeyColumnName);
            sql = sql.Replace("$LastUpdate", this.LastUpdateColumnName);
            return sql;
        }

        private SqlConnection OpenDatabase() {
            SqlConnection DB = new SqlConnection(this.connectionString);
            DB.Open();
            return DB;
        }

        private string GetConfig(string name, string defaultValue) {
            // Validate input arguments
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("Name");

            // Get value from configuration
            string Value = this.configuration[name];
            if (string.IsNullOrEmpty(Value)) Value = defaultValue;
            return Value;
        }

        private static bool IsValidDbObjectName(string s) {
            if (string.IsNullOrEmpty(s)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(s, DbObjectNameFormat);
        }

        #region Inactive profiles - not implemented

        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate) {
            throw new NotImplementedException();
        }

        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords) {
            throw new NotImplementedException();
        }

        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords) {
            throw new NotImplementedException();
        }

        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate) {
            throw new NotImplementedException();
        }

        #endregion

    }
}
