namespace XCentium.Commerce.Plugin.Hotfixes.Pipelines.Blocks
{
    using System.Threading.Tasks;
    using Policies;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Customers;
    using Sitecore.Commerce.Plugin.ManagedLists;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    /// <summary>
    /// This Block is part of this fix:
    /// Customers Dashboard is very slow to load and it's proportional to the number of customers you have
    /// This was fixed in 9.2 but all previous versions need this fix.
    /// </summary>
    public class AddCustomerToRecentListBlock : PipelineBlock<Customer, Customer, CommercePipelineExecutionContext>
    {
        public override Task<Customer> Run(Customer arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull("The customer can not be null");
            Condition.Requires(arg.UserName).IsNotNullOrEmpty("The customer user name can not be null");

            var listMembershipComponent = arg.GetComponent<ListMembershipsComponent>();
            listMembershipComponent.Memberships?.Add(context.GetPolicy<FixKnownCustomersListsPolicy>().RecentCustomers);

            return Task.FromResult(arg);
        }
    }
}