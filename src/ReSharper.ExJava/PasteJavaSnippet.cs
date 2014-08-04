using System;
using System.Windows.Forms;
using JetBrains.ActionManagement;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.DataFlow;
using JetBrains.Threading;
using JetBrains.Util;
using JetBrains.Util.Logging;
using JetBrains.VsIntegration;
using JetBrains.VsIntegration.ActionManagement;

namespace ReSharper.ExJava
{
    [ActionHandler("PasteJavaSnippet")]
    public class PasteJavaSnippet : IActionHandler
    {
        public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
        {
            return true;
        }

        public void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            IDataObject dataObject = Clipboard.GetDataObject();
            string contents = TryGetString(dataObject);
            if (string.IsNullOrWhiteSpace(contents))
            {
                return;
            }

            var csharpCode = TryConvertJavaToCSharp(contents);
            if (string.IsNullOrWhiteSpace(csharpCode))
            {
                return;
            }

            Paste(csharpCode);
        }

        private static string TryConvertJavaToCSharp(string contents)
        {
            try
            {
                return JavaToCSharp.JavaToCSharpConverter.ConvertText(contents);
            }
            catch (Exception ex)
            {
                Logger.LogMessage("Failed to convert Java code to C#");
                Logger.LogExceptionSilently(ex);
            }

            return null;
        }

        private static void Paste(string text)
        {
            var locks = Shell.Instance.GetComponent<IShellLocks>();

            Action action = (() =>
            {
                try
                {
                    Shell.Instance.GetComponent<JetBrains.UI.Clipboard>().SetText(text);
                    object obj = null;
                    Shell.Instance.GetComponent<VsActionManager>().ExecuteVsCommand(VsConstants.GUID_VSStandardCommandSet97, VSStd97CmdID.CmdIdPaste, ref obj, ref obj);
                }
                catch (Exception)
                {
                    Logger.LogMessage(LoggingLevel.NORMAL, "Failed to update clipboard");
                }
            });
            locks.Queue("PasteJavaSnippet", () => locks.ExecuteOrQueueWhenNotGuarded(EternalLifetime.Instance, "PasteJavaSnippet2", action));
        }

        private string TryGetString(IDataObject dataObject)
        {
            try
            {
                return (string)dataObject.GetData(DataFormats.StringFormat);
            }
            catch (Exception ex)
            {
                Logger.LogMessage("Failed to get data from clipboard");
                Logger.LogExceptionSilently(ex);
            }

            return null;
        }
    }
}