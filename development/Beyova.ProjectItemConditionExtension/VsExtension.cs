using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;

namespace Beyova.VsExtension
{
    public static class VsExtension
    {
        /// <summary>
        /// Gets the dt e2.
        /// </summary>
        /// <returns></returns>
        public static DTE2 GetDTE2()
        {
            return Package.GetGlobalService(typeof(DTE)) as DTE2;
        }

        /// <summary>
        /// Gets the solution configurations.
        /// </summary>
        /// <returns></returns>
        public static SolutionConfigurations GetSolutionConfigurations()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return GetDTE2().Solution.SolutionBuild.SolutionConfigurations;
        }

        /// <summary>
        /// Determines whether the specified array has item.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   <c>true</c> if the specified array has item; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasItem(this Array array, object obj)
        {
            if (array != null && obj != null)
            {
                foreach (var item in array)
                {
                    if (item == obj)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified configuration manager has configuration.
        /// </summary>
        /// <param name="configurationManager">The configuration manager.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        ///   <c>true</c> if the specified configuration manager has configuration; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasConfiguration(this ConfigurationManager configurationManager, string name)
        {
            return configurationManager != null
                && !string.IsNullOrWhiteSpace(name)
                && ((Array)configurationManager.ConfigurationRowNames).HasItem(name);
        }

        /// <summary>
        /// Gets the selected project items.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ProjectItem> GetSelectedProjectItems()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var result = new List<ProjectItem>();

            DTE2 _applicationObject = GetDTE2();
            UIHierarchy uih = _applicationObject.ToolWindows.SolutionExplorer;
            Array selectedItems = (Array)uih.SelectedItems;
            if (null != selectedItems)
            {
                foreach (UIHierarchyItem selItem in selectedItems)
                {
                    ProjectItem projectItem = selItem.Object as ProjectItem;
                    if (projectItem != null)
                    {
                        result.Add(projectItem);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the solution projects.
        /// </summary>
        /// <returns></returns>
        public static Projects GetSolutionProjects()
        {
            return GetDTE2().Solution.Projects;
        }

        /// <summary>
        /// Clears the specified configuration manager.
        /// </summary>
        /// <param name="solutionConfigurations">The solution configurations.</param>
        /// <param name="name">The name.</param>
        /// <param name="stringComparison">The string comparison.</param>
        public static void Delete(this SolutionConfigurations solutionConfigurations, string name, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            if (solutionConfigurations != null && !string.IsNullOrWhiteSpace(name))
            {
                foreach (SolutionConfiguration item in solutionConfigurations)
                {
                    if (item != null && item.Name.Equals(name, stringComparison))
                    {
                        item.Delete();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static Property GetProperty(this Properties properties, string key)
        {
            if (properties != null && !string.IsNullOrWhiteSpace(key))
            {
                foreach (Property item in properties)
                {
                    if (item.Name == key)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetKeys(this Properties properties)
        {
            List<string> result = new List<string>();

            if (properties != null)
            {
                foreach (Property item in properties)
                {
                    result.Add(item.Name);
                }
            }

            return result;
        }

        /// <summary>
        /// Sets the project item property as attribute.
        /// </summary>
        /// <param name="projectItem">The project item.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        public static void SetPropertyAsAttribute(this ProjectItem projectItem, string attributeName,
    string attributeValue)
        {
            if (projectItem != null && !string.IsNullOrWhiteSpace(attributeName))
            {
                IVsHierarchy hierarchy;
                ((IVsSolution)Package.GetGlobalService(typeof(SVsSolution)))
                    .GetProjectOfUniqueName(projectItem.ContainingProject.UniqueName, out hierarchy);

                IVsBuildPropertyStorage buildPropertyStorage = hierarchy as IVsBuildPropertyStorage;

                if (buildPropertyStorage != null)
                {
                    string fullPath = (string)projectItem.Properties.Item("FullPath").Value;

                    uint itemId;
                    hierarchy.ParseCanonicalName(fullPath, out itemId);

                    buildPropertyStorage.SetItemAttribute(itemId, attributeName, attributeValue);
                }
            }
        }

        /// <summary>
        /// Gets the include path.
        /// </summary>
        /// <param name="projectItem">The project item.</param>
        /// <returns></returns>
        public static string GetIncludePath(this ProjectItem projectItem)
        {
            if (projectItem != null)
            {
                var projectItemPath = projectItem.Properties.Item("FullPath").Value.ToString();
                var directoryPath = Path.GetDirectoryName(projectItem.ContainingProject.FullName);
                if (projectItemPath.StartsWith(directoryPath))
                {
                    return projectItemPath.Substring(directoryPath.Length).TrimStart('\\');
                }

                return projectItemPath;
            }

            return null;
        }
    }
}