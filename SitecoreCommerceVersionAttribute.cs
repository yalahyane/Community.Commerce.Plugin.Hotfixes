namespace Community.Commerce.Plugin.Hotfixes
{
    [System.AttributeUsage(System.AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    sealed class SitecoreCommerceVersionAttribute : System.Attribute
    {
        public string SitecoreCommerceVersion { get; }
        public SitecoreCommerceVersionAttribute(string version)
        {
            this.SitecoreCommerceVersion = version;
        }
    }
}