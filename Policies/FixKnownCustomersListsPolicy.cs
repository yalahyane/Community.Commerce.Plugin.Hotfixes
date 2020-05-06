namespace Community.Commerce.Plugin.Hotfixes.Policies
{
    using Sitecore.Commerce.Core;

    public class FixKnownCustomersListsPolicy : Policy
    {
        public FixKnownCustomersListsPolicy()
        {
            this.RecentCustomers = nameof(RecentCustomers);
        }

        public string RecentCustomers { get; set; }
    }
}
