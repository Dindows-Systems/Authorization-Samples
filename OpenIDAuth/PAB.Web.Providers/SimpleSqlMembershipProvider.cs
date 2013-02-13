
using System.Configuration.Provider;
using System.Data;
using System.Data.SqlClient;
using System.Web.Security;
using System;

namespace PAB.Web.Providers {

    public sealed class SimpleSqlMembershipProvider : MembershipProvider {
        private const int DefaultMinRequiredPasswordLength = 5;

        #region Initialization and configuration

        private string applicationName, connectionString;
        private System.Collections.Specialized.NameValueCollection configuration;

        private bool pEnablePasswordRetrieval;
        private bool pRequiresQuestionAndAnswer;



        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config) {
            // Validate arguments
            if (config == null) throw new ArgumentNullException("config");
            if (string.IsNullOrEmpty(name)) name = "SimpleSqlMembershipProvider";
            if (String.IsNullOrEmpty(config["description"])) {
                config.Remove("description");
                config.Add("description", "PAB simple SQL membership provider");
            if(config["ConnectionStringName"]==null)
                config.Add("ConnectionStringName", "IttyUrl");
            }

            // Initialize base class
            base.Initialize(name, config);

            // Initialize current class
            this.configuration = config;
          System.Configuration.ConnectionStringSettings ConnectionStringSettings = System.Configuration.ConfigurationManager.ConnectionStrings[config["connectionStringName"]];
           if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "") throw new ProviderException("Connection string cannot be blank.");
          this.connectionString = ConnectionStringSettings.ConnectionString;
           // this.connectionString = config[0];
            this.applicationName = GetConfig("applicationName", "");
        }

        public override string ApplicationName {
            get { return this.applicationName; }
            set { this.applicationName = value; }
        }



      
        
        public override bool EnablePasswordReset {
            get { return true; }
        }

        public override bool EnablePasswordRetrieval {
            get { return true; /* return Convert.ToBoolean(this.GetConfig("EnablePasswordRetrieval", "true"));*/ }
        }

        public override int MaxInvalidPasswordAttempts {
            get { return 0; }
        }

        public override int MinRequiredNonAlphanumericCharacters {
            get { return Convert.ToInt32(this.GetConfig("minRequiredNonAlphanumericCharacters", "0")); }
        }

        public override int MinRequiredPasswordLength {
            get { return Convert.ToInt32(this.GetConfig("minRequiredPasswordLength", DefaultMinRequiredPasswordLength.ToString())); }
        }

        public override int PasswordAttemptWindow {
            get { return 0; }
        }

        public override bool RequiresQuestionAndAnswer {
            get { return false; /* return Convert.ToBoolean(this.GetConfig("RequiresQuestionAndAnswer", "true"));*/ }
        }

        public override bool RequiresUniqueEmail {
            get { return Convert.ToBoolean(this.GetConfig("requiresUniqueEmail", "true")); }
        }

        public override string PasswordStrengthRegularExpression {
            get { return this.GetConfig("passwordStrengthRegularExpression", ""); }
        }

        public override MembershipPasswordFormat PasswordFormat {
            get { return MembershipPasswordFormat.Clear; }
        }

        #endregion

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status) {
            // Check username
            if (string.IsNullOrEmpty(username) || username.Length > 100) {
                status = MembershipCreateStatus.InvalidUserName;
                return null;
            }
           // username = username.ToLower();
            if (this.CheckUserExists(username)) {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }

            // Check if password meets complexity requirements
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(args);
            if (args.Cancel) {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            // Check e-mail
            if (!IsEmail(email)) {
                status = MembershipCreateStatus.InvalidEmail;
                return null;
            }
            email = email.ToLower();
            if (this.RequiresUniqueEmail && this.GetUserNameByEmail(email) != string.Empty) {
                status = MembershipCreateStatus.DuplicateEmail;
            }

            // Generate hash from password -- don't need any of this with OpenId
           // string passwordSalt = Membership.GeneratePassword(5, 1);
            //string passwordHash = ComputeSHA512(password + passwordSalt);
          //  string passwordHash = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "SHA1");


            // Insert to database
            SqlConnection db = this.OpenDatabase();
            try {
 SqlCommand cmd = new SqlCommand("INSERT INTO Users (UserName, Password,  Email, Comment, Enabled, DateCreated, DateLastLogin, DateLastActivity, DateLastPasswordChange) VALUES (@UserName, @Password,  @Email, NULL, @Enabled, GETDATE(), NULL, NULL, GETDATE())", db);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = username;
                cmd.Parameters.Add("@Password", SqlDbType.Char, 86).Value = password;
               // cmd.Parameters.Add("@PasswordSalt", SqlDbType.Char, 5).Value = passwordSalt;
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value = email;
                cmd.Parameters.Add("@Enabled", SqlDbType.Bit).Value = isApproved;
                // we don't need next two for OpenId--
               // cmd.Parameters.Add("@PasswordQuestion", SqlDbType.VarChar, 500).Value = passwordQuestion;
              //  cmd.Parameters.Add("@PasswordAnswer", SqlDbType.VarChar, 250).Value = passwordAnswer;
                int rowCount = cmd.ExecuteNonQuery();
                if (rowCount == 0) {
                    status = MembershipCreateStatus.UserRejected;
                }
                else {
                    status = MembershipCreateStatus.Success;
                }
            }
            finally { db.Close(); }

            if (status == MembershipCreateStatus.Success) return this.GetUser(username, false);
            return null;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline) {
            // Validate arguments
            if (string.IsNullOrEmpty(username) || username.Length > 100) return null;
            username = username.ToLower();

            // Read user information
            MembershipUser u = null;
            SqlDataReader r = null;
            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("SELECT UserId, UserName,Password, Email, Comment, Enabled, DateCreated, DateLastLogin, DateLastActivity, DateLastPasswordChange FROM Users WHERE UserName=@UserName", db);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = username;
                r = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (r.Read()) u = this.GetUserFromReader(r);
            }
            finally {
                if (r != null) r.Close();
                db.Close();
            }

            // Update last activity date
            if (userIsOnline) this.UpdateLastActivityDate(u.UserName);

            return u;
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline) {
            // Get userId
            if (providerUserKey == null) return null;
            int userId = Convert.ToInt32(providerUserKey);

            // Read user information
            MembershipUser u = null;
            SqlDataReader r = null;
            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("SELECT UserId, UserName, password,Email, Comment, Enabled, DateCreated, DateLastLogin, DateLastActivity, DateLastPasswordChange FROM Users WHERE UserId=@UserId", db);
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
                r = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (r.Read()) u = this.GetUserFromReader(r);
            }
            finally {
                if (r != null) r.Close();
                db.Close();
            }

            // Update last activity date
            if (userIsOnline) this.UpdateLastActivityDate(u.UserName);

            return u;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer) {
             bool retval = false;

            int ret = 0;
            string pwd = "";
            SqlConnection db = this.OpenDatabase();
            try
            {
                SqlCommand cmd = new SqlCommand("UPDATE Users SET PasswordQuestion=@NewPasswordQuestion, PasswordAnswer =@NewPasswordAnswer where UserName=@UserName and Password=@Password", db);
                cmd.Parameters.Add("@NewPasswordQuestion", SqlDbType.VarChar, 500).Value = newPasswordQuestion;
                cmd.Parameters.Add("@NewPasswordAnswer", SqlDbType.VarChar, 250).Value = newPasswordAnswer;
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = username;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar, 50).Value = password;


                ret = cmd.ExecuteNonQuery();
                if (ret > 0) retval = true;
            }
            finally
            {
                
                db.Close();
            }

            return retval;
        }

        public override string GetPassword(string username, string answer) {
             SqlDataReader r = null;
            string pwd="";
            SqlCommand cmd = null;
            SqlConnection db = this.OpenDatabase();
            try
            {
                if (answer != null)
                {
                    cmd = new SqlCommand("SELECT  password  FROM Users WHERE UserName=@UserName and PasswordAnswer =@PasswordAnswer", db);
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = username;
                    cmd.Parameters.Add("@PasswordAnswer", SqlDbType.VarChar, 250).Value = answer;
                }
                else
                {
                    cmd = new SqlCommand("SELECT  password  FROM Users WHERE UserName=@UserName", db);
                    cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = username;
                    

                }
                 r= cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (r.Read()) pwd = r.GetString(0);
            }
            finally
            {
                if (r != null) r.Close();
                db.Close();
            }
             
            return  pwd;

        }







        public override string GetUserNameByEmail(string email) {
            if (string.IsNullOrEmpty(email) || email.Length > 100) return string.Empty;

            string UserName = string.Empty;
            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("SELECT UserName FROM Users WHERE Email=@Email", db);
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value = email.ToLower();
                UserName = (string)cmd.ExecuteScalar();
                if (UserName == null) UserName = string.Empty;
            }
            finally {
                db.Close();
            }
            return UserName;
        }

        public override bool ValidateUser(string username, string password) {
            // Validate arguments
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || username.Length > 100) return false;
           // username = username.ToLower();

            // Get password hash and salt for username
            string pwd = string.Empty;
            string passwordSalt = string.Empty;
            SqlDataReader r = null;
            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("SELECT Password FROM Users WHERE UserName=@UserName AND Enabled=1", db);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = username;
                r = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (r.Read()) {
                    pwd = Convert.ToString(r["Password"]).Trim();
                    //passwordSalt = Convert.ToString(r["PasswordSalt"]);
                }
            }
            finally {
                if (r != null) r.Close();
                db.Close();
            }

            // Check if user exists
            if (string.IsNullOrEmpty(pwd) ) { return false; }

            // Validate password
           // string hash = FormsAuthentication.HashPasswordForStoringInConfigFile(password ,"SHA1");
            if (pwd.Equals(password.Trim())) {
                // Password is valid
                this.UpdateLastLoginDate(username);
                
                return true;
            }
            else {
                return false;
            }
        }

         

        public override bool ChangePassword(string username, string oldPassword, string newPassword) {
            // Validate user
            if (!ValidateUser(username, oldPassword)) return false;
            username = username.ToLower();

            // Check if password meets complexivity requirements
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, true);
            OnValidatingPassword(args);
            if (args.Cancel) {
                if (args.FailureInformation != null) {
                    throw args.FailureInformation;
                }
                else {
                    throw new MembershipPasswordException("Change password canceled due to new password validation failure.");
                }
            }

            // Update password in database
            return this.SetPassword(username, newPassword);
        }

        public override string ResetPassword(string username, string answer) {
            // Check if user exists
            if (!this.CheckUserExists(username)) throw new MembershipPasswordException("User not found");
            username = username.ToLower();

            // Generate new password
            string newPassword = Membership.GeneratePassword(this.MinRequiredPasswordLength, this.MinRequiredNonAlphanumericCharacters);
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPassword, true);
            OnValidatingPassword(args);
            if (args.Cancel) {
                if (args.FailureInformation != null) {
                    throw args.FailureInformation;
                }
                else {
                    throw new MembershipPasswordException("Reset password canceled due to password validation failure.");
                }
            }

            // Reset password
            this.SetPassword(username, newPassword);
            return newPassword;
        }

        public override bool UnlockUser(string username) {
            // Check if user exists
            if (!this.CheckUserExists(username)) return false;
            username = username.ToLower();

            // Update password in database
            int rowCount = 0;
            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("UPDATE Users SET Enabled=1 WHERE UserName=@UserName", db);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = username;
                rowCount = cmd.ExecuteNonQuery();
            }
            finally { db.Close(); }

            return rowCount > 0;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData) {
            // Check if user exists
            if (!this.CheckUserExists(username)) return false;
            username = username.ToLower();

            // Delete user data
            int rowCount = 0;
            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("DELETE FROM Users WHERE UserName=@UserName", db);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = username;
                rowCount = cmd.ExecuteNonQuery();
            }
            finally { db.Close(); }

            return rowCount > 0;
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords) {
            // Validate arguments
            if (emailToMatch == null) emailToMatch = string.Empty;
            if (pageIndex < 0) throw new ArgumentOutOfRangeException("pageIndex");
            if (pageSize < 1) throw new ArgumentOutOfRangeException("pageSize");
            emailToMatch = emailToMatch.ToLower().Trim();

            // Get table with users
            DataTable userTable = new DataTable();
            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("SELECT UserId, UserName, Email, Comment, Enabled, DateCreated, DateLastLogin, DateLastActivity, DateLastPasswordChange FROM Users WHERE Email LIKE @Email", db);
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value = emailToMatch;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(userTable);
            }
            finally {
                db.Close();
            }

            // Get bounds
            totalRecords = userTable.Rows.Count;
            int startIndex = pageIndex * pageSize;
            int endIndex = startIndex + pageSize - 1;

            // Get number of users
            if (totalRecords == 0 || startIndex > totalRecords) return new MembershipUserCollection(); // no users
            return this.GetUsersFromDataTable(userTable, startIndex, endIndex);
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords) {
            // Validate arguments
            if (string.IsNullOrEmpty(usernameToMatch)) throw new ArgumentNullException("usernameToMatch");
            if (pageIndex < 0) throw new ArgumentOutOfRangeException("pageIndex");
            if (pageSize < 1) throw new ArgumentOutOfRangeException("pageSize");
            usernameToMatch = usernameToMatch.ToLower().Trim();

            // Get table with users
            DataTable userTable = new DataTable();
            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("SELECT UserId, UserName, Email, Comment, Enabled, DateCreated, DateLastLogin, DateLastActivity, DateLastPasswordChange FROM Users WHERE UserName LIKE @UserName", db);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = usernameToMatch;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(userTable);
            }
            finally {
                db.Close();
            }

            // Get bounds
            totalRecords = userTable.Rows.Count;
            int startIndex = pageIndex * pageSize;
            int endIndex = startIndex + pageSize - 1;

            // Get number of users
            if (totalRecords == 0 || startIndex > totalRecords) return new MembershipUserCollection(); // no users
            return this.GetUsersFromDataTable(userTable, startIndex, endIndex);
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords) {
            // Validate arguments
            if (pageIndex < 0) throw new ArgumentOutOfRangeException("pageIndex");
            if (pageSize < 1) throw new ArgumentOutOfRangeException("pageSize");

            // Get table with users
            DataTable userTable = new DataTable();
            SqlConnection db = this.OpenDatabase();
            try {
                SqlDataAdapter da = new SqlDataAdapter("SELECT UserId, UserName, Email, Comment, Enabled, DateCreated, DateLastLogin, DateLastActivity, DateLastPasswordChange FROM Users", db);
                da.Fill(userTable);
            }
            finally {
                db.Close();
            }

            // Get bounds
            totalRecords = userTable.Rows.Count;
            int startIndex = pageIndex * pageSize;
            int endIndex = startIndex + pageSize - 1;

            // Get number of users
            if (totalRecords == 0 || startIndex > totalRecords) return new MembershipUserCollection(); // no users
            return this.GetUsersFromDataTable(userTable, startIndex, endIndex);
        }

        public override int GetNumberOfUsersOnline() {
            SqlConnection db = this.OpenDatabase();

            int OnlineCount = 0;
            try {
                SqlCommand cmd = new SqlCommand("SELECT Count(*) FROM Users WHERE LastActivityDate > @LastActivityDate", db);
                cmd.Parameters.Add("@LastActivityDate", SqlDbType.DateTime).Value = DateTime.Now.AddMinutes(-Membership.UserIsOnlineTimeWindow);
                OnlineCount = (int)cmd.ExecuteScalar();
            }
            finally { db.Close(); }

            return OnlineCount;
        }

        public override void UpdateUser(MembershipUser user) {
            // Validate arguments
            if (user == null) throw new ArgumentNullException("user");
            if (!IsEmail(user.Email)) throw new ArgumentException("E-mail is invalid", "user");
            if (!this.CheckUserExists(user.UserName)) throw new ArgumentException("User not found", "user");

            // Update database
            SqlConnection db = this.OpenDatabase();

            try {
                SqlCommand cmd = new SqlCommand("UPDATE Users SET Email=@Email, Comment=@Comment, Enabled=@Enabled WHERE UserName=@UserName", db);
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 100).Value = user.Email.ToLower();
                cmd.Parameters.Add("@Comment", SqlDbType.Text).Value = user.Comment;
                cmd.Parameters.Add("@Enabled", SqlDbType.Bit).Value = user.IsApproved;
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = user.UserName.ToLower();
                cmd.ExecuteNonQuery();
            }
            finally { db.Close(); }
        }

        // Public extension functions

        public bool CheckUserExists(string username) {
            if (string.IsNullOrEmpty(username)) return false;
            if (username.Length > 100) throw new ArgumentOutOfRangeException("username", "Maximum length of 100 characters exceeded");

            bool exists = false;
            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE UserName=@UserName", db);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = username.ToLower();
                exists = (int)cmd.ExecuteScalar() == 1;
            }
            finally { db.Close(); }
            return exists;
        }

        // Private support functions

        private MembershipUserCollection GetUsersFromDataTable(DataTable userTable, int startIndex, int endIndex) {
            // Validate arguments
            if (userTable == null) throw new ArgumentNullException("userTable");
            if (startIndex < 0) throw new ArgumentOutOfRangeException("startIndex");
            if (endIndex < startIndex) throw new ArgumentOutOfRangeException("endIndex");
            if (endIndex > userTable.Rows.Count) endIndex = userTable.Rows.Count;

            // Read selected users
            MembershipUserCollection uc = new MembershipUserCollection();
            for (int i = startIndex; i < endIndex; i++) {
                string username = Convert.ToString(userTable.Rows[i]["UserName"]);
                object providerUserKey = userTable.Rows[i]["UserId"];
                string email = Convert.ToString(userTable.Rows[i]["Email"]);
                string comment = ""; if (userTable.Rows[i]["Comment"] != DBNull.Value) comment = Convert.ToString(userTable.Rows[i]["Comment"]);
                bool isApproved = Convert.ToBoolean(userTable.Rows[i]["Enabled"]);
                bool isLockedOut = !Convert.ToBoolean(userTable.Rows[i]["Enabled"]);
                DateTime creationDate = Convert.ToDateTime(userTable.Rows[i]["DateCreated"]);
                DateTime lastLoginDate = new DateTime(); if (userTable.Rows[i]["DateLastLogin"] != DBNull.Value) lastLoginDate = Convert.ToDateTime(userTable.Rows[i]["DateLastLogin"]);
                DateTime lastActivityDate = new DateTime(); if (userTable.Rows[i]["DateLastActivity"] != DBNull.Value) lastLoginDate = Convert.ToDateTime(userTable.Rows[i]["DateLastActivity"]);
                DateTime lastPasswordChangedDate = Convert.ToDateTime(userTable.Rows[i]["DateLastPasswordChange"]);
                uc.Add(new MembershipUser(this.Name, username, providerUserKey, email, string.Empty, comment, isApproved, isLockedOut, creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, new DateTime()));
            }

            return uc;
        }

        private MembershipUser GetUserFromReader(SqlDataReader reader) {
            string username = Convert.ToString(reader["UserName"]);
            object providerUserKey = reader["UserId"];
            string email = Convert.ToString(reader["Email"]);
            string comment = ""; if (reader["Comment"] != DBNull.Value) comment = Convert.ToString(reader["Comment"]);
            bool isApproved = Convert.ToBoolean(reader["Enabled"]);
            bool isLockedOut = !Convert.ToBoolean(reader["Enabled"]);
            DateTime creationDate = Convert.ToDateTime(reader["DateCreated"]);
            DateTime lastLoginDate = new DateTime(); if (reader["DateLastLogin"] != DBNull.Value) lastLoginDate = Convert.ToDateTime(reader["DateLastLogin"]);
            DateTime lastActivityDate = new DateTime(); if (reader["DateLastActivity"] != DBNull.Value) lastLoginDate = Convert.ToDateTime(reader["DateLastActivity"]);
            DateTime lastPasswordChangedDate = Convert.ToDateTime(reader["DateLastPasswordChange"]);
            return new MembershipUser(this.Name, username, providerUserKey, email, string.Empty, comment, isApproved, isLockedOut, creationDate, lastLoginDate, lastActivityDate, lastPasswordChangedDate, new DateTime());
        }

        private bool SetPassword(string username, string password) {
            // Generate new password hash and salt
           // string passwordSalt = Membership.GeneratePassword(5, 1);
           // string passwordHash = FormsAuthentication.HashPasswordForStoringInConfigFile(password+passwordSalt, "SHA1"); //ComputeSHA512(password + passwordSalt);

            // Update password in database
            int rowCount = 0;
            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("UPDATE Users SET Password=@Password,  DateLastActivity=GETDATE() WHERE UserName=@UserName", db);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = username;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar, 50).Value = password;
               // cmd.Parameters.Add("@PasswordSalt", SqlDbType.Char, 5).Value = passwordSalt;
                rowCount = cmd.ExecuteNonQuery();
            }
            finally { db.Close(); }

            return rowCount > 0;
        }

        private void UpdateLastActivityDate(string username) {
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException("username");
            if (username.Length > 100) throw new ArgumentOutOfRangeException("username", "Maximum length of 100 characters exceeded");

            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("UPDATE Users SET DateLastActivity=GETDATE() WHERE UserName=@UserName", db);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = username.ToLower();
                cmd.ExecuteNonQuery();
            }
            finally { db.Close(); }
        }

        private void UpdateLastLoginDate(string username) {
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException("username");
            if (username.Length > 100) throw new ArgumentOutOfRangeException("username", "Maximum length of 100 characters exceeded");

            SqlConnection db = this.OpenDatabase();
            try {
                SqlCommand cmd = new SqlCommand("UPDATE Users SET DateLastLogin=GETDATE(), DateLastActivity=GETDATE() WHERE UserName=@UserName", db);
                cmd.Parameters.Add("@UserName", SqlDbType.VarChar, 100).Value = username.ToLower();
                cmd.ExecuteNonQuery();
            }
            finally { db.Close(); }
        }

        private static bool IsEmail(string email) {
            if (string.IsNullOrEmpty(email) || email.Length > 100) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(email, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
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

        private static string ComputeSHA512(string s) {
            if (string.IsNullOrEmpty(s)) throw new ArgumentNullException();
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(s);
            buffer = System.Security.Cryptography.SHA512Managed.Create().ComputeHash(buffer);
            return System.Convert.ToBase64String(buffer).Substring(0, 86); // strip padding
        }

    }

}