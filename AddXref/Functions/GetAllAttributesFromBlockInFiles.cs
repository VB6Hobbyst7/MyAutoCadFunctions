﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrefManager.Models;

namespace XrefManager.Functions
{
    public class GetAllAttributesFromBlockInFiles
    {
        private List<BlockData> blockDataList = new List<BlockData>();
        private string blockName = "";
        private string outputFileLocation = "";

        public void GetAllAttributes()
        {
            blockName = GetBlockName();

            if (string.IsNullOrEmpty(blockName))
            {
                return;
            }

            var drawingList = SelectDrawings();
            if (drawingList.Count == 0)
            {
                return;
            }

            outputFileLocation = SelectOutputFileLocation();

            if (string.IsNullOrEmpty(outputFileLocation))
            {
                return;
            }

            GetAttributeList(drawingList);
            WriteBlockDataListToFile();
        }

        private string GetBlockName()
        {
            var blockName = "";

            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Request for objects to be selected in the drawing area

                var opt = new PromptSelectionOptions()
                {
                    SingleOnly = true
                };
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection(opt);

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;

                    // Step through the objects in the selection set
                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        Entity ent = acTrans.GetObject(acSSObj.ObjectId, OpenMode.ForRead) as Entity;
                        if (ent is BlockReference br)
                        {
                            blockName = br.Name;
                        }

                        // Save the new object to the database
                        acTrans.Dispose();
                    }

                    // Dispose of the transaction
                }
            }
            return blockName;
        }

        private List<string> SelectDrawings()
        {
            var FD = new OpenFileDialog()
            {
                Filter = "AutoCAD Files | *.dwg",
                Multiselect = true,
                Title = "Select drawings to export data from"
            };
            FD.ShowDialog();

            return FD.FileNames.ToList();
        }

        private string SelectOutputFileLocation()
        {
            var diag = new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = "Text File | *.txt",
                Filter = "Text File | *.txt",
                Title = "Select location to save output file"
            };
            diag.ShowDialog();
            return diag.FileName;
        }

        private void GetAttributeList(List<string> drawingList)
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
                                    ChangeAttributeValuePaperSpace(trx, openDoc.Database, drawing);
                                    ChangeAttributeValueModelSpace(trx, openDoc.Database, drawing);
                                    trx.Dispose();
                                }
                            }
                        }
                    }
                }
                else
                {
                    Database xrefDb = new Database(false, true);

                    xrefDb.ReadDwgFile(drawing, FileShare.Read, false, "");

                    try
                    {
                        using (Transaction trx = xrefDb.TransactionManager.StartTransaction())
                        {
                            ChangeAttributeValuePaperSpace(trx, xrefDb, drawing);
                            ChangeAttributeValueModelSpace(trx, xrefDb, drawing);
                            trx.Dispose();
                        }
                    }
                    catch (Exception) { }
                    xrefDb.CloseInput(true);
                }
            }
        }

        private void ChangeAttributeValuePaperSpace(Transaction trx, Database db, string drawingName)
        {
            var _blockData = new BlockData()
            {
                BlockName = Path.GetFileNameWithoutExtension(drawingName),
                AttNameAndvalue = new List<AttName_Value>()
            };
            ObjectId psId;
            BlockTable bt = (BlockTable)trx.GetObject(db.BlockTableId, OpenMode.ForRead);
            psId = bt[BlockTableRecord.PaperSpace];

            BlockTableRecord btr = (BlockTableRecord)trx.GetObject(psId, OpenMode.ForRead);

            foreach (var entId in btr)
            {
                if (trx.GetObject(entId, OpenMode.ForRead) is Entity ent)
                {
                    if (ent is BlockReference br)
                    {
                        BlockTableRecord bd = (BlockTableRecord)trx.GetObject(br.BlockTableRecord, OpenMode.ForRead);

                        // ... to see whether it's a block with the name we're after

                        if (bd.Name.ToUpper() == blockName.ToUpper())
                        {
                            // Check each of the attributes...

                            foreach (ObjectId arId in br.AttributeCollection)
                            {
                                DBObject obj = trx.GetObject(arId, OpenMode.ForRead);
                                if (obj is AttributeReference ar)
                                {
                                    // ... to see whether it has
                                    // the tag we're after
                                    _blockData.AttNameAndvalue.Add(new AttName_Value { attName = ar.Tag, attValue = ar.TextString });


                                }
                            }
                            blockDataList.Add(_blockData);
                        }
                    }
                }
            }
        }

        private void ChangeAttributeValueModelSpace(Transaction trx, Database db, string drawingName)
        {
            var _blockData = new BlockData()
            {
                BlockName = Path.GetFileNameWithoutExtension(drawingName)
            };
            ObjectId psId;
            BlockTable bt = (BlockTable)trx.GetObject(db.BlockTableId, OpenMode.ForRead);
            psId = bt[BlockTableRecord.ModelSpace];

            BlockTableRecord btr = (BlockTableRecord)trx.GetObject(psId, OpenMode.ForRead);

            foreach (var entId in btr)
            {
                if (trx.GetObject(entId, OpenMode.ForRead) is Entity ent)
                {
                    if (ent is BlockReference br)
                    {
                        BlockTableRecord bd = (BlockTableRecord)trx.GetObject(br.BlockTableRecord, OpenMode.ForRead);

                        // ... to see whether it's a block with the name we're after

                        if (bd.Name.ToUpper() == blockName.ToUpper())
                        {
                            // Check each of the attributes...

                            foreach (ObjectId arId in br.AttributeCollection)
                            {
                                DBObject obj = trx.GetObject(arId, OpenMode.ForRead);
                                if (obj is AttributeReference ar)
                                {
                                    // ... to see whether it has
                                    // the tag we're after
                                    _blockData.AttNameAndvalue.Add(new AttName_Value { attName = ar.Tag, attValue = ar.TextString });

                                }
                            }
                            blockDataList.Add(_blockData);
                        }
                    }
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


        private void WriteBlockDataListToFile()
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(outputFileLocation);



            foreach (var drawing in blockDataList)
            {
                var line = "";
                foreach (var blockattLine in drawing.AttNameAndvalue)
                {
                    line = line + blockattLine.attName + "\t";
                }
                file.WriteLine("Drawing Name\t" + line);
                break;
            }

           
            foreach (var drawing in blockDataList)
            {
                var line = "";
                foreach (var blockattLine in drawing.AttNameAndvalue)
                {
                    line = line + blockattLine.attValue + "\t";
                }
                file.WriteLine(drawing.BlockName + "/t" + line);
            }


            file.Close();
        }
    }
}
