﻿using Calc.Core.Objects;
using Calc.Core.Objects.Buildups;
using Calc.Core.Objects.Elements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Calc.Core.Interfaces
{
    public interface IElementCreator
    {
        List<CalcElement> CreateCalcElements(List<CustomParamSetting> customParamSettings, List<string> parameterNameList);
    }

    public interface IBuildupComponentCreator
    {
        public List<BuildupComponent> CreateBuildupComponentsFromSelection(List<CustomParamSetting> customParamSettings);
    }

    public interface IImageSnapshotCreator
    {
        public string CreateImageSnapshot(string baseName);
    }
    

    public interface IVisualizer
    {
        void ResetView(List<IGraphNode> nodes);
        void IsolateAndColorBottomBranchElements(IGraphNode node);
        void IsolateAndColorSubbranchElements(IGraphNode node);
        
    }
}
