
using System;
using System.Web.Security;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Web;

namespace PAB.Web.Providers {

    public sealed class SimpleSqlRoleProvider : RoleProvider {

        #region Initialization and configuration

        private string applicationName, connectionString;
      //  private NameValueCollection config;

        public override void Initialize(string name, NameValueCollection config) {
            // Validate arguments
            if (config == null) throw new ArgumentNullException("config");
            //this.config = config;
            if (string.IsNullOrEmpty(name)) name = "SimpleSqlRoleProvider";
            if (String.IsNullOrEmpty(config["description"])) {
                config.Remove("description");
                config.Add("description", "PAB simple SQL role provider");
            }

            // Initialize base class
            base.Initialize(name, config);

            // Initialize current class
            System.Configuration.ConnectionStringSettings ConnectionStringSettings = System.Configuration.ConfigurationManager.ConnectionStrings[config["connectionStringName"]];
            if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "") throw new ProviderException("Connection string cannot be blank.");
            this.connectionString = ConnectionStringSettings.ConnectionString;
            this.applicationName = config["applicationName"];
        }

        public override string ApplicationName {
            get { return applicationName; }
            set { applicationName = value; }
        }

        #endregion

        

        public override void CreateRole(string rolename) {
            // Validate arguments
            if (string.IsNullOrEmpty(rolename)) throw new ArgumentNullException("rolename");
            if (rolename.IndexOf(',') > 0) throw new ArgumentException("Role names cannot contain commas");
            if (rolename.Length > 100) throw new ArgumentException("Maximum role name length is 100 characters");
            if (this.RoleExists(rolename)) throw new ProviderException("Role name already exists");
            rolename = rolename.ToLower();

            // Create role
            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("INSERT INTO Roles (RoleName) VALUES (@RoleName)", db);
                cmd.Parameters.Add("@Rolename", SqlDbType.VarChar, 100).Value = rolename;
                cmd.ExecuteNonQuery();
            }
            finally { db.Close(); }
        }

        public override bool DeleteRole(string rolename, bool throwOnPopulatedRole) {
            // Validate arguments
            if (string.IsNullOrEmpty(rolename)) throw new ArgumentNullException("rolename");
            if (!this.RoleExists(rolename)) throw new ProviderException("Role does not exist");
            if (throwOnPopulatedRole && this.GetUsersInRole(rolename).Length > 0) throw new ProviderException("Cannot delete a populated role");
            rolename = rolename.ToLower();

            // Delete role
            SqlConnection db = this.OpenDatabase();
            int rowCount = 0;
            try {
                SqlCommand cmd = new SqlCommand("DELETE FROM Roles WHERE RoleName = @RoleName", db);
                cmd.Parameters.Add("@RoleName", SqlDbType.VarChar, 100).Value = rolename;
                rowCount = cmd.ExecuteNonQuery();
            }
            finally { db.Close(); }

            return rowCount > 0;
        }

        public override void AddUsersToRoles(string[] usernames, string[] rolenames) {
            // Validate arguments
            foreach (string rolename in rolenames) if (!this.RoleExists(rolename)) throw new ProviderException("Role name not found");
            foreach (string username in usernames) {
                if (username.IndexOf(',') > 0) throw new ArgumentException("User names cannot contain commas.");
                foreach (string rolename in rolenames) {
                    if (IsUserInRole(username, rolename)) return; // user is already in this role
                }
            }

            SqlConnection db = this.OpenDatabase();
            SqlCommand cmd = new SqlCommand("INSERT INTO UsersInRoles (UserName, RoleName) VALUES (@UserName, @RoleName)", db);
            cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100);
            cmd.Parameters.Add("@RoleName", SqlDbType.VarChar, 100);
            SqlTransaction tran = null;

            try {
                tran = db.BeginTransaction();
                cmd.Transaction = tran;
                foreach (string username in usernames) {
                    foreach (string rolename in rolenames) {
                        cmd.Parameters["@UserName"].Value = username;
                        cmd.Parameters["@RoleName"].Value = rolename;
                        cmd.ExecuteNonQuery();
                    }
                }
                tran.Commit();
            }
            catch {
                tran.Rollback();
                throw;
            }
            finally {
                db.Close();
            }
        }

        public override string[] GetAllRoles() {
            // Get data from database
            DataTable roleTable = new DataTable();
            SqlConnection db = this.OpenDatabase();
            try {
                SqlDataAdapter da = new SqlDataAdapter("SELECT RoleName FROM Roles", db);
                da.Fill(roleTable);
            }
            finally {
                db.Close();
            }
            return TableToArray(roleTable);
        }

        public override string[] GetRolesForUser(string username) {
            // Validate arguments
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException("username");
            if (username.IndexOf(',') > -1) throw new ArgumentException("User name cannot contain comma", "username");
            if (username.Length > 100) throw new ArgumentException("User name cannot be longer than 100 characters", "username");
            username = username.ToLower();
            if (HttpContext.Current.Request.Cookies["roles"] != null)
            {
                return  HttpContext.Current.Request.Cookies["roles"].Value.Split(',');
            }
            else
            {

                // Get data from database
                DataTable roleTable = new DataTable();
                SqlConnection db = this.OpenDatabase();
                try
                {
                    SqlCommand cmd = new SqlCommand("SELECT RoleName FROM UsersInRoles WHERE UserName=@UserName", db);
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = username;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(roleTable);
                }
                finally
                {
                    db.Close();
                }
                string[] roles= TableToArray(roleTable);
                
                HttpContext.Current.Response.Cookies["roles"].Value = String.Join(",", roles);
                HttpContext.Current.Response.Cookies["roles"].Expires = DateTime.Now.AddDays(30);
                return roles;
            }
        }

        public override string[] GetUsersInRole(string rolename) {
            // Validate arguments
            if (string.IsNullOrEmpty(rolename)) throw new ArgumentNullException("rolename");
            if (rolename.IndexOf(',') > -1) throw new ArgumentException("Role name cannot contain comma", "rolename");
            if (rolename.Length > 100) throw new ArgumentException("Role name cannot be longer than 100 characters", "rolename");
            rolename = rolename.ToLower();

            // Get data from database
            DataTable roleTable = new DataTable();
            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("SELECT UserName FROM UsersInRoles WHERE RoleName=@RoleName", db);
                cmd.Parameters.Add("@RoleName", SqlDbType.VarChar, 100).Value = rolename;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(roleTable);
            }
            finally {
                db.Close();
            }
            return TableToArray(roleTable);
        }

        public override bool IsUserInRole(string username, string rolename) {
            // Validate arguments
            if (string.IsNullOrEmpty(rolename)) throw new ArgumentNullException("rolename");
            if (rolename.IndexOf(',') > -1) throw new ArgumentException("Role name cannot contain comma", "rolename");
            if (rolename.Length > 100) throw new ArgumentException("Role name cannot be longer than 100 characters", "rolename");
            rolename = rolename.ToLower();
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException("username");
            if (username.IndexOf(',') > -1) throw new ArgumentException("User name cannot contain comma", "username");
            if (username.Length > 100) throw new ArgumentException("User name cannot be longer than 100 characters", "username");
            username = username.ToLower();

            // Get data from database
            int rowCount = 0;
            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM UsersInRoles WHERE RoleName=@RoleName AND UserName=@UserName", db);
                cmd.Parameters.Add("@RoleName", SqlDbType.VarChar, 100).Value = rolename;
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = username;
                rowCount = (int)cmd.ExecuteScalar();
            }
            finally {
                db.Close();
            }
            return rowCount > 0;
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] rolenames) {
            // Validate arguments
            foreach (string rolename in rolenames) if (!RoleExists(rolename)) throw new ProviderException("Role name not found.");
            foreach (string username in usernames) {
                foreach (string rolename in rolenames) {
                    if (!IsUserInRole(username, rolename)) throw new ProviderException("User is not in role.");
                }
            }

            SqlConnection db = this.OpenDatabase();
            SqlCommand cmd = new SqlCommand("DELETE FROM UsersInRoles WHERE UserName=@UserName AND RoleName=@RoleName", db);
            cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100);
            cmd.Parameters.Add("@RoleName", SqlDbType.VarChar, 100);
            SqlTransaction tran = null;

            try {
                tran = db.BeginTransaction();
                cmd.Transaction = tran;

                foreach (string username in usernames) {
                    foreach (string rolename in rolenames) {
                        cmd.Parameters["@UserName"].Value = username;
                        cmd.Parameters["@RoleName"].Value = rolename;
                        cmd.ExecuteNonQuery();
                    }
                }

                tran.Commit();
            }
            catch {
                tran.Rollback();
                throw;
            }
            finally {
                db.Close();
            }
        }

        public override bool RoleExists(string rolename) {
            // Validate arguments
            if (string.IsNullOrEmpty(rolename)) return false;
            rolename = rolename.ToLower();

            // Check if role exists
            SqlConnection db = this.OpenDatabase();
            int rowCount = 0;
            try {
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Roles WHERE RoleName=@RoleName", db);
                cmd.Parameters.Add("@RoleName", SqlDbType.VarChar, 100).Value = rolename;
                rowCount = (int)cmd.ExecuteScalar();
            }
            finally { db.Close(); }

            return rowCount > 0;
        }

        public override string[] FindUsersInRole(string rolename, string usernameToMatch) {
            // Validate arguments
            if (string.IsNullOrEmpty(rolename)) throw new ArgumentNullException("rolename");
            if (rolename.IndexOf(',') > -1) throw new ArgumentException("Role name cannot contain comma", "rolename");
            if (rolename.Length > 100) throw new ArgumentException("Role name cannot be longer than 100 characters", "rolename");
            rolename = rolename.ToLower();
            if (string.IsNullOrEmpty(usernameToMatch)) throw new ArgumentNullException("usernameToMatch");
            if (usernameToMatch.Length > 100) throw new ArgumentException("User name cannot be longer than 100 characters", "usernameToMatch");
            usernameToMatch = usernameToMatch.ToLower();

            // Get data from database
            DataTable roleTable = new DataTable();
            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("SELECT UserName FROM UsersInRoles WHERE RoleName=@RoleName AND UserName LIKE @UserName", db);
                cmd.Parameters.Add("@RoleName", SqlDbType.VarChar, 100).Value = rolename;
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = usernameToMatch;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(roleTable);
            }
            finally {
                db.Close();
            }
            return TableToArray(roleTable);
        }

        // Private support functions

        private SqlConnection OpenDatabase() {
            SqlConnection DB = new SqlConnection(this.connectionString);
            DB.Open();
            return DB;
        }

        private string[] TableToArray(DataTable table) {
            // Validate arguments
            if (table == null) throw new ArgumentNullException("table");
            if (table.Rows.Count == 0) return new string[0];

            // Convert table to array
            string[] data = new string[table.Rows.Count];
            for (int i = 0; i < table.Rows.Count; i++) data[i] = Convert.ToString(table.Rows[i][0]);
            return data;
        }

    }

}