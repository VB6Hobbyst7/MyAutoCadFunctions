﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Microsoft.Win32;
using System.IO;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;

namespace XrefManager
{
    public class MainClass
    {
        public MainClass()
        {
        }

        [CommandMethod("AddXref", CommandFlags.Session)]
        public void AddXref()
        {
            var AX = new AddXref();
            AX.addXref();
        }

        [CommandMethod("UnloadXref", CommandFlags.Session)]
        public void UnloadXRef()
        {
            var UX = new UnloadXref();
            UX.unloadXref();
        }

        [CommandMethod("DetachXref", CommandFlags.Session)]
        public void DetachXRef()
        {
            var UX = new UnloadXref();
            UX.detechXref();
        }

        [CommandMethod("AddXrefSpecialized", CommandFlags.Session)]
        public void AddXrefSpecialized()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptResult prConfigPath = ed.GetString("\nEnter path to config file: ");
            if (prConfigPath.Status != PromptStatus.OK)
            {
                ed.WriteMessage("No string was provided\n");
                return;
            }
            var LU = new LayerUpdate();
            var propertyList = LU.ReadLayerPropertyFile(prConfigPath.StringResult);

            var AX = new AddXref();
            var drawingList = AX.addXrefSpecialized();
            if (drawingList != null)
            {
                LU.ChangeLayersOnDrawing(propertyList, drawingList);
            }
        }

        [CommandMethod("PURGEDRAWINGS", CommandFlags.Session)]
        public void PurgeDrawings()
        {
            var PD = new PurgeDrawings();
            PD.purgeMultipleFiles();
        }


        [CommandMethod("LAYERUPDATE", CommandFlags.Session)]
        public void LayerUpdate()
        {
            var LU = new LayerUpdate();
            LU.UpdateLayers();
        }

        [CommandMethod("-LAYERUPDATETHISDRAWING", CommandFlags.Session)]
        public void LayerUpdateWithArgs()
        {
            var LU = new LayerUpdate();

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptResult prConfigPath = ed.GetString("\nEnter path to config file: ");
            if (prConfigPath.Status != PromptStatus.OK)
            {
                ed.WriteMessage("No string was provided\n");
                return;
            }

            LU.UpdateLayersThisDrawing(prConfigPath.StringResult);
        }

        [CommandMethod("ChangeAttribute", CommandFlags.Session)]
        public void ChangeAttribute()
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptStringOptions blocknameOptions = new PromptStringOptions("\nEnter Blockname");
            blocknameOptions.AllowSpaces = true;

            PromptStringOptions attributeTagOptions = new PromptStringOptions("\nEnter attribute name");
            attributeTagOptions.AllowSpaces = true;

            PromptStringOptions oldStringOptions = new PromptStringOptions("\nEnter old attribute value");
            oldStringOptions.AllowSpaces = true;

            PromptStringOptions newStringOptions = new PromptStringOptions("\nEnter new attribute value");
            newStringOptions.AllowSpaces = true;



            PromptResult blockname = ed.GetString(blocknameOptions);
            if (blockname.Status != PromptStatus.OK)
            {
                ed.WriteMessage("No string was provided\n");
                return;
            }

            PromptResult attributeTag = ed.GetString(attributeTagOptions);
            if (attributeTag.Status != PromptStatus.OK)
            {
                ed.WriteMessage("No string was provided\n");
                return;
            }

            PromptResult oldString = ed.GetString(oldStringOptions);
            if (oldString.Status != PromptStatus.OK)
            {
                ed.WriteMessage("No string was provided\n");
                return;
            }

            PromptResult newString = ed.GetString(newStringOptions);
            if (newString.Status != PromptStatus.OK)
            {
                ed.WriteMessage("No string was provided\n");
                return;
            }

            var UX = new UnloadXref();

            var drawingList = UX.getDrawingList();
            if (drawingList.Count == 0 || drawingList == null)
            {
                return;
            }

            var RV = new ReplaceValue();

            RV.ReplaceStringValue(drawingList, blockname.StringResult, attributeTag.StringResult, oldString.StringResult, newString.StringResult);

        }
        [CommandMethod("MOVEXREF", CommandFlags.Session)]
        public void moveToXrefLayer()
        {
            var MTXL = new moveToXreflayer();
            MTXL.moveXref();
        }
    }
}
