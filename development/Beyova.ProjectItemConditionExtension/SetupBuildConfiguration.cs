﻿using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Beyova.VsExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SetupBuildConfiguration
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 256;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("02cac10d-8a08-4bc4-a942-ce4b18b43141");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupBuildConfiguration"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private SetupBuildConfiguration(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SetupBuildConfiguration Instance
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
                return package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in SetupBuildConfiguration's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new SetupBuildConfiguration(package, commandService);
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

            var dte = VsExtension.GetDTE2();

            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                project.ConfigurationManager.AddConfigurationRow("DEV", "Debug", true);
                project.ConfigurationManager.AddConfigurationRow("QA", "Debug", true);
                project.ConfigurationManager.AddConfigurationRow("STAGING", "Release", true);
                project.ConfigurationManager.AddConfigurationRow("PROD", "Release", true);

                if (project.ConfigurationManager.HasConfiguration("Debug"))
                {
                    project.ConfigurationManager.DeleteConfigurationRow("Debug");
                }
                if (project.ConfigurationManager.HasConfiguration("Release"))
                {
                    project.ConfigurationManager.DeleteConfigurationRow("Release");
                }
            }

            dte.Solution.SolutionBuild.SolutionConfigurations.Delete("Debug", StringComparison.OrdinalIgnoreCase);
            dte.Solution.SolutionBuild.SolutionConfigurations.Delete("Release", StringComparison.OrdinalIgnoreCase);

            dte.Documents.SaveAll();
        }
    }
}
