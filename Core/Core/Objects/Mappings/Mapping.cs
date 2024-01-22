﻿using Calc.Core.Objects.Buildups;
using Calc.Core.Objects.GraphNodes;
using Speckle.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calc.Core.Objects.Mappings
{
    public class Mapping : IHasProject
    {
        [JsonProperty("id")]
        public int Id { get; set; } = -1;
        [JsonProperty("mapping_name")]
        public string Name { get; set; }
        [JsonProperty("project_id")]
        public Project Project { get; set; }
        [JsonProperty("mappings")]
        public List<MappingItem> MappingItems { get; set; }

        public Mapping()
        {
            // empty constructor for deserialization
        }

        public Mapping(Forest forest, string mappingName)
        {
            MappingItems = new List<MappingItem>();
            foreach (var tree in forest.Trees)
            {
                MappingItems.AddRange(ExtractMappingItems(tree.Flatten(), tree.Name));
            }
            Name = mappingName;
        }

        /// <summary>
        /// Assigns the buildups to the tree based on the mapping.
        /// Automatically determines which mapping to use based on the tree name.
        /// Example Use:
        /// <code>
        /// var buildups = await BuildupStorageDriver.GetAllBuildups();
        /// var mappings = await MappingStorageDriver.GetMappings("my mapping name");
        /// foreach (var tree in trees)
        /// {
        ///     MappingStorageDriver.ApplyMappingToTree(tree, mappings.First(), buildups);
        /// }
        /// </code>
        /// </summary>
        /// <param name="tree">The tree to assign buildups to.</param>
        /// <param name="allBuildups">The full list of buildups from the database.</param>
        /// <returns></returns>
        public Tree ApplyToTree(Tree tree, List<Buildup> allBuildups, int maxBuildups)
        {
            var brokenTree = new Tree()
            {
                Name = tree.Name,
            };

            tree.ResetBuildups();
            // find the mapping items that apply to this tree
            var mappingItems = MappingItems.Where(mappingItem => mappingItem.TreeName == tree.Name);
            if (!mappingItems.Any())
            {
                return null;
            }

            foreach (var mappingItem in mappingItems)
            {
                var buildupIds = mappingItem.BuildupIds.Take(maxBuildups).ToList();
                // find the buildups from this mapping item
                var buildups = allBuildups.Where(b => buildupIds.Contains(b.Id)).ToList();
                var match = MapBuildupsToBranch(tree, buildups, mappingItem.Path);

                if (!match)
                {
                    brokenTree.AddBranchWithMappingItem(mappingItem, buildups);
                }
            }

            return brokenTree.SubBranches.Count > 0 ? brokenTree : null;
        }

        private bool MapBuildupsToBranch(Branch branch, List<Buildup> buildups, List<MappingPath> path)
        {

            if (branch.Path.SequenceEqual(path))
            {
                branch.SetBuildups(buildups);
                return true;
            }
            else
            {
                if (branch.SubBranches.Count == 0)
                {
                    return false;
                }
                foreach (var subBranch in branch.SubBranches)
                {
                    bool subMatch = MapBuildupsToBranch(subBranch, buildups, path);
                    if (subMatch)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public string SerializeMappingItems()
        {
            var mappingsJson = new StringBuilder();
            mappingsJson.Append($"[{string.Join(",", MappingItems.Select(t => t.Serialize()))}]");
            return mappingsJson.ToString();
        }

        public Mapping Copy(string newName)
        {
            return new Mapping()
            {
                Name = newName,
                Project = Project,
                MappingItems = MappingItems
            };
        }

        private static List<MappingItem> ExtractMappingItems(List<Branch> branches, string treeName)
        {
            var mappingItems = new List<MappingItem>();
            foreach (var branch in branches)
            {
                if (branch.Buildups != null && branch.Buildups.Count > 0)
                {
                    mappingItems.Add(new MappingItem()
                    {
                        BuildupIds = branch.Buildups.Select(b => b.Id).ToList(),
                        Path = branch.Path,
                        TreeName = treeName
                    });
                }
            }
            return mappingItems;
        }
    }
}
