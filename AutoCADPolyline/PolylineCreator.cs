using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCADPolyline
{
    public class PolylineCreator
    {
        [CommandMethod("AddPolyline")]
        public void AddPolyline()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            PolylineModel polylineModel = new PolylineModel();
            
            PolylineWindow pwindow = new PolylineWindow(polylineModel);
            Application.ShowModalWindow(pwindow);
        }

    }
}
