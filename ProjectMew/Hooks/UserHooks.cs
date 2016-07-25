using System.Collections.Generic;
using PokemonGoDesktop.API.Proto;
using System.ComponentModel;

namespace ProjectMew.Hooks
{
    /// <summary>
    /// EventArgs used for the <see cref="UserHooks.UserPostLogin"/> event.
    /// </summary>
    public class UserPostLoginEventArgs
    {
        /// <summary>
        /// The User who fired the event.
        /// </summary>
        public PSPlayer User { get; set; }

        /// <summary>
        /// Initializes a new instance of the UserPostLoginEventArgs class.
        /// </summary>
        /// <param name="user">The User who fired the event.</param>
        public UserPostLoginEventArgs(PSPlayer user)
        {
            User = user;
        }
    }

    /// <summary>
    /// EventArgs used for the <see cref="UserHooks.UserPreLogin"/> event.
    /// </summary>
    public class UserPreLoginEventArgs : HandledEventArgs
    {
        /// <summary>
        /// The User who fired the event.
        /// </summary>
        public PSPlayer User { get; set; }

        /// <summary>
        /// The User's login name.
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// The User's raw password.
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// EventArgs used for the <see cref="UserHooks.UserLogout"/> event.
    /// </summary>
    public class UserLogoutEventArgs
    {
        /// <summary>
        /// The User who fired the event.
        /// </summary>
        public PSPlayer User { get; set; }

        /// <summary>
        /// Initializes a new instance of the UserLogoutEventArgs class.
        /// </summary>
        /// <param name="User">The User who fired the event.</param>
        public UserLogoutEventArgs(PSPlayer user)
        {
            User = user;
        }
    }

    /// <summary>
    /// EventArgs used for the <see cref="UserHooks.UserCommand"/> event.
    /// </summary>
    public class UserCommandEventArgs : HandledEventArgs
    {
        /// <summary>
        /// The User who fired the event.
        /// </summary>
        public PSPlayer User { get; set; }

        /// <summary>
        /// The command's name that follows the <see cref="Commands.Specifier"/>.
        /// </summary>
        public string CommandName { get; set; }

        /// <summary>
        /// The command's full text.
        /// </summary>
        public string CommandText { get; set; }

        /// <summary>
        /// The command's parameters extracted from <see cref="CommandText"/>.
        /// </summary>
        public List<string> Parameters { get; set; }

        /// <summary>
        /// The full list of server commands.
        /// </summary>
        public IEnumerable<Command> CommandList { get; set; }
    }

    /// <summary>
    /// A collection of events fired by Users that can be hooked to.
    /// </summary>
    public static class UserHooks
    {
        /// <summary>
        /// The delegate of the <see cref="UserPostLogin"/> event.
        /// </summary>
        /// <param name="e">The EventArgs for this event.</param>
        public delegate void UserPostLoginD(UserPostLoginEventArgs e);
        /// <summary>
        /// Fired by Users after they've successfully logged in to a user account.
        /// </summary>
        public static event UserPostLoginD UserPostLogin;

        /// <summary>
        /// The delegate of the <see cref="UserPreLogin"/> event.
        /// </summary>
        /// <param name="e">The EventArgs for this event.</param>
        public delegate void UserPreLoginD(UserPreLoginEventArgs e);
        /// <summary>
        /// Fired by Users when sending login credentials to the server.
        /// </summary>
        public static event UserPreLoginD UserPreLogin;

        /// <summary>
        /// The delegate of the <see cref="UserLogout"/> event.
        /// </summary>
        /// <param name="e">The EventArgs for this event.</param>
        public delegate void UserLogoutD(UserLogoutEventArgs e);
        /// <summary>
        /// Fired by Users upon logging out from a user account.
        /// </summary>
        public static event UserLogoutD UserLogout;

        /// <summary>
        /// The delegate of the <see cref="UserCommand"/> event.
        /// </summary>
        /// <param name="e">The EventArgs for this event.</param>
        public delegate void UserCommandD(UserCommandEventArgs e);
        /// <summary>
        /// Fired by Users when using a command.
        /// </summary>
        public static event UserCommandD UserCommand;

        /// <summary>
        /// Fires the <see cref="UserPostLogin"/> event.
        /// </summary>
        /// <param name="ply">The User firing the event.</param>
        public static void OnUserPostLogin(PSPlayer ply)
        {
            if (UserPostLogin == null)
            {
                return;
            }

            UserPostLoginEventArgs args = new UserPostLoginEventArgs(ply);
            UserPostLogin(args);
        }

        /// <summary>
        /// Fires the <see cref="UserCommand"/> event.
        /// </summary>
        /// <param name="User">The User firing the event.</param>
        /// <param name="cmdName">The command name.</param>
        /// <param name="cmdText">The raw command text.</param>
        /// <param name="args">The command args extracted from the command text.</param>
        /// <param name="commands">The list of commands.</param>
        /// <returns>True if the event has been handled.</returns>
        public static bool OnUserCommand(PSPlayer User, string cmdName, string cmdText, List<string> args, ref IEnumerable<Command> commands)
        {
            if (UserCommand == null)
            {
                return false;
            }
            UserCommandEventArgs UserCommandEventArgs = new UserCommandEventArgs()
            {
                User = User,
                CommandName = cmdName,
                CommandText = cmdText,
                Parameters = args,
                CommandList = commands,
            };
            UserCommand(UserCommandEventArgs);
            return UserCommandEventArgs.Handled;
        }

        /// <summary>
        /// Fires the <see cref="UserPreLogin"/> event.
        /// </summary>
        /// <param name="ply">The User firing the event.</param>
        /// <param name="name">The user name.</param>
        /// <param name="pass">The password.</param>
        /// <returns>True if the event has been handled.</returns>
        public static bool OnUserPreLogin(PSPlayer ply, string name, string pass)
        {
            if (UserPreLogin == null)
                return false;

            var args = new UserPreLoginEventArgs { User = ply, LoginName = name, Password = pass };
            UserPreLogin(args);
            return args.Handled;
        }

        /// <summary>
        /// Fires the <see cref="UserLogout"/> event.
        /// </summary>
        /// <param name="ply">The User firing the event.</param>
        public static void OnUserLogout(PSPlayer ply)
        {
            if (UserLogout == null)
                return;

            var args = new UserLogoutEventArgs(ply);
            UserLogout(args);
        }
    }

}
