using System;
using System.Collections.Generic;
using System.Windows;

namespace MediaFileManager.Desktop.Helpers
{
    public static class PersonalizationHelpers
    {
        private static readonly string[] CtrlAssemblyNames = 
        {
            "Controls", 
            "Controls.Data", 
            "Controls.Docking", 
            "Controls.FileDialogs",
            "Controls.GridView", 
            "Controls.Input", 
            "Controls.Navigation"
        };

        public static List<string> ThemeNames { get; } = new List<string> 
        {
            "Fluent", "VisualStudio2019", "Crystal", "Expression_Dark", "Green", 
            "Material", "Office2013", "Office2016", "Summer", "Transparent", 
            "Vista", "VisualStudio2013", "Windows7", "Windows8",
        };

        /// <summary>
        /// Helper for merging Telerik theme into the App Resources
        /// </summary>
        /// <param name="themeAssemblyName">Name of the theme to use (e.g. "Fluent" or "Office2013"). Query this class's ThemeNames list for available names to use.</param>
        public static void UpdateTheme(string themeAssemblyName)
        {
            Application.Current.Resources.MergedDictionaries.Clear();

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{themeAssemblyName};component/Themes/System.Windows.xaml", UriKind.RelativeOrAbsolute)
            });

            foreach (var ctrlAssemblyName in CtrlAssemblyNames)
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri($"/Telerik.Windows.Themes.{themeAssemblyName};component/Themes/Telerik.Windows.{ctrlAssemblyName}.xaml", UriKind.RelativeOrAbsolute)
                });
            }
        }

        /// <summary>
        /// Helper for merging Telerik themes into a pre-defined ResourceDictionary. This is useful for custom controls that do not have access to the App resources
        /// </summary>
        /// <param name="source">The ResourceDictionary to merge the Telerik theme ResourceDictionaries into.</param>
        /// <param name="themeAssemblyName">Name of the theme to use (e.g. "Fluent" or "Office2013"). Query this class's ThemeNames list for available names to use.</param>
        public static void UpdateTheme(ResourceDictionary source, string themeAssemblyName)
        {
            if(source == null)
            {
                throw new Exception("You must pass a valid ResourceDictionary to reset the themes.");
            }

            source.MergedDictionaries.Clear();

            source.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{themeAssemblyName};component/Themes/System.Windows.xaml", UriKind.RelativeOrAbsolute)
            });

            foreach (var ctrlAssemblyName in CtrlAssemblyNames)
            {
                source.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri($"/Telerik.Windows.Themes.{themeAssemblyName};component/Themes/Telerik.Windows.{ctrlAssemblyName}.xaml", UriKind.RelativeOrAbsolute)
                });
            }
        }
    }
}
