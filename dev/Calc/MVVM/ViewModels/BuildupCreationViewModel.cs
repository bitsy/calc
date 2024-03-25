﻿using Calc.Core;
using Calc.Core.Calculations;
using Calc.Core.Color;
using Calc.Core.Helpers;
using Calc.Core.Interfaces;
using Calc.Core.Objects;
using Calc.Core.Objects.Buildups;
using Calc.Core.Objects.Materials;
using Calc.MVVM.Helpers.Mediators;
using Calc.MVVM.Models;
using Speckle.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Calc.MVVM.ViewModels
{

    public class BuildupCreationViewModel : INotifyPropertyChanged
    {
        private DirectusStore store;
        public List<LcaStandard> StandardsAll { get => store.StandardsAll; }
        public List<Unit> BuildupUnitsAll { get => store.UnitsAll; }
        public List<MaterialFunction> MaterialFunctionsAll { get => store.MaterialFunctionsAll; }
        public List<BuildupGroup> BuildupGroupsAll { get => store.BuildupGroupsAll; }

        public BasicAmountsModel AmountsModel { get; set; } = new BasicAmountsModel();
        private List<Material> CurrentMaterials { get => store.CurrentMaterials; }

        private readonly IBuildupComponentCreator buildupComponentCreator;
        public bool MaterialSelectionEnabled { get => SelectedComponent is LayerComponent; }
        public HslColor CurrentColor { get => SelectedComponent?.HslColor?? ItemPainter.DefaultColor; }

        private List<BuildupComponent> buildupComponents = new List<BuildupComponent>();
        public List<BuildupComponent> BuildupComponents
        {
            get => buildupComponents;
            set
            {
                if (buildupComponents == value) return;
                buildupComponents = value;
                OnPropertyChanged(nameof(BuildupComponents));
            }
        }

        public List<CalculationComponent> AllCalculationComponents
        {
            get
            {
                var calcs = new List<CalculationComponent>();
                foreach (var component in BuildupComponents)
                {
                    calcs.AddRange(component.GetCalculationComponents());
                }
                CalculationComponent.UpdatePosition(calcs);
                return calcs;
            }
        }

        private Dictionary<LayerComponent,  LayerMaterialModel> layerMaterialModels = new Dictionary<LayerComponent, LayerMaterialModel>();
        private LayerMaterialModel InvalidLayerMaterialModel { get => new LayerMaterialModel(); }
        public LayerMaterialModel CurrentLayerMaterialModel
        {
            get
            {
                if (layerMaterialModels == null) return null;
                if (SelectedComponent is LayerComponent layerComponent)
                {
                  return layerMaterialModels.TryGetValue(layerComponent, out var layerMaterialModel) ? layerMaterialModel : InvalidLayerMaterialModel;
                }
                return InvalidLayerMaterialModel;
            }
        }

        private string newBuildupName;
        public string NewBuildupName
        {
            get => newBuildupName;
            set
            {
                if (newBuildupName == value) return;
                newBuildupName = value;
                OnPropertyChanged(nameof(NewBuildupName));
            }
        }

        private string newBuildupDescription;
        public string NewBuildupDescription
        {
            get => newBuildupDescription;
            set
            {
                if (newBuildupDescription == value) return;
                newBuildupDescription = value;
                OnPropertyChanged(nameof(NewBuildupDescription));
            }
        }

        private ICalcComponent selectedComponent;
        public ICalcComponent SelectedComponent
        {
            get => selectedComponent;
            set
            {
                if (selectedComponent == value) return;
                selectedComponent = value;
                OnPropertyChanged(nameof(SelectedComponent));
                OnPropertyChanged(nameof(CurrentLayerMaterialModel));
                UpdateAmounts();
            }
        }

        public LcaStandard SelectedStandard
        {
            get => store.StandardSelected;
            set
            {
                if (store.StandardSelected == value) return;
                store.StandardSelected = value;
                UpdateLayerMaterialModels();
                OnPropertyChanged(nameof(SelectedStandard));
                UpdateCalculationComponents();
                UpdateLayerColors();

            }
        }

        private Unit? selectedBuildupUnit;
        public Unit? SelectedBuildupUnit
        {
            get => selectedBuildupUnit;
            set
            {
                if (selectedBuildupUnit == value) return;
                selectedBuildupUnit = value;
                OnPropertyChanged(nameof(SelectedBuildupUnit));
                UpdateCalculationComponents();
                UpdateAmounts();
            }
        }

        private BuildupGroup selectedBuildupGroup;
        public BuildupGroup SelectedBuildupGroup
        {
            get => selectedBuildupGroup;
            set
            {
                if (selectedBuildupGroup == value) return;
                selectedBuildupGroup = value;
                OnPropertyChanged(nameof(SelectedBuildupGroup));
            }
        }

        public double? BuildupGwp { get => AllCalculationComponents?.Where(c => c.Amount.HasValue).Sum(c => c.Gwp); }
        public double? BuildupGe { get => AllCalculationComponents?.Where(c => c.Amount.HasValue).Sum(c => c.Ge); }


        public BuildupCreationViewModel(DirectusStore store, IBuildupComponentCreator bcCreator)
        {
            this.store = store;
            buildupComponentCreator = bcCreator;
        }

        public void HandleLoaded()
        {
            OnPropertyChanged(nameof(StandardsAll));
            OnPropertyChanged(nameof(SelectedStandard));
            OnPropertyChanged(nameof(BuildupUnitsAll));
            OnPropertyChanged(nameof(BuildupGroupsAll));
        }

        public void HandleDeselect()
        {
            SelectedComponent = null;
            MediatorToView.Broadcast("ViewDeselectTreeView");
            OnPropertyChanged(nameof(CurrentColor));
        }

        public void HandleAmountClicked(Unit? newBuildupUnit)
        {
          if(newBuildupUnit == null) return;
           SetCurrentNormalizer(newBuildupUnit.Value);
        }

        /// <summary>
        /// updates the basic unit amounts of the current selection
        /// </summary>
        private void UpdateAmounts()
        {
            var materialUnit = CurrentLayerMaterialModel?.MainMaterial?.MaterialUnit;
            AmountsModel.UpdateAmounts(SelectedComponent, SelectedBuildupUnit, materialUnit);
        }

        /// <summary>
        /// selecting elements from revit
        /// </summary>
        public void HandleSelectingElements()
        {
            BuildupComponents = buildupComponentCreator.CreateBuildupComponentsFromSelection();
            UpdateLayerMaterialModels();
            UpdateCalculationComponents();
            UpdateLayerColors();
        }

        /// <summary>
        /// component selection from treeview changed
        /// </summary>
        public void HandleComponentSelectionChanged(ICalcComponent selectedCompo)
        {
            SelectedComponent = selectedCompo;
            // set the main material tab to active
            CurrentLayerMaterialModel.ResetActiveMaterial();
            OnPropertyChanged(nameof(CurrentColor));
        }

        public void HandleReduceMaterial()
        {
            CurrentLayerMaterialModel.RemoveMaterial();
            UpdateAmounts();
        }

        private void UpdateLayerMaterialModels()
        {
            layerMaterialModels?.Clear();
            foreach (var component in BuildupComponents.Where(c => c.HasLayers))
            {
                foreach (var layer in component.LayerComponents)
                {
                    var layerMaterialModel = new LayerMaterialModel(layer, CurrentMaterials, MaterialFunctionsAll);
                    layerMaterialModel.MaterialPropertyChanged += HandleMaterialChanged;
                    layerMaterialModels.Add(layer, layerMaterialModel);
                }
            }
            OnPropertyChanged(nameof(CurrentLayerMaterialModel));
            CurrentLayerMaterialModel.NotifyPropertiesChange();
            UpdateAmounts();
        }

        // on material changed
        
        private void HandleMaterialChanged(object sender, EventArgs e)
        {
            if (sender is LayerMaterialModel changedModel)
            {
                UpdateMaterialModelSettings(changedModel);
            }
            UpdateCalculationComponents();
            UpdateLayerColors();
            UpdateAmounts();
        }

        /// <summary>
        /// set the same material setting to models with the same target material
        /// </summary>
        private void UpdateMaterialModelSettings(LayerMaterialModel changedModel)
        {
            foreach (var model in layerMaterialModels.Values)
            {
                if (model.CheckIdenticalTargetMaterial(changedModel))
                {
                    model.LearnMaterialSetting(changedModel);
                }
            }
        }

        private void UpdateLayerColors()
        {
            ItemPainter.ColorLayersByMaterial(BuildupComponents);
            OnPropertyChanged(nameof(CurrentColor));
        }

        public void UpdateCalculationComponents()
        {
            var quantityRatio = GetQuantityRatio();

            foreach (var component in BuildupComponents)
            {
                component.UpdateCalculationComponents(quantityRatio);
            }
            OnPropertyChanged(nameof(AllCalculationComponents));
            OnPropertyChanged(nameof(BuildupGwp));
            OnPropertyChanged(nameof(BuildupGe));
        }

        private void SetCurrentNormalizer(Unit unit) // temporary only for testing
        {
            foreach (var component in BuildupComponents)
            {
                component.IsNormalizer = false;
            }

            if (SelectedComponent is BuildupComponent bc)
            {
                bc.IsNormalizer = true;
                SelectedBuildupUnit = unit;
            }
        }

        private double GetQuantityRatio()
        {
            var normalizer = BuildupComponents.Where(c => c.IsNormalizer).ToList();
            if (normalizer.Count != 1) return 0;
            if (SelectedBuildupUnit == null) return 0;
            var value = normalizer[0].BasicParameterSet.GetAmountParam((Unit)SelectedBuildupUnit).Amount;
            if (value.HasValue)
            {
                return 1 / value.Value;
            }
            return 0;
        }

        private Buildup CreateBuildup()
        {
            var buildup = new Buildup
            {
                Name = NewBuildupName,
                Description = NewBuildupDescription,
                Standard = SelectedStandard,
                Group = SelectedBuildupGroup,
                BuildupUnit = (Unit)SelectedBuildupUnit,
                CalculationComponents = AllCalculationComponents,
                BuildupGwp = BuildupGwp??0,
                BuildupGe = BuildupGe??0
            };
            return buildup;
        }

        public async Task<bool> HandleSaveBuildup()
        {
            var buildup = CreateBuildup();
            bool response = await store.SaveSingleBuildup(buildup);
            return response;

        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
