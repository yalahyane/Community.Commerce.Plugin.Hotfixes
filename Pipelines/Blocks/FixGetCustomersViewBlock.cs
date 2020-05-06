namespace Community.Commerce.Plugin.Hotfixes.Pipelines.Blocks
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Policies;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Customers;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// This Block is part of this fix:
    /// Customers Dashboard is very slow to load and it's proportional to the number of customers you have
    /// This was fixed in 9.2 but all previous versions need this fix.
    /// </summary>
    public class FixGetCustomersViewBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly IFindEntitiesInListPipeline _findEntitiesInListPipeline;

        public FixGetCustomersViewBlock(IFindEntitiesInListPipeline findEntitiesInListPipeline)
        {
            this._findEntitiesInListPipeline = findEntitiesInListPipeline;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            var entityViewArgument = context.CommerceContext.GetObjects<EntityViewArgument>().FirstOrDefault();
            if (string.IsNullOrEmpty(entityViewArgument?.ViewName) || !entityViewArgument.ViewName.Equals(context.GetPolicy<KnownCustomerViewsPolicy>().Customers, StringComparison.OrdinalIgnoreCase) && !entityViewArgument.ViewName.Equals(context.GetPolicy<KnownCustomerViewsPolicy>().CustomersDashboard, StringComparison.OrdinalIgnoreCase))
                return entityView;
            EntityView customersView;
            if (entityViewArgument.ViewName.Equals(context.GetPolicy<KnownCustomerViewsPolicy>().CustomersDashboard, StringComparison.OrdinalIgnoreCase))
            {
                var entityView1 = new EntityView
                {
                    EntityId = string.Empty,
                    ItemId = string.Empty,
                    Name = context.GetPolicy<KnownCustomerViewsPolicy>().Customers,
                    DisplayRank = 100
                };
                customersView = entityView1;
                entityView.ChildViews.Add(customersView);
            }
            else
                customersView = entityView;
            customersView.UiHint = "Table";
            foreach (var customer in (await this._findEntitiesInListPipeline.Run(new FindEntitiesInListArgument(typeof(Customer), context.GetPolicy<FixKnownCustomersListsPolicy>().RecentCustomers, 0, int.MaxValue), context).ConfigureAwait(false)).List.Items.Cast<Customer>().OrderByDescending(c => c.DateCreated))
            {
                var entityView1 = new EntityView
                {
                    EntityId = customer.Id,
                    ItemId = customer.Id,
                    DisplayName = customer.DisplayName,
                    Name = context.GetPolicy<KnownCustomerViewsPolicy>().Summary
                };
                var entityView2 = entityView1;
                var properties1 = entityView2.Properties;
                var viewProperty1 = new ViewProperty
                {
                    Name = "ItemId",
                    RawValue = customer.Id,
                    IsHidden = true,
                    IsReadOnly = true
                };
                properties1.Add(viewProperty1);
                var properties2 = entityView2.Properties;
                var viewProperty2 = new ViewProperty
                {
                    Name = "Email",
                    RawValue = customer.Email,
                    IsReadOnly = true,
                    UiType = "EntityLink"
                };
                properties2.Add(viewProperty2);
                var properties3 = entityView2.Properties;
                var viewProperty3 = new ViewProperty
                {
                    Name = "UserName",
                    RawValue = customer.UserName,
                    IsReadOnly = true,
                    UiType = "EntityLink"
                };
                properties3.Add(viewProperty3);
                var properties4 = entityView2.Properties;
                var viewProperty4 = new ViewProperty
                {
                    Name = "Status",
                    RawValue = customer.AccountStatus,
                    IsReadOnly = true
                };
                properties4.Add(viewProperty4);
                var properties5 = entityView2.Properties;
                var viewProperty5 = new ViewProperty
                {
                    Name = "DateCreated",
                    RawValue = customer.DateCreated,
                    IsReadOnly = true
                };
                properties5.Add(viewProperty5);
                var properties6 = entityView2.Properties;
                var viewProperty6 = new ViewProperty
                {
                    Name = "DateUpdated",
                    RawValue = customer.DateUpdated,
                    IsReadOnly = true
                };
                properties6.Add(viewProperty6);
                customersView.ChildViews.Add(entityView2);
            }
            return entityView;
        }
    }
}
