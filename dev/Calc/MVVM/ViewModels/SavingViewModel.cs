﻿using Calc.MVVM.Helpers.Mediators;
using Calc.MVVM.Models;
using Calc.MVVM.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Calc.MVVM.ViewModels
{

    public class SavingViewModel : INotifyPropertyChanged
    {
        private readonly NodeTreeModel nodetreeVM;
        private readonly CalculationViewModel calculationVM;
        public double ElementCount { get; set; }


        public SavingViewModel(CalculationViewModel calVM)
        
        {
            calculationVM = calVM;
        }

        public void HandleSavingResults()
        {
            int count = calculationVM.Results?.Count??0;
            string message;
            if (count>100)
            {
                message = $"{count} elements to save,\nthis may take a while.";
            }
            else
            {
                message = $"{count} elements to save.";
            }
            MediatorToView.Broadcast("ShowSavingOverlay", message);
        }


        public async Task HandleSendingResults(string newName)
        {
            
            MediatorToView.Broadcast("ShowWaitingOverlay", "Saving results...");
            ResultSender resultSender = new ResultSender();
            bool? feedback =  await resultSender.SaveResults(calculationVM.Store,calculationVM.Results,newName);
            MediatorToView.Broadcast("ShowMainView");
            MediatorToView.Broadcast
                ("ShowMessageOverlay",
                new List<object> 
                    {   feedback,
                        "No element is saved",
                        "Saved results successfully.",
                        "Error occured while saving." 
                    }
                 );
        }

        public void HandleSaveResultCanceled()
        {
            MediatorToView.Broadcast("ShowMainView");
        }

        private NodeModel GetNodeToCalculate()
        {
            NodeModel CurrentForestItem = nodetreeVM.CurrentForestItem;
            NodeModel SelectedNodeItem = nodetreeVM.SelectedNodeItem;
            if (CurrentForestItem == null) return null;
            NodeModel nodeToCalculate = SelectedNodeItem ?? CurrentForestItem;
            return nodeToCalculate;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
