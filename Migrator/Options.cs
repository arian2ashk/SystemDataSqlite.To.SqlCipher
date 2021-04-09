using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Migrator
{
    public class Options
    {
        /// <remarks>
        ///     order of the parameters must be the same as order of properties in this class.
        /// </remarks>
        public Options(string path, string password, string newPassword)
        {
            Path = path;
            Password = password;
            NewPassword = newPassword;
        }

        [Option('d', "databasePath",
            HelpText = "Path to the System.Data.SQLite encrypted database.")]
        public string Path { get; }

        [Option('p', "password",
            HelpText = "Password of the System.Data.SQLite encrypted database.")]
        public string Password { get; }

        [Option('n', "newPassword",
            HelpText = "Optional new password that will be used for the new SqlCipher encrypted database.")]
        public string NewPassword { get; }
    }
}
