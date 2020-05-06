namespace Community.Commerce.Plugin.Hotfixes.Pipelines.Blocks
{
    using System.Collections.Generic;
    using System.Linq;
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
    public class RecentCustomersListKeepShortBlock : PipelineBlock<Customer, Customer, CommercePipelineExecutionContext>
    {
        private readonly IFindEntitiesInListPipeline _findEntitiesInListPipeline;
        private readonly IRemoveListEntitiesPipeline _removeListEntitiesPipeline;

        public RecentCustomersListKeepShortBlock(IFindEntitiesInListPipeline findEntitiesInListPipeline, IRemoveListEntitiesPipeline removeListEntitiesPipeline)
        {
            this._findEntitiesInListPipeline = findEntitiesInListPipeline;
            this._removeListEntitiesPipeline = removeListEntitiesPipeline;
        }

        public override async Task<Customer> Run(Customer arg, CommercePipelineExecutionContext context)
        {
            var entitiesInListArgument = await this._findEntitiesInListPipeline.Run(new FindEntitiesInListArgument(typeof(Customer), context.GetPolicy<FixKnownCustomersListsPolicy>().RecentCustomers, 0, int.MaxValue), context).ConfigureAwait(false);
            var count = entitiesInListArgument.List.Items.Count;
            if (count <= 10)
                return arg;
            foreach (var customer in entitiesInListArgument.List.Items.Cast<Customer>().OrderByDescending(c => c.DateCreated).Reverse().Take(count - 10))
            {
                await this._removeListEntitiesPipeline.Run(new ListEntitiesArgument(new List<string>
                {
                    customer.Id
                }, context.GetPolicy<FixKnownCustomersListsPolicy>().RecentCustomers), context).ConfigureAwait(false);
            }
            return arg;
        }
    }
}
