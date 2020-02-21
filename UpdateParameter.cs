using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MonitorProgressCSharp.Views;

namespace MonitorProgressCSharp
{
    class UpdateParameter
    {
        public UpdateParameter(ExternalCommandData commandData)
        {
            App = commandData.Application;
            Doc = App.ActiveUIDocument.Document;
        }

        UIApplication App { get; }
        Document Doc { get; }
        Element Element { get; set; }
        Parameter Parameter { get; set; }

        public void StartJob()
        {
            SelectInstance();

            ProgressStatusUI progressBarUI = new ProgressStatusUI();
            progressBarUI.ContentRendered += ProgressBarUI_ContentRendered;
            progressBarUI.ShowDialog();                                                                                               
        }

        void ProgressBarUI_ContentRendered(object sender, EventArgs e)
        {
            ProgressStatusUI progressBarUI = sender as ProgressStatusUI;

            if (progressBarUI == null)
                throw new Exception("Error trying to create progress bar window");

            for (int i = 1; i <= 100; i++)
            {
                ChangeParameter(i.ToString());
                progressBarUI.UpdateStatus(string.Format("Update parameter {0}", i.ToString()), i);
                if (progressBarUI.ProcessCancelled)
                    break;
            }

            progressBarUI.JobCompleted();
        }

        void ChangeParameter(string newValue)
        {
            using (Transaction t = new Transaction(Doc, "Set Parameter"))
            {
                t.Start();

                Parameter.Set(newValue);
                // System.Threading.Thread.Sleep(50);

                t.Commit();
            }
        }

        void SelectInstance()
        {
            Selection sel = App.ActiveUIDocument.Selection;
            Reference reference = sel.PickObject(ObjectType.Element, "Select an element");
            Element = Doc.GetElement(reference);

            Parameter = Element.get_Parameter(BuiltInParameter.DOOR_NUMBER);

            if (Parameter.IsReadOnly)
                throw new Exception("Please select an instance with editable 'Mark' parameter");
        }
    }
}