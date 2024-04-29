﻿using Calc.MVVM.Helpers.Mediators;
using Calc.Core.Objects;
using Calc.Core.Objects.Buildups;
using Calc.Core.Objects.GraphNodes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Calc.MVVM.Models;
using Calc.Core;
using Calc.Core.Objects.Materials;
using Calc.Core.Objects.Results;

namespace Calc.MVVM.ViewModels
{

    public class MaterialSelectionViewModel : INotifyPropertyChanged
    {
        private readonly List<Material> allMaterials;
        public ICollectionView AllMaterialsView { get; set; }
        public List<FilterTagModel> MaterialTypeTags { get; set; }
        public List<FilterTagModel> ProductTypeTags { get; set; }
        private string currentSearchText;

        private Material selectedMaterial;
        public Material SelectedMaterial
        {
            get => selectedMaterial;
            set
            {
                if (value == selectedMaterial) return;
                selectedMaterial = value;
                OnPropertyChanged(nameof(SelectedMaterial));
            }
        }

        private FilterTagModel selectedMaterialTypeTag;
        public FilterTagModel SelectedMaterialTypeTag
        {
            get => selectedMaterialTypeTag;
            set
            {
                selectedMaterialTypeTag = value;
                FilterMaterialsWithType();
                UpdateDynamicCounts();
                OnPropertyChanged(nameof(SelectedMaterialTypeTag));
            }
        }

        private FilterTagModel selectedProductTypeTag;
        public FilterTagModel SelectedProductTypeTag
        {
            get => selectedProductTypeTag;
            set
            {
                selectedProductTypeTag = value;
                FilterMaterialsWithType();
                UpdateDynamicCounts();
                OnPropertyChanged(nameof(SelectedProductTypeTag));
            }
        }

        public MaterialSelectionViewModel(DirectusStore store)
        {
            allMaterials = store.CurrentMaterials;
            AllMaterialsView = CollectionViewSource.GetDefaultView(allMaterials);
            InitTypeTags();

        }

        public void Reset()
        {
            SelectedMaterialTypeTag = null;
            SelectedProductTypeTag = null;
            SelectedMaterial = null;
        }

        public void PrepareMaterialSelection(Material material)
        {
            currentSearchText = "";
            SelectedMaterialTypeTag = null;
            SelectedProductTypeTag = null;
            SelectedMaterial = material;
            // if a material was already selected, find the corresponding tags
            if (SelectedMaterial != null)
            {
                SelectedMaterialTypeTag = MaterialTypeTags.Find(t => t.Name == SelectedMaterial.MaterialType);
                SelectedProductTypeTag = ProductTypeTags.Find(t => t.Name == SelectedMaterial.ProductType);
            }
        }

        private void InitTypeTags()
        {
            MaterialTypeTags = new List<FilterTagModel>();
            ProductTypeTags = new List<FilterTagModel>();
            foreach (var material in allMaterials)
            {
                var mType = material.MaterialType;
                var pType = material.ProductType;
                if (!MaterialTypeTags.Exists(t => t.Name == mType))
                {
                    var family = material.MaterialTypeFamily;
                    MaterialTypeTags.Add(new FilterTagModel(mType, family));
                }
                if (!ProductTypeTags.Exists(t => t.Name == pType))
                {
                    var family = material.ProductTypeFamily;
                    ProductTypeTags.Add(new FilterTagModel(pType, family));
                }
                // add relation count to each other
                var mTag = MaterialTypeTags.Find(t => t.Name == mType);
                var pTag = ProductTypeTags.Find(t => t.Name == pType);
                mTag.AddRelationCount(pTag);
                pTag.AddRelationCount(mTag);
            }
        }

        private void FilterMaterialsWithType()
        {
            AllMaterialsView.Filter = (obj) =>
            {
                var material = obj as Material;
                var name = material.Name.ToLower();
                if (SelectedMaterialTypeTag != null && material.MaterialType != SelectedMaterialTypeTag.Name)
                {
                    return false;
                }
                if (SelectedProductTypeTag != null && material.ProductType != SelectedProductTypeTag.Name)
                {
                    return false;
                }
                if (!string.IsNullOrEmpty(currentSearchText) && !name.Contains(currentSearchText))
                {
                    return false;
                }
                return true;
            };
        }

        /// <summary>
        /// dynamic count is the count of each tag where the other type is selected
        /// if a tag of material type is selected, 
        /// the dynamic count of product type is the count of all materials with the selected material type.
        /// if a tag of the same type group is selected, all the other tags of the same type group will have null dynamic count
        /// the dynamic count is filtered with both selected tags
        /// </summary>
        private void UpdateDynamicCounts()
        {
            UpdateOtherGroupDynamicCounts(selectedProductTypeTag, MaterialTypeTags);
            UpdateOtherGroupDynamicCounts(selectedMaterialTypeTag, ProductTypeTags);
            UpdateSameGroupDynamicCounts(selectedMaterialTypeTag, MaterialTypeTags);
            UpdateSameGroupDynamicCounts(selectedProductTypeTag, ProductTypeTags);
        }

        private void UpdateOtherGroupDynamicCounts(FilterTagModel selectedTag, List<FilterTagModel> allTags)
        {
            foreach (var tag in allTags)
            {
                tag.UpdateDynamicCount(selectedTag);
            }
        }

        private void UpdateSameGroupDynamicCounts(FilterTagModel selectedTag, List<FilterTagModel> allTags)
        {
            foreach(var tag in allTags)
            {
                if (tag != selectedTag && selectedTag!=null)
                {
                    tag.CleanDynamicCount();
                }
            }
        }

        public void HandleSearchTextChanged(string currentText)
        {
            currentSearchText = currentText.ToLower();
            FilterMaterialsWithType();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
