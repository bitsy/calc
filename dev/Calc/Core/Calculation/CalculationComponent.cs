﻿using Calc.Core.Color;
using Calc.Core.Objects.Assemblies;
using Calc.Core.Objects.Materials;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Calc.Core.Calculation
{
    /// <summary>
    /// The calculation of a material in the assembly, amount per assembly unit.
    /// </summary>
    public class CalculationComponent
    {
        [JsonProperty("position")]
        public int Position { get; set; }
        [JsonProperty("function")]
        public MaterialFunction Function { get; set; }
        [JsonProperty("amount")]
        public double Amount { get; set; }
        [JsonProperty("carbon_a1a3")]
        public double? Gwp { get; set; } // calculated gwp
        [JsonProperty("grey_energy_fabrication_total")]
        public double? Ge { get; set; } // calculated ge
        [JsonProperty("cost")]
        public double? Cost { get; set; }

        [JsonProperty("calc_materials_id")]
        public Material Material { get; set; }
        [JsonIgnore]
        public string ElementTypeId { get; set; }
        public HslColor HslColor { get; set; }
        public bool HasError { get; set; }
        public bool IsComplete { get => CheckComplete(); }

        public void LinkMaterial(List<Material> materials)
        {
            if (Material != null)
            {
                Material = materials.Find(m => m.Id == Material.Id);
            }
        }

        public static List<CalculationComponent> FromLayer(LayerComponent layer, double normalizeRatio)
        {
            var result = new List<CalculationComponent>();
   
            if (!layer.HasMainMaterial) return result;
            var mainCalculationComponent = FromLayerMaterial(layer, normalizeRatio, true);
            result.Add(mainCalculationComponent);

            if (!layer.HasSubMaterial) return result;
            var subCalculationComponent = FromLayerMaterial(layer, normalizeRatio, false);
            result.Add(subCalculationComponent);
            
            return result;
        }

       private static CalculationComponent FromLayerMaterial(LayerComponent layer, double normalizeRatio, bool getMain = true)
        {
            var layerAmountParam = layer.GetAmountParam();
            var layerAmount = layer.GetLayerAmount(normalizeRatio, getMain);

            var amortizationFactor = layer.GetAmortizationFactor();
            var calculatedGwp = layer.GetMaterialGwp(getMain) * layerAmount * amortizationFactor;
            var calculatedGe = layer.GetMaterialGe(getMain) * layerAmount * amortizationFactor;

            return new CalculationComponent
            {
                Material = getMain ? layer.MainMaterial : layer.SubMaterial,
                Function = layer.Function,
                Amount = Math.Round(layerAmount.Value, 5),
                HasError = layerAmountParam.HasError,
                Gwp = Math.Round(calculatedGwp.Value,5),
                Ge = Math.Round(calculatedGe.Value,5),
                ElementTypeId = layer.TypeIdentifier,
                HslColor = layer.HslColor
            };
        }

        public bool CheckComplete()
        {
            return Function != null && Amount > 0;
        }

        /// <summary>
        /// update the position number of the components
        /// </summary>
        /// <param name="components"></param>
        public static void UpdatePosition(List<CalculationComponent> components)
        {
            if (components == null) return;
            for (int i = 0; i < components.Count; i++)
            {
                components[i].Position = i + 1;
            }
        }

    }
}
