﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrefManager.Forms;
using XrefManager.Models;
using XrefManager.Workers;

namespace XrefManager
{
    public class ReplaceValue
    {
        public void openDialogeBox()
        {
            var xmlReader = new ReadXml();

            if (xmlReader.checkXmlPath())
            {
                var drawingList = new List<string>();

                using (var _form = new PurgeAttributeForm())
                {
                    _form.SetTabIndex(1);

                    var result = _form.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        drawingList = _form.attributeDrawingList;
                    }
                    if (result == System.Windows.Forms.DialogResult.None)
                    {
                        return;
                    }

                    ReplaceStringValue(drawingList, _form.attBlockname, _form.attAttributeName, _form.attOldValue, _form.attNewValue);

                }
            }
        }

        public void getBlockData(PurgeAttributeForm form)
        {
            form.Close();
            form.Dispose();

            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            var blockData = new BlockData();
            blockData.AttNameAndvalue = new List<AttName_Value>();

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Request for objects to be selected in the drawing area

                var opt = new PromptSelectionOptions();
                opt.SingleOnly = true;

                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(opt);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;

                    // Step through the objects in the selection set
                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        Entity ent = acTrans.GetObject(acSSObj.ObjectId, OpenMode.ForRead) as Entity;

                        BlockReference br = ent as BlockReference;
                        if (br != null)
                        {
                            blockData.BlockName = br.Name;

                            foreach (ObjectId attribute in br.AttributeCollection)
                            {
                                DBObject obj = acTrans.GetObject(attribute, OpenMode.ForRead);
                                AttributeReference ar = obj as AttributeReference;
                                if (ar != null)
                                {
                                    blockData.AttNameAndvalue.Add(new AttName_Value { attName = ar.Tag, attValue = ar.TextString });
                                }
                            }

                        }

                        // Save the new object to the database
                        acTrans.Dispose();
                    }

                    // Dispose of the transaction
                }
            }

            using (var _form = new PurgeAttributeForm())
            {
                var drawingList = new List<string>();

                _form.SetTabIndex(1);
                _form.blockData = blockData;
                _form.setValueFromInput();

                var result = _form.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    drawingList = _form.attributeDrawingList;
                }
                if (result == System.Windows.Forms.DialogResult.None)
                {
                    return;
                }

                ReplaceStringValue(drawingList, _form.attBlockname, _form.attAttributeName, _form.attOldValue, _form.attNewValue);
            }
        }

        public void ReplaceStringValue(List<string> drawingList, string blockName, string attributeTag, string oldString, string newString)
        {
            foreach (var drawing in drawingList)
            {
                var openDocs = Application.DocumentManager;

                if (DrawingIsOpen(drawing))
                {
                    foreach (Document openDoc in openDocs)
                    {
                        if (openDoc.Name == drawing)
                        {
                            using (DocumentLock docloc = openDoc.LockDocument())
                            {
                                using (Transaction trx = openDoc.TransactionManager.StartTransaction())
                                {
                                    ChangeAttributeValuePaperSpace(trx, openDoc.Database, blockName, attributeTag, oldString, newString);
                                    ChangeAttributeValueModelSpace(trx, openDoc.Database, blockName, attributeTag, oldString, newString);
                                    trx.Commit();
                                    trx.Dispose();
                                }
                            }
                        }
                    }
                }
                else
                {
                    Database xrefDb = new Database(false, true);

                    xrefDb.ReadDwgFile(drawing, FileShare.ReadWrite, false, "");

                    try
                    {
                        using (Transaction trx = xrefDb.TransactionManager.StartTransaction())
                        {
                            ChangeAttributeValuePaperSpace(trx, xrefDb, blockName, attributeTag, oldString, newString);
                            ChangeAttributeValueModelSpace(trx, xrefDb, blockName, attributeTag, oldString, newString);
                            trx.Commit();
                            trx.Dispose();
                        }
                    }
                    catch (Exception) { }
                    xrefDb.CloseInput(true);
                    xrefDb.SaveAs(drawing, false, DwgVersion.Current, null);
                }

            }
        }

        private bool DrawingIsOpen(string drawing)
        {
            var openDocs = Application.DocumentManager;

            foreach (Document openDoc in openDocs)
            {
                if (openDoc.Name == drawing)
                {
                    return true;
                }
            }
            return false;
        }

        private void ChangeAttributeValuePaperSpace(Transaction trx, Database db, string blockName, string attributeTag, string oldString, string newString)
        {
            ObjectId psId;
            BlockTable bt = (BlockTable)trx.GetObject(db.BlockTableId, OpenMode.ForRead);
            psId = bt[BlockTableRecord.PaperSpace];

            BlockTableRecord btr = (BlockTableRecord)trx.GetObject(psId, OpenMode.ForRead);

            foreach (var entId in btr)
            {
                Entity ent = trx.GetObject(entId, OpenMode.ForRead) as Entity;
                if (ent != null)
                {
                    BlockReference br = ent as BlockReference;
                    if (br != null)
                    {
                        BlockTableRecord bd = (BlockTableRecord)trx.GetObject(br.BlockTableRecord, OpenMode.ForRead);

                        // ... to see whether it's a block with the name we're after

                        if (bd.Name.ToUpper() == blockName.ToUpper())
                        {
                            // Check each of the attributes...

                            foreach (ObjectId arId in br.AttributeCollection)
                            {
                                DBObject obj = trx.GetObject(arId, OpenMode.ForRead);
                                AttributeReference ar = obj as AttributeReference;
                                if (ar != null)
                                {
                                    // ... to see whether it has
                                    // the tag we're after

                                    if (ar.Tag.ToUpper() == attributeTag.ToUpper())
                                    {
                                        // check if attribute has correct value
                                        if (ar.TextString == oldString || oldString == "*")
                                        {
                                            // If so, update the value
                                            ar.UpgradeOpen();
                                            ar.TextString = newString;
                                            ar.DowngradeOpen();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ChangeAttributeValueModelSpace(Transaction trx, Database db, string blockName, string attributeTag, string oldString, string newString)
        {
            ObjectId psId;
            BlockTable bt = (BlockTable)trx.GetObject(db.BlockTableId, OpenMode.ForRead);
            psId = bt[BlockTableRecord.ModelSpace];

            BlockTableRecord btr = (BlockTableRecord)trx.GetObject(psId, OpenMode.ForRead);

            foreach (var entId in btr)
            {
                Entity ent = trx.GetObject(entId, OpenMode.ForRead) as Entity;
                if (ent != null)
                {
                    BlockReference br = ent as BlockReference;
                    if (br != null)
                    {
                        BlockTableRecord bd = (BlockTableRecord)trx.GetObject(br.BlockTableRecord, OpenMode.ForRead);

                        // ... to see whether it's a block with the name we're after

                        if (bd.Name.ToUpper() == blockName.ToUpper())
                        {
                            // Check each of the attributes...

                            foreach (ObjectId arId in br.AttributeCollection)
                            {
                                DBObject obj = trx.GetObject(arId, OpenMode.ForRead);
                                AttributeReference ar = obj as AttributeReference;
                                if (ar != null)
                                {
                                    // ... to see whether it has
                                    // the tag we're after

                                    if (ar.Tag.ToUpper() == attributeTag.ToUpper())
                                    {
                                        // check if attribute has correct value
                                        if (ar.TextString == oldString)
                                        {
                                            // If so, update the value
                                            ar.UpgradeOpen();
                                            ar.TextString = newString;
                                            ar.DowngradeOpen();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
