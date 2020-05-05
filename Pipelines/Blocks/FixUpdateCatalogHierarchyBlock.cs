namespace XCentium.Commerce.Plugin.Hotfixes.Pipelines.Blocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// This Block Fixes this issue:
    /// Disassociating an item from a category removes the parent catalog from ParentCatalogList
    /// Even if the item is still associated with other categories from that catalog.
    /// </summary>
    public class FixUpdateCatalogHierarchyBlock : PipelineBlock<RelationshipArgument, RelationshipArgument, CommercePipelineExecutionContext>
    {
        private readonly IFindEntityPipeline _findEntityPipeline;
        private readonly IPersistEntityPipeline _persistEntityPipeline;
        private readonly IGetCategoriesPipeline _getCategoriesPipeline;

        public FixUpdateCatalogHierarchyBlock(IFindEntityPipeline findEntityPipeline, IPersistEntityPipeline persistEntityPipeline, IGetCategoriesPipeline getCategoriesPipeline)
        {
            this._findEntityPipeline = findEntityPipeline;
            this._persistEntityPipeline = persistEntityPipeline;
            this._getCategoriesPipeline = getCategoriesPipeline;
        }

        public override async Task<RelationshipArgument> Run(RelationshipArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");
            Condition.Requires(arg.TargetName).IsNotNullOrEmpty($"{this.Name}: The target name can not be null or empty");
            Condition.Requires(arg.SourceName).IsNotNullOrEmpty($"{this.Name}: The source name can not be null or empty");
            Condition.Requires(arg.RelationshipType).IsNotNullOrEmpty($"{this.Name}: The relationship type can not be null or empty");
            if (!new[]
            {
                "CatalogToCategory",
                "CatalogToSellableItem",
                "CategoryToCategory",
                "CategoryToSellableItem"
            }.Contains(arg.RelationshipType, StringComparer.OrdinalIgnoreCase))
                return arg;
            var source = await this._findEntityPipeline.Run(new FindEntityArgument(typeof(CatalogItemBase), arg.SourceName), context) as CatalogItemBase;
            var stringList = new List<string>();
            if (arg.TargetName.Contains("|"))
            {
                var strArray = arg.TargetName.Split('|');
                stringList.AddRange(strArray);
            }
            else
                stringList.Add(arg.TargetName);
            var sourceChanged = new ValueWrapper<bool>(false);
            foreach (var entityId in stringList)
            {
                var catalogItemBase = await this._findEntityPipeline.Run(new FindEntityArgument(typeof(CatalogItemBase), entityId), context) as CatalogItemBase;
                if (source != null && catalogItemBase != null)
                {
                    var changed = new ValueWrapper<bool>(false);
                    if (arg.RelationshipType.Equals("CatalogToCategory", StringComparison.OrdinalIgnoreCase))
                    {
                        source.ChildrenCategoryList = this.UpdateHierarchy(arg, catalogItemBase.SitecoreId, source.ChildrenCategoryList, sourceChanged);
                        catalogItemBase.ParentCatalogList = this.UpdateHierarchy(arg, source.SitecoreId, catalogItemBase.ParentCatalogList, changed);
                        catalogItemBase.CatalogToEntityList = this.UpdateHierarchy(arg, source.SitecoreId, catalogItemBase.CatalogToEntityList, changed);
                    }
                    else if (arg.RelationshipType.Equals("CategoryToCategory", StringComparison.OrdinalIgnoreCase))
                    {
                        source.ChildrenCategoryList = this.UpdateHierarchy(arg, catalogItemBase.SitecoreId, source.ChildrenCategoryList, sourceChanged);
                        catalogItemBase.ParentCategoryList = this.UpdateHierarchy(arg, source.SitecoreId, catalogItemBase.ParentCategoryList, changed);
                        catalogItemBase.ParentCatalogList = this.UpdateHierarchy(arg, ExtractCatalogId(source.Id), catalogItemBase.ParentCatalogList, changed);
                    }
                    else if (arg.RelationshipType.Equals("CatalogToSellableItem", StringComparison.OrdinalIgnoreCase))
                    {
                        source.ChildrenSellableItemList = this.UpdateHierarchy(arg, catalogItemBase.SitecoreId, source.ChildrenSellableItemList, sourceChanged);
                        catalogItemBase.ParentCatalogList = this.UpdateHierarchy(arg, source.SitecoreId, catalogItemBase.ParentCatalogList, changed);
                        catalogItemBase.CatalogToEntityList = this.UpdateHierarchy(arg, source.SitecoreId, catalogItemBase.CatalogToEntityList, changed);
                    }
                    else if (arg.RelationshipType.Equals("CategoryToSellableItem", StringComparison.OrdinalIgnoreCase))
                    {
                        source.ChildrenSellableItemList = this.UpdateHierarchy(arg, catalogItemBase.SitecoreId, source.ChildrenSellableItemList, sourceChanged);
                        catalogItemBase.ParentCategoryList = this.UpdateHierarchy(arg, source.SitecoreId, catalogItemBase.ParentCategoryList, changed);
                        if (arg.Mode.GetValueOrDefault() == RelationshipMode.Delete)
                        {
                            var allCategories = await this._getCategoriesPipeline.Run(new GetCategoriesArgument(" "), context);
                            var category = allCategories.FirstOrDefault(cat => cat.SitecoreId.Equals(source.SitecoreId, StringComparison.OrdinalIgnoreCase));
                            if (category != null && !catalogItemBase.ParentCatalogList.Contains(category.ParentCatalogList))
                            {
                                catalogItemBase.ParentCatalogList = this.UpdateHierarchy(arg, ExtractCatalogId(source.Id), catalogItemBase.ParentCatalogList, changed);
                            }
                        }
                        else
                        {
                            catalogItemBase.ParentCatalogList = this.UpdateHierarchy(arg, ExtractCatalogId(source.Id), catalogItemBase.ParentCatalogList, changed);
                        }
                    }
                    if (changed.Value)
                    {
                        await this._persistEntityPipeline.Run(new PersistEntityArgument(catalogItemBase), context);
                    }
                }
            }
            if (sourceChanged.Value)
            {
                await this._persistEntityPipeline.Run(new PersistEntityArgument(source), context);
            }
            return arg;
        }

        private static string ExtractCatalogId(string id)
        {
            var strArray = id.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            return strArray.Length < 3 ? string.Empty : GuidUtils.GetDeterministicGuidString($"{CommerceEntity.IdPrefix<Catalog>()}{strArray[2]}");
        }

        private string UpdateHierarchy(RelationshipArgument arg, string targetId, string rawChildren, ValueWrapper<bool> changed)
        {
            if (rawChildren == null)
                rawChildren = string.Empty;
            var list = rawChildren.Split(new[]
            {
                '|'
            }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if ((arg.Mode.GetValueOrDefault() == RelationshipMode.Create ? (arg.Mode.HasValue ? 1 : 0) : 0) != 0 && !list.Contains(targetId))
            {
                if (!changed.Value)
                    changed.Value = true;
                list.RemoveAll(c => c.Equals(targetId, StringComparison.OrdinalIgnoreCase));
                list.Add(targetId);
            }
            else
            {
                if ((arg.Mode.GetValueOrDefault() == RelationshipMode.Delete ? (arg.Mode.HasValue ? 1 : 0) : 0) != 0 && list.Contains(targetId))
                {
                    if (!changed.Value)
                        changed.Value = true;
                    list.RemoveAll(c => c.Equals(targetId, StringComparison.OrdinalIgnoreCase));
                }
            }
            return string.Join("|", list);
        }
    }
}
