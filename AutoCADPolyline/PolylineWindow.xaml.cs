using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Microsoft.Win32;
using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Autodesk.AutoCAD.Geometry;

using acad = Autodesk.AutoCAD;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace AutoCADPolyline
{

    public partial class PolylineWindow : System.Windows.Window
    {
        PolylineModel data;
        public PolylineWindow(PolylineModel polylineModel)
        {
            InitializeComponent();
            data = polylineModel;
            this.DataContext = data;
        }

        private void BtnColor_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            var result = colorDialog.ShowModal();

            if (result != true) return;

            var acColor = colorDialog.Color;
            byte byt = Convert.ToByte(acColor.ColorIndex);
            int rgb = acad.Colors.EntityColor.LookUpRgb(byt);
            byte b = Convert.ToByte((rgb & 0xffL));
            byte g = Convert.ToByte((rgb & 0xff00L) >> 8);
            byte r = Convert.ToByte(rgb >> 16);
            data.Color = (r, g, b);
            Color color = Color.FromRgb(r, g, b);
            rectColor.Fill = new SolidColorBrush(color);

        }

        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Filter = "Text files (*.txt)|*.txt";
            var result = fileDialog.ShowDialog();
            if (result != true) return;
            string filePath = fileDialog.FileName;
            string[] vertexes;

            using (StreamReader sr = new StreamReader(filePath))
            {
                vertexes = sr.ReadToEnd().Split('\n').Select(s => s.Trim('\n', '\r')).ToArray();
            }

            for (int i = 0; i < vertexes.Count(); i++)
            {
                var vertexParams = vertexes[i].Split(' ').Select(s => s.Trim('\r', ' ')).Select(s => Convert.ToDouble(s)).ToArray();
                var vertex = new PolylineModel.Vertex
                {
                    X = vertexParams[0],
                    Y = vertexParams[1],
                    Bulge = vertexParams.Count() != 3 ? 0 : Math.Tan(0.25*vertexParams[2]*Math.PI/180)
                };

                data.Vertexes.Add(vertex);
            }
            
            if (data.IsValid)
                btnOK.IsEnabled = true;
        }

        private void BtnMouseCoord_Click(object sender, RoutedEventArgs e)
        {
            var acDoc = acadApp.DocumentManager.MdiActiveDocument;
            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");
            using (var editorUserInteraction = acDoc.Editor.StartUserInteraction(this)) { pPtRes = acDoc.Editor.GetPoint(pPtOpts); }

            if (pPtRes.Status != PromptStatus.OK)
                return;

            Point3d plStart = pPtRes.Value;
            data.Vertexes[0].X = plStart.X;
            data.Vertexes[0].Y = plStart.Y;
            textX.Text = $"{plStart.X}";
            textY.Text = $"{plStart.Y}";
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {

            if (!data.IsValid)
                return;

            var acDoc = acadApp.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                var acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (var acPoly = new Polyline2d())
                {
                    for (int i = 1; i < data.Vertexes.Count; i++)
                    {
                        acPoly.NonDBAppendVertex(new Vertex2d(
                                new Point3d(data.Vertexes[0].X + data.Vertexes[i].X,
                                            data.Vertexes[0].Y + data.Vertexes[i].Y, 0), 
                                    data.Vertexes[i].Bulge, data.Thickness, data.Thickness, 0
                            ));
                    }
                    acPoly.SetDatabaseDefaults();
                    acPoly.Closed = data.IsClose;
                    if (data.IsSmoothing)
                        acPoly.ConvertToPolyType(Poly2dType.FitCurvePoly);
                    acPoly.Color = acad.Colors.Color.FromRgb(data.Color.R, data.Color.G, data.Color.B);

                    acBlkTblRec.AppendEntity(acPoly);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);
                }
                acTrans.Commit();
            }
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
       
    }
}
