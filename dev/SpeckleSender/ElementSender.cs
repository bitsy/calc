﻿using Autodesk.Revit.DB;
using Calc.Core;
using Calc.Core.Interfaces;
using Calc.Core.Objects.Assemblies;
using Speckle.Core.Api;
using Speckle.Core.Api.GraphQL.Inputs;
using Speckle.Core.Credentials;
using Speckle.Core.Kits;
using Speckle.Core.Transports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeckleSender
{
    public class ElementSender : IElementSender
    {
        private readonly ISpeckleConverter speckleConverter;
        private readonly string revitAppName;
        private readonly Client client;
        private readonly Account account;
        private readonly string builderProjectId;
        private readonly Document doc;

        public ElementSender(Document doc, CalcConfig config)
        {
            this.doc = doc;
            revitAppName = HostApplications.Revit.GetVersion(HostAppVersion.v2023);
            var speckleKit = KitManager.GetDefaultKit();

            // this should be loading the revit converter from nuget Speckle.Objects.Converter.Revit2023 (priotized?)
            // or from the installed speckle app kit.
            // see: https://speckle.community/t/how-to-run-revit-to-speckle-conversions-in-c/1548/6

            speckleConverter = speckleKit.LoadConverter(revitAppName);
            speckleConverter.SetContextDocument(doc);

            // make client
            account = new Account();
            account.token = config.SpeckleToken;
            account.serverInfo = new Speckle.Core.Api.GraphQL.Models.ServerInfo { url = config.SpeckleServerUrl };
            client = new Client(account);
            builderProjectId = config.SpeckleBuilderProjectId;
        }

        private AssemblyBase CreateAssemblyBase(AssemblyData assemblyData)
        {
            List<object> elementList = assemblyData.ElementIds
                .Select(id => doc.GetElement(new ElementId(id)))
                .ToList()
                .ConvertAll(e => e as Object);
            speckleConverter.SetContextDocument(new RevitDocumentAggregateCache(doc));
            var elementBases = speckleConverter.ConvertToSpeckle(elementList);
            var assemblyBase = new AssemblyBase(assemblyData, elementBases);
            foreach (var dProp in assemblyData.Properties)
            {
                assemblyBase[dProp.Key] = dProp.Value;
            }

            return assemblyBase;
        }


        /// <summary>
        /// send the elements to a speckle project with the model name, return the model id, which would be saved back to revit group
        /// the group shared parameters are wrapped as dynamic properties
        /// </summary>
        public async Task<string> SendAssembly(AssemblyData assemblyData)
        {
           var assemblyBase = CreateAssemblyBase(assemblyData);
            var transport = new ServerTransport(account, builderProjectId);
            var objectId = await Operations.Send(assemblyBase, transport, true);
            // ensure model exists
            // filter the models by model path (group + code) contains model code
            var filter = new ProjectModelsFilter(null,null,null,null,assemblyData.Code,new List<string> { "Calc Builder","Calc Builder Revit2023"}.AsReadOnly());
            var models = await client.Model.GetModels(builderProjectId, 10000, null, filter);

            // get the model with exactly the same code
            // get the model code with splitting '/' and get the last item
            var found = models.items.Where(m => m.name.Split('/').Last().ToLower() == assemblyData.Code.ToLower()).ToList();
            if (found.Count > 1)
            {
                throw new Exception($"Multiple models with code '{assemblyData.Code}' found");
            }
            var model = found.FirstOrDefault();
            if (model == null)
            {
                // create a new model
                model = await client.Model.Create(
                    new CreateModelInput(assemblyData.ModelPath, assemblyData.Description, builderProjectId)
                    );
            }
            else
            {
                // update the description if it doesn't match
                if(model.description != assemblyData.Description || model.name != assemblyData.ModelPath)
                {
                    model = await client.Model.Update(
                        new UpdateModelInput(model.id, assemblyData.ModelPath, assemblyData.Description, builderProjectId)
                        );
                }
            }

            string revitFilePath = doc.PathName;
            string revitUserName = doc.Application.Username;
            var commitId = await client.Version.Create(
                                  new CommitCreateInput
                                  {
                                      streamId = builderProjectId,
                                      branchName = assemblyData.ModelPath,
                                      objectId = objectId,
                                      message = $"[{revitUserName}]{revitFilePath}",
                                      sourceApplication = "Calc Builder"
                                  });
            return model.id;
        }
    }
}
