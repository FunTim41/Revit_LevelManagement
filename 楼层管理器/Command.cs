using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit.Async;

namespace 楼层管理器
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements
        )
        {
            try
            {
                RevitTask.Initialize(commandData.Application);
                MainView mainView = new MainView(commandData);
                mainView.Show();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Tip", ex.Message);
                throw;
            }

            return Result.Succeeded;
        }
    }
}
