﻿using Speckle.Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Calc.Core.Color;

namespace Calc.Core.Objects
{
    public class Forest : IHasProject, IGraphNode
    {
        [JsonIgnore]
        public List<CalcElement> Elements { get => GetElements(); }
        [JsonIgnore]
        public List<string> ElementIds => Elements.Select(e => e.Id).ToList();
        [JsonIgnore]
        public List<Branch> SubBranches => Trees.ConvertAll(tree => (Branch)tree);
        [JsonIgnore]
        public HslColor HslColor { get; set; } = new HslColor(0, 0, 85);
        [JsonProperty("id")]
        public int Id { get; set; } = -1;
        [JsonProperty("forest_name")]
        public string Name { get; set; }
        [JsonIgnore]
        private List<Tree> _trees;
        [JsonProperty("trees")] // for receiving the tree JSON from the API
        public List<Tree> Trees
        {
            get => _trees;
            set
            {
                _trees = value;
                if (_trees != null)
                {
                    foreach (var tree in _trees)
                    {
                        tree.ParentForest = this;
                    }
                }
            }
        }
        [JsonProperty("project_id")]
        public Project Project { get; set; }

        public void SetBranchColorsBy(string method)
        {
            List<Branch> branches = this.Trees.ConvertAll(tree => (Branch)tree);

            switch (method)
            {
                case "branches":
                    BranchPainter.ColorBranchesByBranch(branches);
                    break;
                case "buildups":
                    BranchPainter.ColorBranchesByBuildup(branches);
                    break;
                default:
                    break;
            }
        }

        public HashSet<string> GetAllParameters()
        {
            var parameters = new HashSet<string>();
            foreach (Tree tree in Trees)
            {
                parameters.UnionWith(tree.FilterConfig.GetAllParameters());
                parameters.UnionWith(tree.BranchConfig);
            }
            return parameters;
        }

        /// <summary>
        /// Plants all trees in the forest and returns the remaining elements
        /// </summary>
        public List<CalcElement> PlantTrees(List<CalcElement> searchElements)
        {
            foreach (Tree tree in Trees)
            {
                searchElements = tree.Plant(searchElements);
            }
            return searchElements;
        }

        public string SerializeTrees()
        {
            var treesJson = new StringBuilder();
            treesJson.Append($"[{string.Join(",", Trees.Select(t => t.Serialize()))}]");
            return treesJson.ToString();
        }

        private List<CalcElement> GetElements()
        {
            List<CalcElement> elements = new();

            foreach (Tree tree in Trees)
            {
                elements.AddRange(tree.Elements);
            }

            return elements;
        }
    }
}
