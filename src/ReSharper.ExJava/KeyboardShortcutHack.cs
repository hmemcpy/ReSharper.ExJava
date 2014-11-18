using System;
using EnvDTE;
using JetBrains.Application;
using JetBrains.Application.Components;
using JetBrains.Util.Lazy;

namespace ReSharper.ExJava
{
    internal static class VisualStudioHelper
    {
        public static bool VisualStudioIsPresent()
        {
            return Shell.Instance.HasComponent<DTE>();
        }

        /// <summary>
        /// Must run on main UI thread
        /// </summary>
        public static void AssignKeyboardShortcutIfMissing(string macroName, string keyboardShortcut)
        {
            var dte = Shell.Instance.GetComponent<DTE>();

            var command = dte.Commands.Item(macroName);

            if (command != null)
            {
                var currentBindings = (object[])command.Bindings;

                if (currentBindings.Length == 1)
                {
                    if (currentBindings[0].ToString() == keyboardShortcut)
                    {
                        GetOutputWindowPane(dte, "TestCop", true).OutputString(
                            string.Format("Keyboard shortcut for '{0}' is '{1}'\n", macroName, keyboardShortcut));
                        return;
                    }
                }

                command.Bindings = string.IsNullOrEmpty(keyboardShortcut)
                    ? new Object[] { }
                    : new Object[] { keyboardShortcut };
                GetOutputWindowPane(dte, "ReSharper", true).OutputString(
                    string.Format("Setting keyboard shortcut for '{0}' to '{1}'\n", macroName, keyboardShortcut)
                    );
            }
        }

        public static OutputWindowPane GetOutputWindowPane(string name, bool show)
        {
            var dte = Shell.Instance.GetComponent<DTE>();
            return GetOutputWindowPane(dte, name, show);
        }

        /// <summary>
        /// Must run on main UI thread
        /// </summary>
        private static OutputWindowPane GetOutputWindowPane(DTE dte, string name, bool show)
        {
            /* If compilation generates:: 'EnvDTE.Constants' can be used only as one of its applicable interfaces
             * then set DTE assembly reference property Embed Interop Types = false  */

            var win = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
            if (show)
            {
                win.Visible = true;
            }

            var ow = (OutputWindow)win.Object;
            OutputWindowPane owpane;
            try
            {
                owpane = ow.OutputWindowPanes.Item(name);
            }
            catch (Exception)
            {
                owpane = ow.OutputWindowPanes.Add(name);
            }

            owpane.Activate();
            return owpane;
        }
    }
}