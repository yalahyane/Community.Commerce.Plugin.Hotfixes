namespace Community.Commerce.Plugin.Hotfixes.Pipelines.Blocks
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Policies;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Customers;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// This Block is part of this fix:
    /// Customers Dashboard is very slow to load and it's proportional to the number of customers you have
    /// This was fixed in 9.2 but all previous versions need this fix.
    /// </summary>
    public class RemoveCustomerFromRecentListBlock : PipelineBlock<Customer, Customer, CommercePipelineExecutionContext>
    {
        private readonly IRemoveListEntitiesPipeline _removeListEntitiesPipeline;

        public RemoveCustomerFromRecentListBlock(IRemoveListEntitiesPipeline removeListEntitiesPipeline)
        {
            this._removeListEntitiesPipeline = removeListEntitiesPipeline;
        }

        public override async Task<Customer> Run(Customer arg, CommercePipelineExecutionContext context)
        {
            await this._removeListEntitiesPipeline.Run(new ListEntitiesArgument(new List<string>
            {
                arg.Id
            }, context.GetPolicy<FixKnownCustomersListsPolicy>().RecentCustomers), context).ConfigureAwait(false);
            return arg;
        }
    }
}
