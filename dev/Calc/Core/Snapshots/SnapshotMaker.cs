﻿using Calc.Core.Calculation;
using Calc.Core.Helpers;
using Calc.Core.Objects;
using Calc.Core.Objects.BasicParameters;
using Calc.Core.Objects.Buildups;
using Calc.Core.Objects.Elements;
using Calc.Core.Objects.GraphNodes;
using Calc.Core.Objects.Results;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Calc.Core.Snapshots
{
    /// <summary>
    /// make the flat snapshots for a branch (each element) or for a new buildup (each element type)
    /// only merge the snapshots before sending to directus
    /// </summary>
    public class SnapshotMaker
    {
        /// <summary>
        /// generate the snapshots for a **dead end** branch, 
        /// </summary>
        public static void Snap(Branch branch)
        {

            if (branch == null) return;
            if (branch.SubBranches.Count == 0) return;

            var resultList = new List<BuildupSnapshot>();

            var buildupList = branch.Buildups ?? new();

            foreach (var element in branch.Elements)
            {
                foreach (var buildup in buildupList)
                {               
                    var snapshots = GetBuildupSnapshot(buildup, element);
                        resultList.AddRange(snapshots);                    
                }
            }

            branch.BuildupSnapshots = resultList;
        }

        /// <summary>
        /// make the snapshots for a branch, claim the element to the snapshots
        /// </summary>
        private static List<BuildupSnapshot> GetBuildupSnapshot(Buildup buildup, CalcElement element)
        {
            var snapshots = new List<BuildupSnapshot>();
            foreach (var component in buildup.CalculationComponents)
            {
                var snapshot = GetBuildupSnapshot(component, buildup);
                snapshot.ClaimElement(element);
                snapshots.Add(snapshot);
            }
            return snapshots;
        }

        /// <summary>
        /// get the buildup snapshot from a single calculation component
        /// </summary>
        private static BuildupSnapshot GetBuildupSnapshot(CalculationComponent component, Buildup buildup)
        {
            var calculationComponents = buildup.CalculationComponents;

            var material = component.Material;
            var materialSnapshot = new MaterialSnapshot
            {
                MaterialFunction = component.Function.Name,
                MaterialSourceUuid = material.SourceUuid,
                MaterialSource = material.DataSource,
                MaterialName = material.Name,
                MaterialUnit = material.MaterialUnit,
                MaterialAmount = component.Amount,
                MaterialGwp = material.Gwp,
                MaterialGe = material.Ge,
                Gwp = component.Gwp,
                Ge = component.Ge
            };

            var snapshot = new BuildupSnapshot
            {
                BuildupName = buildup.Name,
                BuildupCode = buildup.Code,
                BuildupGroup = buildup.Group.Name,
                BuildupUnit = buildup.BuildupUnit,
                MaterialSnapshots = new List<MaterialSnapshot> { materialSnapshot }
            };

            return snapshot;
        }



        private static BuildupSnapshot GetResult(string treeName, CalcElement element, Buildup buildup, CalculationComponent component, double amount)
        {
            var material = component.Material;
            var gwp = component.Gwp * amount;
            var ge = component.Ge * amount;

            var calculationResult = new BuildupSnapshot
            {
                Tree = treeName,

                ElementId = element.Id,
                ElementType = element.TypeName,
                ElementUnit = buildup.BuildupUnit,
                ElementAmount = amount,

                BuildupName = buildup.Name,
                BuildupCode = buildup.Code,
                GroupName = buildup.Group?.Name,
                BuildupUnit = buildup.BuildupUnit,

                MaterialName = material.Name,
                MaterialUnit = component.Material.MaterialUnit,
                MaterialAmount = component.Amount ?? 0,
                MaterialStandard = material.Standard.Name,
                MaterialSource = material.DataSource,
                MaterialSourceUuid = material.SourceUuid,
                MaterialFunction = component.Function.Name,
                MaterialGwp = material.Gwp ?? 0,
                MaterialGe = material.Ge ?? 0,

                Gwp = gwp ?? 0,
                Ge = ge ?? 0,
            };
            return calculationResult;
        }

        /// <summary>
        /// get the layer result from a buildup component
        /// </summary>
        public static List<LayerResult> GetResult(double totalRatio, BuildupComponent buildupComponent, Unit buildupUnit, string buildupName, string buildupCode, string buildupGroup)
        {
            var result = new List<LayerResult>();
            var layerComponents = buildupComponent.LayerComponents;
            if (layerComponents == null) return result;
            foreach (var layerComponent in layerComponents)
            {
                var calculationComponents = layerComponent.CalculationComponents;
                if (calculationComponents == null) continue;
                foreach (var component in calculationComponents)
                {
                    var calculationResult = new LayerResult
                    {
                        ElementTypeId = buildupComponent.TypeIdentifier.ToString(),
                        ElementType = $"{buildupComponent.Title} : {layerComponent.Title}",
                        ElementUnit = component.Material.MaterialUnit,
                        ElementAmount = (double)layerComponent.GetLayerAmount(totalRatio), // the layer amount is normalized to the total ratio

                        BuildupName = buildupName,
                        BuildupCode = buildupCode,
                        GroupName = buildupGroup,
                        BuildupUnit = buildupUnit,

                        MaterialName = component.Material.Name,
                        MaterialUnit = component.Material.MaterialUnit,
                        MaterialAmount = component.Amount ?? 0,
                        MaterialStandard = component.Material.Standard.Name,
                        MaterialSource = component.Material.DataSource,
                        MaterialSourceUuid = component.Material.SourceUuid,
                        MaterialFunction = component.Function.Name,
                        MaterialGwp = component.Material.Gwp ?? 0,
                        MaterialGe = component.Material.Ge ?? 0,

                        Gwp = component.Gwp ?? 0,
                        Ge = component.Ge ?? 0
                    };
                    result.Add(calculationResult);
                }
            }

            return result;

        }




    }
}
