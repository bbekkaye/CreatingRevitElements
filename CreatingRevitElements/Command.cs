#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using  Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
#endregion

namespace CreatingRevitElements
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            // public IList<Element> PickElementsByRectangle(string statusPrompt)
            // Use the rectangle picking tool to identify model elements to select.

            IList<Element> pickedElements = uidoc.Selection.PickElementsByRectangle("Select by rectangle");
            List<CurveElement> curveElement = new List<CurveElement>();

            // Collect Ids of all picked elements
            IList<ElementId> idsToSelect = new List<ElementId>(pickedElements.Count);

            WallType curWallType = GetWallTypeByName(doc, @"Generic - 8""");
            Level curLevel = GetLevelByName(doc, "Level 1");

            MEPSystemType curSystemType = GetSystemTypeByName(doc, "Domestic Hot Water");
            PipeType curPipeType = GetPipeTypeByName(doc, "Default");

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Create Wall");

                foreach (Element element in pickedElements)
                {
                    if (!(element is CurveElement)) continue;
                    CurveElement curve = (CurveElement)element;
                    CurveElement curve2 = (CurveElement)curve;

                    curveElement.Add(curve);

                    GraphicsStyle curGS = curve.LineStyle as GraphicsStyle;
                    Curve curCurve = curve.GeometryCurve;
                    XYZ startPoint = curCurve.GetEndPoint(0);
                    XYZ endPoint = curCurve.GetEndPoint(1);

                    //Wall newWall = Wall.Create(doc, curCurve, curWallType.Id, curLevel.Id, 15, 0, false, false);
                    Pipe newPipe = Pipe.Create(doc, curSystemType.Id, curPipeType.Id, curLevel.Id, startPoint,
                        endPoint);

                    Debug.WriteLine(curGS.Name);
                }

                tx.Commit();
            }

            // Update the current selection
            TaskDialog.Show("Revit", $"{curveElement.Count} Elements Selected.");
            return Result.Succeeded;
        }

        private WallType GetWallTypeByName(Document doc, string wallTypeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(WallType));

            foreach (Element curElement in collector)
            {
                WallType wallType = curElement as WallType;
                if (wallType.Name == wallTypeName)
                    return wallType;
            }

            return null;
        }

        private Level GetLevelByName(Document doc, string levelName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(Level));

            foreach (Element curElement in collector)
            {
                Level level = curElement as Level;
                if (level.Name == levelName)
                    return level;
            }

            return null;
        }

        private MEPSystemType GetSystemTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(MEPSystemType));

            foreach (Element curElement in collector)
            {
                MEPSystemType curType = curElement as MEPSystemType;
                if (curType.Name == typeName)
                    return curType;
            }

            return null;
        }

        private PipeType GetPipeTypeByName(Document doc, string typeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(PipeType));

            foreach (Element curElement in collector)
            {
                PipeType curType = curElement as PipeType;
                if (curType.Name == typeName)
                    return curType;
            }

            return null;
        }
    }
}
