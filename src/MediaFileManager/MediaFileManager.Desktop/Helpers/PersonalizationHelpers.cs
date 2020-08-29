using System;
using System.Collections.Generic;
using System.Windows;

namespace MediaFileManager.Desktop.Helpers
{
    public static class PersonalizationHelpers
    {
        public static List<string> ThemeNames { get; } = new List<string> 
        {
            "Fluent", "VisualStudio2019", "Crystal", "Expression_Dark", "Green", 
            "Material", "Office2013", "Office2016", "Summer", "Transparent", 
            "Vista", "VisualStudio2013", "Windows7", "Windows8",
        };

        public static void UpdateTheme(string themeAssemblyName)
        {
            Application.Current.Resources.MergedDictionaries.Clear();

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/Telerik.Windows.Themes.{themeAssemblyName};component/Themes/System.Windows.xaml", UriKind.RelativeOrAbsolute)
            });

            var ctrlAssemblyNames = new[] { "Controls", "Controls.Data", "Controls.GridView", "Controls.Input", "Controls.Navigation", "Controls.Docking" };

            foreach (var ctrlAssemblyName in ctrlAssemblyNames)
            {
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri($"/Telerik.Windows.Themes.{themeAssemblyName};component/Themes/Telerik.Windows.{ctrlAssemblyName}.xaml", UriKind.RelativeOrAbsolute)
                });
            }
        }

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

            var ctrlAssemblyNames = new[] { "Controls", "Controls.Data", "Controls.GridView", "Controls.Input", "Controls.Navigation", "Controls.Docking" };

            foreach (var ctrlAssemblyName in ctrlAssemblyNames)
            {
                source.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri($"/Telerik.Windows.Themes.{themeAssemblyName};component/Themes/Telerik.Windows.{ctrlAssemblyName}.xaml", UriKind.RelativeOrAbsolute)
                });
            }
        }
    }
}
