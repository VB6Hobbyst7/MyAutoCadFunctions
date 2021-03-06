﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using System.IO;

namespace XrefManager.Functions
{
    class EditView
    {
        public void SetTopView()
        {
            // Get the current document and database

            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            var FD = new Microsoft.Win32.OpenFileDialog();

            FD.Filter = "AutoCAD Files | *.dwg";
            FD.ShowDialog();

            //create a database and try to load the file
            Database acCurDb = new Database(false, true);
            using (acCurDb)
            {
                try
                {
                    acCurDb.ReadDwgFile(FD.FileName, FileShare.ReadWrite, false, "");

                    // Start a transaction
                    using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                    {
                        // Open the Viewport table for read
                        ViewportTable acVportTbl;
                        acVportTbl = acTrans.GetObject(acCurDb.ViewportTableId, OpenMode.ForWrite) as ViewportTable;
                        {
                            var first = true;
                            foreach (ObjectId acObjId in acVportTbl)
                            {
                                // Open the object for read
                                ViewportTableRecord acVportTblRec;
                                acVportTblRec = acTrans.GetObject(acObjId, OpenMode.ForWrite) as ViewportTableRecord;

                                if (first)
                                {
                                    var vStyle = get2DVisualStyle(acDoc, acCurDb);


                                    acVportTblRec.UpgradeOpen();
                                    acVportTblRec.SetViewDirection(OrthographicView.TopView);

                                    first = false;

                                    if (vStyle == null)
                                    {
                                        return;
                                    }

                                    acVportTblRec.VisualStyleId = vStyle.Id;

                                }
                                else
                                {
                                    acVportTblRec.Erase();
                                }

                            }
                            // Commit the changes
                            acTrans.Commit();
                        }
                    }

                    acCurDb.SaveAs(FD.FileName, false, DwgVersion.Current, null);

                }
                catch (Autodesk.AutoCAD.Runtime.Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message);
                }
            }
        }


        private DBVisualStyle get2DVisualStyle(Document acDoc, Database acCurDb)
        {

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                DBDictionary vStyles = acTrans.GetObject(acCurDb.VisualStyleDictionaryId,
                                                         OpenMode.ForRead) as DBDictionary;

                // Output a message to the Command Line history
                acDoc.Editor.WriteMessage("\nVisual styles: ");

                // Step through the dictionary
                foreach (DBDictionaryEntry entry in vStyles)
                {
                    // Get the dictionary entry
                    DBVisualStyle vStyle = vStyles.GetAt(entry.Key).GetObject(OpenMode.ForRead) as DBVisualStyle;

                    // If the visual style is not marked for internal use then output its name
                    if (vStyle.InternalUseOnly == false)
                    {
                        if (vStyle.Name.ToUpper().Contains("WIRE"))
                        {
                            return vStyle;
                        }
                    }
                }
            }
            return null;
        }
    }
}
