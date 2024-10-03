﻿using Calc.Core.Objects;
using Calc.Core.Objects.Assemblies;
using Calc.Core.Objects.Elements;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calc.Core.Interfaces
{
    public interface IElementCreator
    {
        List<CalcElement> CreateCalcElements(List<CustomParamSetting> customParamSettings, List<string> parameterNameList);
    }


    public interface IElementSourceHandler
    {
        public ElementSourceSelectionResult SelectElements(List<CustomParamSetting> customParamSettings);
        public void SaveAssemblyRecord(string assemblyCode, string assemblyName, Unit newAssemblyUnit, AssemblyGroup assemblyGroup, string description, List<AssemblyComponent> components);
        public AssemblyRecord GetAssemblyRecord();
    }

    public interface IImageSnapshotCreator
    {
        public string CreateImageSnapshot(string baseName);
    }

    /// <summary>
    /// send elements to speckle
    /// </summary>
    public interface IElementSender
    {
        public Task<string> SendAssembly(AssemblyData assemblyData);
    }
    

    public interface IVisualizer
    {
        void ResetView(List<IGraphNode> nodes);
        void IsolateAndColorBottomBranchElements(IGraphNode node);
        void IsolateAndColorSubbranchElements(IGraphNode node);
        
    }
}
