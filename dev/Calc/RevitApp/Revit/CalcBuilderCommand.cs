﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Calc.Core;
using Calc.Core.DirectusAPI;
using Calc.Core.Objects.Buildups;
using Calc.MVVM.Services;
using Calc.MVVM.ViewModels;
using Calc.MVVM.Views;
using Calc.RevitConnector.Revit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Calc.RevitApp.Revit
{
    [Transaction(TransactionMode.Manual)]
    public class CalcBuilderCommand : IExternalCommand
    {
        private Directus directusInstance;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                App.RevitVersion = commandData.Application.Application.VersionNumber;
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                UIDocument uidoc = commandData.Application.ActiveUIDocument;

                Logger.Log("Now authenticating.");
                Task.Run(() => Authenticate()).Wait();
                if (directusInstance == null)
                {
                    Logger.Log("Failed to get directus.");
                    return Result.Cancelled;
                }
                Logger.Log("Authentication successful.");

                DirectusStore store = new DirectusStore(directusInstance);
                BuildupComponentCreator componentCreator = new BuildupComponentCreator(uidoc);

                //List<BuildupComponent> components = componentCreator.CreateBuildupComponentsFromSelection();
                //TaskDialog.Show("Components", components.Count.ToString());

                BuilderViewModel builderViewModel = new BuilderViewModel(store, componentCreator);
                BuilderView builderView = new BuilderView(builderViewModel);

                builderView.Show();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                Debug.WriteLine(ex.Message);
                return Result.Failed;
            }
        }

        private async Task Authenticate()
        {
            var authenticator = new DirectusAuthenticator();
            directusInstance = await authenticator.ShowLoginWindowAsync().ConfigureAwait(false);
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // depending on the build mode, use different paths
            string assemblyFolder = @"C:\source\calc\bin";
            //string assemblyFolder = $"C:\\ProgramData\\Autodesk\\Revit\\Addins\\{App.RevitVersion}\\CalcRevit"; // Specify the directory where your DLLs are located
            string assemblyName = new AssemblyName(args.Name).Name;
            string assemblyPath = Path.Combine(assemblyFolder, assemblyName + ".dll");

            if (File.Exists(assemblyPath))
                return Assembly.LoadFrom(assemblyPath);

            return null;
        }
    }
}