using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectMew.Auth;
using PokemonGoDesktop.API.Proto;

namespace ProjectMew
{
    public delegate void CommandDelegate(CommandArgs args);

    public class CommandArgs : EventArgs
    {
        public string Message { get; private set; }
        public PSPlayer Player { get; private set; }

        /// <summary>
        /// Parameters passed to the arguement. Does not include the command name.
        /// IE 'kick "jerk face"' will only have 1 argument
        /// </summary>
        public List<string> Parameters { get; private set; }

        public CommandArgs(string message, PSPlayer ply, List<string> args)
        {
            Message = message;
            Player = ply;
            Parameters = args;
        }
    }

    public class Command
    {
        /// <summary>
        /// Gets or sets the help text of this command.
        /// </summary>
        public string HelpText { get; set; }
        /// <summary>
        /// Gets or sets an extended description of this command.
        /// </summary>
        public string[] HelpDesc { get; set; }
        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        public string Name { get { return Names[0]; } }
        /// <summary>
        /// Gets the names of the command.
        /// </summary>
        public List<string> Names { get; protected set; }

        public bool RequireLogin { get; set; }

        private CommandDelegate commandDelegate;
        public CommandDelegate CommandDelegate
        {
            get { return commandDelegate; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                commandDelegate = value;
            }
        }

        public Command(List<string> permissions, CommandDelegate cmd, params string[] names)
            : this(cmd, names)
        {
        }

        public Command(CommandDelegate cmd, params string[] names)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");
            if (names == null || names.Length < 1)
                throw new ArgumentException("names");

            CommandDelegate = cmd;
            HelpText = "No help available.";
            HelpDesc = null;
            Names = new List<string>(names);
        }

        public bool Run(string msg, PSPlayer ply, List<string> parms)
        {
            if (RequireLogin && !ply.isLoggedin)
            {
                ProjectMew.Log.ConsoleInfo("Cannot run this command unless you are logged in.");
                return false;
            }

            try
            {
                CommandDelegate(new CommandArgs(msg, ply, parms));
            }
            catch (Exception e)
            {
                ProjectMew.Log.ConsoleError("Command failed, check logs for more details.");
                ProjectMew.Log.Error(e.ToString());
            }

            return true;
        }

        public bool HasAlias(string name)
        {
            return Names.Contains(name);
        }
    }

    public static class Commands
    {
        public static List<Command> DefinedCommands = new List<Command>();

        private delegate void AddChatCommand(string permission, CommandDelegate command, params string[] names);

        public static void InitCommands()
        {
            Action<Command> add = (cmd) =>
            {
                DefinedCommands.Add(cmd);
            };

            add(new Command(Login, "login")
            {
                HelpText = "Used to authenticate A Google/Ptc Player Account, username and password only required for Ptc logins. - Syntax: login <google/ptc> [username] [password]"
            });

            add(new Command(Find, "find")
            {
                HelpText = "Returns Geo Locations of Pokemon/Pokestops, default distance 200 meters - Syntax: find <pokemon/pokestop> [distance]",
                RequireLogin = true
            });

            add(new Command(Evolve, "evolve")
            {
                HelpText = "Evolves selected pokemon. Syntax: evolve <pokemon/all> [amount]",
                RequireLogin = true
            });

            add(new Command(Help, "help")
            {
                HelpText = "Returns list of run-able commands - help [command] for more information on a command."
            });
        }

        public static bool HandleCommand(PSPlayer player, string cmdText)
        {
            var args = ParseParameters(cmdText);
            if (args.Count < 1)
                return false;

            string cmdName = args[0].ToLower();
            args.RemoveAt(0);

            IEnumerable<Command> cmds = DefinedCommands.FindAll(c => c.HasAlias(cmdName));

            if (Hooks.UserHooks.OnUserCommand(player, cmdName, cmdText, args, ref cmds))
                return true;

            foreach (Command cmd in cmds)
            {
                cmd.Run(cmdText, player, args);
            }
            return true;
        }

        /// <summary>
        /// Parses a string of parameters into a list. Handles quotes.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static List<String> ParseParameters(string str)
        {
            var ret = new List<string>();
            var sb = new StringBuilder();
            bool instr = false;
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];

                if (c == '\\' && ++i < str.Length)
                {
                    if (str[i] != '"' && str[i] != ' ' && str[i] != '\\')
                        sb.Append('\\');
                    sb.Append(str[i]);
                }
                else if (c == '"')
                {
                    instr = !instr;
                    if (!instr)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (sb.Length > 0)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else if (IsWhiteSpace(c) && !instr)
                {
                    if (sb.Length > 0)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else
                    sb.Append(c);
            }
            if (sb.Length > 0)
                ret.Add(sb.ToString());

            return ret;
        }

        private static bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n';
        }

        private static void Help(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                ProjectMew.Log.ConsoleError("Invalid syntax! Proper syntax: help <command/page>");
                return;
            }

            int pageNumber;
            if (args.Parameters.Count == 0 || int.TryParse(args.Parameters[0], out pageNumber))
            {
                if (!PaginationTools.TryParsePageNumber(args.Parameters, 0, out pageNumber))
                {
                    return;
                }

                IEnumerable<string> cmdNames = from cmd in DefinedCommands
                                               select cmd.Name;

                PaginationTools.SendPage(pageNumber, PaginationTools.BuildLinesFromTerms(cmdNames),
                    new PaginationTools.Settings
                    {
                        HeaderFormat = "Commands ({0}/{1}):",
                        FooterFormat = "Type help {{0}} for more."
                    });
            }
            else
            {
                string commandName = args.Parameters[0].ToLower();

                Command command = DefinedCommands.Find(c => c.Names.Contains(commandName));
                if (command == null)
                {
                    ProjectMew.Log.ConsoleError("Invalid command.");
                    return;
                }

                ProjectMew.Log.ConsoleError("{0} help: ", command.Name);
                if (command.HelpDesc == null)
                {
                    ProjectMew.Log.ConsoleInfo(command.HelpText);
                    return;
                }
                foreach (string line in command.HelpDesc)
                {
                    ProjectMew.Log.ConsoleInfo(line);
                }
            }
        }

        private static void Evolve(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                ProjectMew.Log.ConsoleInfo("Invalid syntax! Proper syntax: evolve <pokemon/all> [amount]");
                return;
            }
            
            if (args.Parameters[0].ToLower() != "all")
            {
                PokemonId id;
                int amount = -1;

                if (!Enum.TryParse(args.Parameters[0], out id))
                {
                    ProjectMew.Log.ConsoleError("Error - {0} is not a valid pokemon!", args.Parameters[0]);
                    return;
                }

                if (args.Parameters.Count == 2 && int.TryParse(args.Parameters[1], out amount))
                {
                    ProjectMew.Log.ConsoleError("Error - {0} is not a valid Number!", args.Parameters[1]);
                    return;
                }

                ProjectMew.Player.Evolve(new PokemonId[] { id }, amount);
            }
            else
            {
                ProjectMew.Player.Evolve((PokemonId[])Enum.GetValues(typeof(PokemonId)), -1);
            }
        }

        private static void GetVersion(CommandArgs args)
        {
            ProjectMew.Log.ConsoleInfo("ProjectMew: {0} ({1}).", ProjectMew.VersionNum, ProjectMew.VersionCodename);
        }

        private static void Login(CommandArgs args)
        {
            if (args.Parameters.Count == 0 || args.Parameters.Count > 3)
            {
                ProjectMew.Log.ConsoleError("Invalid syntax! Proper syntax: login <google/ptc> [username] [password]");
                return;
            }
            
            switch (args.Parameters[0].ToLower())
            {
                case "google":
                    GoogleAuth();
                    return;
                case "ptc":
                    if (args.Parameters.Count != 3)
                    {
                        ProjectMew.Log.ConsoleError("Invalid syntax! Proper syntax: login ptc [username] [password]");
                        return;
                    }
                    PtcAuth(args.Parameters[1], args.Parameters[2]);
                    return;
                default:
                    return;
            }
        }

        private static async void GoogleAuth()
        {
            ProjectMew.Log.ConsoleInfo("Obtaining Device Code...");
            await ProjectMew.Player.DoGoogleLogin();
        }

        private static async void PtcAuth(string Username, string Password)
        {
            ProjectMew.Log.ConsoleInfo("Attempting to Login Via PTC...");
            await ProjectMew.Player.DoPtcLogin(Username, Password);
        }

        private static void Find(CommandArgs args)
        {
            throw new NotImplementedException();
        }

    }
}
