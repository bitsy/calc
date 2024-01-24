﻿using Calc.ConnectorRevit.Helpers;
using Calc.ConnectorRevit.Helpers.Mediators;
using Calc.Core;
using Calc.Core.Objects.GraphNodes;
using Calc.Core.Objects.Mappings;
using System.ComponentModel;

namespace Calc.ConnectorRevit.ViewModels
{
    public class ForestViewModel : INotifyPropertyChanged
    {
        private readonly DirectusStore store;        
        public ForestViewModel(DirectusStore directusStore)
        {
            store = directusStore;
        }
        public void HandleForestSelectionChanged(Forest forest)
        {
            if (forest != null)
            {
                Mapping mapping;
                if (store.ForestSelected == forest)
                {
                    // if the same forest is selected, update the current forest presering the mapping
                    Mapping currentMapping = new Mapping("CurrentMapping", store.ForestSelected);
                    ForestHelper.PlantTrees(store.ForestSelected);
                    mapping = currentMapping;
                }
                else
                {
                    // otherwise create new forest and reset mapping
                    store.ForestSelected = forest;
                    ForestHelper.PlantTrees(forest);
                    mapping = store.MappingSelected;
                }
                MediatorFromVM.Broadcast("ForestSelectionChanged", mapping);
            }
        }

        public void HandleUpdateCurrentForest(Forest forest)
        {
            if (forest != null)
            {
                Mapping currentMapping = new Mapping("CurrentMapping", store.ForestSelected);
                ForestHelper.PlantTrees(store.ForestSelected);
                store.MappingSelected = currentMapping;
                MediatorFromVM.Broadcast("ForestSelectionChanged");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


