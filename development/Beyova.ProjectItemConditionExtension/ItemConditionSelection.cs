using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Beyova.VsExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ItemConditionSelection
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("75ee5c42-e3e5-4d20-969f-dcbbb9f592ac");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemConditionSelection"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ItemConditionSelection(AsyncPackage package, OleMenuCommandService commandService)
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
        public static ItemConditionSelection Instance
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
            // Switch to the main thread - the call to AddCommand in ItemConditionSelection's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new ItemConditionSelection(package, commandService);
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

            var selectedItems = VsExtension.GetSelectedProjectItems();
            var operatableItems = (selectedItems?.Where(x =>
            {
                var v = x.Properties.Item("BuildAction")?.Value;

                //https://docs.microsoft.com/en-us/dotnet/api/vslangproj.prjbuildaction
                return (v != null && (int)v != 3);
            }));

            if (!operatableItems.Any())
            {
                VsShellUtilities.ShowMessageBox(
                package,
                "Nothing selected is operatable",
                "Item Condition Selection",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }

            Dictionary<Project, List<ProjectItem>> items = new Dictionary<Project, List<ProjectItem>>();

            foreach (var item in operatableItems)
            {
                if (!items.ContainsKey(item.ContainingProject))
                {
                    items.Add(item.ContainingProject, new List<ProjectItem>());
                }

                items[item.ContainingProject].Add(item);

                //  item.SetPropertyAsAttribute("Condition", string.Format(" '$(Configuration)|$(Platform)' == '{0}|AnyCPU' ", conditionPicker.SelectedConfiguration));
            }


            var conditionPicker = items.Keys.Count == 1 ? new ConditionPicker(items.Keys.First()) : new ConditionPicker();
            conditionPicker.ShowDialog();

            if (!string.IsNullOrWhiteSpace(conditionPicker.SelectedConfiguration))
            {
                foreach (var p in items)
                {
                    XDocument projectXml = XDocument.Load(p.Key.FullName);

                    foreach (var item in p.Value)
                    {
                        var xml = FindProjectItemXml(projectXml, VsExtension.GetIncludePath(item));

                        if (xml != null)
                        {
                            xml.SetAttributeValue("Condition", string.Format(" '$(Configuration)|$(Platform)' == '{0}|AnyCPU' ", conditionPicker.SelectedConfiguration));
                        }
                    }

                    projectXml.Save(p.Key.FullName);
                }
            }

            //string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", GetType().FullName);
            //string title = "ItemConditionSelection";

            //// Show a message box to prove we were here
            //VsShellUtilities.ShowMessageBox(
            //    package,
            //    message,
            //    title,
            //    OLEMSGICON.OLEMSGICON_INFO,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        /// <summary>
        /// Finds the project item XML.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="itemPath">The item path.</param>
        /// <returns></returns>
        private XElement FindProjectItemXml(XDocument document, string itemPath)
        {
            if (document != null && !string.IsNullOrWhiteSpace(itemPath))
            {
                var itemGroups = document.Root?.Elements().Where(x => x.Name.LocalName == "ItemGroup");
                foreach (var item in itemGroups)
                {
                    foreach (var item2 in item.Elements())
                    {
                        if (item2.Attribute("Include")?.Value.Equals(itemPath, StringComparison.OrdinalIgnoreCase) ?? false)
                        {
                            return item2;
                        }
                    }
                }
            }

            return null;
        }
    }
}
