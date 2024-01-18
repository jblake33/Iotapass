using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Dapper;
using Iotapass.Models;

namespace Iotapass.Services
{
    public class SqliteDataAccess : IDisposable

    {
        // Track whether Dispose has been called.
        // START Implementation of IDisposable, to be able to write using (SqliteDataAccess d = new SqliteDataAccess) {...}
        // See https://learn.microsoft.com/en-us/dotnet/api/system.idisposable?view=net-7.0 for more info
        private bool disposed = false;
        public void Dispose(){ Dispose(disposing: true); GC.SuppressFinalize(this); }
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed and unmanaged resources. (nothing to put here)
                // Then, note disposing has been done.
                disposed = true;
            }
        }
        ~SqliteDataAccess() { Dispose(disposing: false); }
        // END Implementation of IDisposable

        // Credentials
        /// <summary>
        /// Returns a list of the saved credentials saved by the profile with this email.
        /// </summary>
        /// <returns>List<CredentialModel></returns>
        internal static List<CredentialModel> ReadCredentials(string email)
        {
            //Opens connection to the DB. The using {} ensures that the connection closes when body finishes (or a crash happens!)
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()) )
            {
                //Our SQL stmt currently is just select * from Credetials
                var output = cnn.Query<CredentialModel>("select * from Credentials where email = '" + email.Replace('\'', '`') + "'", new DynamicParameters());
                // Convert the output to List<CredentailModel>
                List<CredentialModel> tempList = output.ToList();
                // Decrypt each password in each credential
                foreach (CredentialModel credential in tempList)
                {
                    credential.passwd = IotaCryptService.D(credential.passwd, email);
                }
                // Return the list of credentials.
                return tempList;
            }
        }
        /// <summary>
        /// Creates a new credential using the attributes in the acct parameter. Id does not need to be specified.
        /// </summary>
        internal static void CreateCredential(CredentialModel acct)
        {
            acct.passwd = IotaCryptService.E(acct.passwd, acct.email);
            //Get connected to the DB.
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                cnn.Execute(@"insert into Credentials(email, username, passwd, website, isBreached, notes) values(@email, @username, @passwd, @website, @isBreached, @notes)", acct);
            }
        }
        /// <summary>
        /// Updates a credential with the Id matching the value for the Id attribute in acct.
        /// </summary>
        internal static void UpdateCredential(CredentialModel acct, bool UpdateEmail)
        {
            acct.passwd = IotaCryptService.E(acct.passwd, acct.email);
            // If the email does not need to be updated, update credential like normal
            if (!UpdateEmail)
            {
                //Get connected to the DB.
                using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
                {
                    cnn.Execute("update Credentials set username = @username, passwd = @passwd, website = @website, notes = @notes, isBreached = @isBreached where Id = @Id", acct);
                }
            }
            // Otherwise (the email must be updated),
            else
            {
                //Get connected to the DB.
                using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
                {
                    cnn.Execute("update Credentials set email = @email, username = @username, passwd = @passwd, website = @website, notes = @notes, isBreached = @isBreached where Id = @Id", acct);
                }
            }
        }
        /// <summary>
        /// Deletes a credential with the Id matching the value for the Id attribute in acct.
        /// </summary>
        internal static void DeleteCredential(CredentialModel acct)
        {
            //Get connected to the DB.
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                cnn.Execute("delete from Credentials where Id = @Id", acct);
            }
        }
        /// <summary>
        /// Deletes ALL credentials where the email matches the value for email in profile.
        /// </summary>
        internal static void DeleteAllCredentials(ProfileModel profile)
        {
            //Get connected to the DB.
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                cnn.Execute("delete from Credentials where email = @email", profile);
            }
        }
        /// <summary>
        /// Re-encrypt credentials when user changes email. Needs old email and new email as parameters.
        /// </summary>
        /// <param name="oldEmail"></param>
        /// <param name="newEmail"></param>
        internal static void ReEncryptCredentials(string oldEmail, string newEmail)
        {
            if (oldEmail == null || newEmail == null) return;
            if (oldEmail == newEmail) return;
            List<CredentialModel> tmpList = ReadCredentials(oldEmail);
            foreach (CredentialModel tmp in tmpList)
            {
                tmp.email = newEmail;
                UpdateCredential(tmp, true);
            }
        }

        // Profiles
        /// <summary>
        /// Returns a list of the saved profiles.
        /// </summary>
        internal static List<ProfileModel> ReadProfiles()
        {
            //Opens connection to the DB. The using {} ensures that the connection closes when body finishes (or a crash happens!)
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                //Our SQL stmt currently is just select * from Credetials
                var output = cnn.Query<ProfileModel>("select * from Profiles", new DynamicParameters());
                // Convert the output to List<CredentailModel> and return it.
                return output.ToList();
            }
        }
        /// <summary>
        /// Creates a new profile using the attributes of profile. An empty hint is stored as "".
        /// </summary>
        /// <param name="profile"></param>
        internal static void CreateProfile(ProfileModel profile)
        {
            if (String.IsNullOrEmpty(profile.hint))
            {
                profile.hint = "No hint provided.";
            }
            if (String.IsNullOrEmpty(profile.email) || String.IsNullOrEmpty(profile.passwd))
            {
                return;
            }
            // Hash the master password
            profile.passwd = IotaCryptService.GetHash(profile.passwd);
            //Get connected to the DB.
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                cnn.Execute("insert into Profiles values (@email, @passwd, @hint)", profile);
            }
        }
        /// <summary>
        /// Updates the profile that has the oldEmail with the attributes in profile. To avoid updating password, set it to "" or null
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="oldEmail"></param>
        internal static void UpdateProfile(ProfileModel profile, string oldEmail)
        {
            //Get connected to the DB.
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                // If password will not be updated
                if (String.IsNullOrEmpty(profile.passwd))
                {
                    cnn.Execute("update Profiles set email = '" + profile.email.Replace('\'', '`') + "', hint = '" + profile.hint.Replace('\'', '`') + "' where email = '" + oldEmail.Replace('\'', '`') + "'");
                }
                // Otherwise (password will be updated)
                else
                {
                    profile.passwd = IotaCryptService.GetHash(profile.passwd.Replace('\'', '`'));
                    cnn.Execute("update Profiles set email = '" + profile.email.Replace('\'', '`') + "', passwd = '" + profile.passwd + "', hint = '" + profile.hint.Replace('\'', '`') + "' where email = '" + oldEmail.Replace('\'', '`') + "'");
                }
            }
        }
        /// <summary>
        /// Deletes the profile with the email matching the email attribute in profile.
        /// </summary>
        /// <param name="profile"></param>
        internal static void DeleteProfile(ProfileModel profile)
        {
            //Get connected to the DB.
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                cnn.Execute("delete from Profiles where email = @email", profile);
            }
        }
        /// <summary>
        /// Checks if the profile with a matching email exists in the DB. Returns 1 if found, 0 if not.
        /// </summary>
        /// <param name="profile"></param>
        /// 
        internal static int DoesProfileExist(ProfileModel profile)
        {
            //Get connected to the DB.
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                // this returns a 0 or 1 always, just return the "first" value in the query  
                var output = cnn.Query<int>("select exists (select * from Profiles where email = '" + profile.email.Replace('\'', '`') + "')", new DynamicParameters());
                return output.First();
            }
            return 0;
        }
        /// <summary>
        /// Checks to see if an email matches with a profile that has a hint. The hint is returned. An empty string is returned if the profile is not found.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        internal static string ReadProfileHint(string email)
        {
            //Get connected to the DB.
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<string>("select hint from Profiles where email = '" + email.Replace('\'', '`') + "'", new DynamicParameters());
                try
                {
                    string hint = output.First().Trim();
                    return hint;
                }
                catch (InvalidOperationException e)
                {
                    return "";
                }
            }
            return "";
        }
        /// <summary>
        /// Returns a Profile from DB if the profile passed as a parameter has a matching password and email. Password in parameter is plaintext. If no profile match is found, a profile with all attributes empty is returned.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        internal static ProfileModel ProfileMatch(ProfileModel profile)
        {
            //Opens connection to the DB. The using {} ensures that the connection closes when body finishes (or a crash happens!)
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                //Our SQL stmt currently is just select * from Credetials
                var output = cnn.Query<ProfileModel>("select * from profiles where email = '"+profile.email.Replace('\'', '`') + "' limit 0, 1", new DynamicParameters());
                if (output != null)
                {
                    if (output.Count() != 0)
                    {
                        ProfileModel model =  output.ToList().First();
                        
                        // If the plaintext password in the parameter is a match with the hash obtained from the query,
                        if (IotaCryptService.CompareHash(profile.passwd, model.passwd))
                        {
                            // Fill in the two other fields for the profile and return it
                            profile.passwd = model.passwd.Replace('\'', '`');
                            profile.hint = model.hint.Replace('\'', '`');
                            return profile;
                        }
                    }
                }
                // If our output from the query was null, empty, or the hashed passwords didn't match, return a empty profile:
                profile.email = "";
                profile.passwd = "";
                profile.hint = "";
                return profile;
            }
        }
        

        // Preferences
        /// <summary>
        /// Returns a list of the saved preferences.
        /// </summary>
        /// <returns>List<PreferencesModel></returns>
        internal static List<PreferencesModel> ReadAllPreferences()
        {
            //Opens connection to the DB. The using {} ensures that the connection closes when body finishes (or a crash happens!)
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                //Our SQL stmt currently is just select * from Credetials
                var output = cnn.Query<PreferencesModel>("select * from Preferences", new DynamicParameters());
                // Convert the output to List<PreferencesModel> and return it.
                return output.ToList();
            }
        }
        /// <summary>
        /// Returns a list of a user's saved preferences.
        /// </summary>
        /// <returns>List<PreferencesModel></returns>
        internal static List<PreferencesModel> ReadPreferences(string email)
        {
            //Opens connection to the DB. The using {} ensures that the connection closes when body finishes (or a crash happens!)
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                //Our SQL stmt currently is just select * from Credetials
                var output = cnn.Query<PreferencesModel>("select * from Preferences where email = '" + email.Replace('\'', '`') + "'", new DynamicParameters());
                // Convert the output to List<PreferencesModel> and return it.
                return output.ToList();
            }
        }
        /// <summary>
        /// Creates a new preferences class using the attributes of pref.
        /// </summary>
        internal static void CreatePreferences(PreferencesModel pref)
        {
            //Get connected to the DB.
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                cnn.Execute("insert into Preferences values (@email, @resolution, @fontsize, @theme)", pref);
            }
        }
        /// <summary>
        /// Updates the preferences for the email in pref with the new attributes in pref.
        /// </summary>
        /// <param name="pref"></param>
        internal static void UpdatePrefernces(PreferencesModel pref)
        {
            //Get connected to the DB.
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                cnn.Execute("update Preferences set resolution = @resolution, fontsize = @fontsize, theme = @theme where email = @email", pref);
            }
        }
        /// <summary>
        /// Updates the preferences of the user, by changing the old email to the new email.
        /// </summary>
        /// <param name="oldEmail"></param>
        /// <param name="newEmail"></param>
        internal static void UpdatePreferencesEmail(string oldEmail, string newEmail)
        {
            if (oldEmail == null || newEmail == null) return;
            //Get connected to the DB.
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                cnn.Execute("update Preferences set email = '"+ newEmail.Replace('\'', '`') + "' where email = '" + oldEmail.Replace('\'', '`') + "'");
            }
        }
        /// <summary>
        /// Deletes the preferences where the email matches the email attribute in pref.
        /// </summary>
        /// <param name="pref"></param>
        internal static void DeletePrefernces(string email)
        {
            //Get connected to the DB.
            using (IDbConnection cnn = new SqliteConnection(LoadConnectionString()))
            {
                cnn.Execute("delete from Preferences where email = '" + email.Replace('\'', '`') + "'");
            }
        }
        
        // Load connection string
        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
