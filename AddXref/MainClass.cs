﻿using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using XrefManager.Forms;
using XrefManager.Workers;
using LayerConfigEditor.Workers;
using XrefManager.Functions;

namespace XrefManager
{
    public class MainClass
    {
        MyRibbonTab.Commands rb = new MyRibbonTab.Commands();

        public MainClass()
        {
        }

        [CommandMethod("CVIEW", CommandFlags.Session)]
        public void OneVportTopDown()
        {
            var EV = new EditView();
            EV.SetTopView();
        }

        [CommandMethod("Ribb", CommandFlags.Session)]
        public void Ribb()
        {
            rb.RibbonSplitButton();
        }

        [CommandMethod("EditLayerConfig", CommandFlags.Session)]
        public void EditLayerConfig()
        {
            var openConfigTool = new OpenConfigTool();
            openConfigTool.openConfigTool();
        }

        [CommandMethod("AddLayerThisDrawing", CommandFlags.Session)]
        public void AddLayerThisDrawing()
        {
            var getLayerFromObject = new GetLayerFromObject();
            getLayerFromObject.SelectObjectReturnLayer();
        }

        [CommandMethod("AddLayerXref", CommandFlags.Session)]
        public void AddLayerXref()
        {
            var getLayerFromObject = new GetLayerFromObject();
            getLayerFromObject.SelectXrefObjectReturnLayer();
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
            UX.detachXref();
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
            var reader = new ConfigFileReader();
            var propertyList = reader.readConfigFile(prConfigPath.StringResult);

            var AX = new AddXref();
            var LU = new LayerUpdate();
            var drawingList = AX.addXrefSpecialized();
            if (drawingList != null)
            {
                LU.ChangeLayersOnDrawings(propertyList, drawingList);
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
        public void LayerUpdateThisDrawing()
        {
            var LU = new LayerUpdate();
            LU.UpdateLayersThisDrawing();
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

        [CommandMethod("ChangeAttributeDialog", CommandFlags.Session)]
        public void ChangeAttribute_dialog()
        {
            var RV = new ReplaceValue();

            RV.openDialogeBox();
        }

        [CommandMethod("MOVEXREF", CommandFlags.Session)]
        public void moveToXrefLayer()
        {
            var MTXL = new moveToXreflayer();
            MTXL.moveXref();
        }

        [CommandMethod("ChangeBlockColorByLayer", CommandFlags.Session)]
        public void ChangeBlockColorByLayer()
        {
            var changeBlockColorByLayer = new ChangeBlockColorByLayer();
            changeBlockColorByLayer.ChangeColorsBlocksAllDrawings();
        }

        [CommandMethod("GetAllAttributes", CommandFlags.Session)]
        public void GetAllAttributes()
        {
            var GetAllAtt = new GetAllAttributesFromBlockInFiles();
            GetAllAtt.getAllAttributes();
        }

        [CommandMethod("GetAllAttributes", CommandFlags.Session)]
        public void AdjustCableTrays()
        {
            var ACT = new AdjustCableTrays();
            ACT.AdjustCableTrays_bottom();
        }
    }

}
