namespace Community.Commerce.Plugin.Hotfixes
{
    using Microsoft.Extensions.DependencyInjection;
    using Pipelines.Blocks;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.Customers;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;
    using System.Reflection;

    public class ConfigureSitecore : IConfigureSitecore
    {

        public void ConfigureServices(IServiceCollection services)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            string sitecoreCommerceVersion = assembly
                .GetCustomAttribute<SitecoreCommerceVersionAttribute>()
                .SitecoreCommerceVersion.Replace("\"", "");
            // This section Fixes this issue:
            // Disassociating an item from a category removes the parent catalog from ParentCatalogList
            // Even if the item is still associated with other categories from that catalog.
            if (sitecoreCommerceVersion == "9.0.2" || sitecoreCommerceVersion == "9.0.1" || sitecoreCommerceVersion == "9.0")
            {
                services.Sitecore().Pipelines(config => config
                    .ConfigurePipeline<ICreateRelationshipPipeline>(configure => configure.Replace<UpdateCatalogHierarchyBlock, FixUpdateCatalogHierarchyBlock>())
                    .ConfigurePipeline<IDeleteRelationshipPipeline>(configure => configure.Replace<UpdateCatalogHierarchyBlock, FixUpdateCatalogHierarchyBlock>())
                );
            }

            // This section Fixes this issue:
            // Customers Dashboard is very slow to load and it's proportional to the number of customers you have
            // This was fixed in 9.2 but all previous versions need this fix.
            if (sitecoreCommerceVersion == "9.1" || sitecoreCommerceVersion == "9.0.2" || sitecoreCommerceVersion == "9.0.1" ||
                sitecoreCommerceVersion == "9.0")
            {
                services.Sitecore().Pipelines(config => config
                    .ConfigurePipeline<IGetEntityViewPipeline>(c =>
                    {
                        c.Replace<GetCustomersViewBlock, FixGetCustomersViewBlock>();
                    })
                   .ConfigurePipeline<ICreateCustomerPipeline>(configure =>
                    {
                        configure.Add<AddCustomerToRecentListBlock>().After<CreateCustomerBlock>();
                        configure.Add<RecentCustomersListKeepShortBlock>().After<PersistCustomerBlock>();
                    })
                    .ConfigurePipeline<IDeleteCustomerPipeline>(configure =>
                    {
                        configure.Add<RemoveCustomerFromRecentListBlock>().Before<DeleteCustomerBlock>();
                    }));
            }

            // This section Fixes this issue:
            // Customers Dashboard is very slow to load and it's proportional to the number of customers you have
            // This was fixed in 9.2 but all previous versions need this fix.
            if (sitecoreCommerceVersion == "9.1" || sitecoreCommerceVersion == "9.0.2" ||
                sitecoreCommerceVersion == "9.0.1" ||
                sitecoreCommerceVersion == "9.0")
            {
                services.Sitecore().Pipelines(config => config
                    .ConfigurePipeline<IPersistEntityPipeline>(configure =>
                        configure.Add<RemoveEntityFromMemoryCacheBlock>().Before<StoreEntityInMemoryCacheBlock>()));
            }
        }
    }
}
