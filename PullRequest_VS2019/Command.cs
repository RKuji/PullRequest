using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace PullRequest_VS2019
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class Command
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("9cd9d7a3-8267-4209-aca1-094ebdba37a8");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private Command(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static Command Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in Command's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new Command(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string remote = ExecuteGit("remote get-url origin");
            string branch = ExecuteGit("branch --show-current");
            string parentBranch = GetParentBranch(branch);

            string normalizedRepository = NormalizeRemoteUrl(remote);
            string url = $"{normalizedRepository}/compare/{parentBranch}...{branch}?quick_pull=1";

            var dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(SDTE));

            string solutionDir = Path.GetDirectoryName(dte.Solution.FullName);

            var browseProcess = new ProcessStartInfo
            {
                FileName = url,
                WorkingDirectory = solutionDir,
                RedirectStandardOutput = false,
                UseShellExecute = true,
                CreateNoWindow = true
            };

            using (Process p = Process.Start(browseProcess)) { };
        }

        private string ExecuteGit(string arguments)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var dte = (EnvDTE.DTE)Package.GetGlobalService(typeof(SDTE));

            string solutionDir = Path.GetDirectoryName(dte.Solution.FullName);

            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = solutionDir,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process p = Process.Start(psi))
            {
                string result = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                return result.Trim();
            };
        }

        private string GetParentBranch(string branch)
        {
            int pos = branch.LastIndexOf('-');
            return pos >= 0 ? branch.Substring(0, pos) : branch;
        }

        private static string NormalizeRemoteUrl(string remote)
        {
            remote = remote.Trim();

            if (remote.StartsWith("git@github.com:"))
            {
                remote = remote.Replace(
                    "git@github.com:",
                    "https://github.com/");
            }

            if (remote.EndsWith(".git"))
            {
                remote = remote.Substring(0, remote.Length - 4);
            }

            return remote;
        }
    }
}
