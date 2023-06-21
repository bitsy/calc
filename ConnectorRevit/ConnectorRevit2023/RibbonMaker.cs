﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace Calc.ConnectorRevit
{
    public class RibbonMaker
    {
        private readonly string PanelName = "LCA";

        public void Create(UIControlledApplication uiCtrlApp)
        {
            RibbonPanel panel = uiCtrlApp.GetRibbonPanels()
                ?.FirstOrDefault(p => p.Name == this.PanelName);

            if (panel == null)
            {
                panel = uiCtrlApp.CreateRibbonPanel(this.PanelName);
            }

            PushButtonData buttonData = new PushButtonData(
                "LCAButton",
                this.PanelName,
                Assembly.GetExecutingAssembly().Location,
                "RevitLca.LcaCommand");

            PushButton button = panel.AddItem(buttonData) as PushButton;
            Uri uriImage = new Uri("pack://application:,,,/RevitLca;component/Resources/icon.png");
            button.LargeImage = new BitmapImage(uriImage);
        }
    }


}