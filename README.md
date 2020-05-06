# Community.Commerce.Plugin.Hotfixes

This plugin is meant for the Sitecore Commerce Community.  
The goal is to collect as many hotfixes as possible that the community encountered during their implementations.  
The plugin is built to work with all >=9 versions by:

#### 1) Exposing a custom attribute [SitecoreCommerceVersion] to identify the version the plugin will be used with.
#### 2) Using that version attribute to select the right Target Framework:
```
<TargetFramework Condition="'$(SitecoreCommerceVersion)' == '9.0' OR '$(SitecoreCommerceVersion)' == '9.0.1' OR '$(SitecoreCommerceVersion)' == '9.0.2' OR '$(SitecoreCommerceVersion)' == '9.0.3'">net462</TargetFramework>
<TargetFramework Condition="'$(SitecoreCommerceVersion)' == '9.1' OR '$(SitecoreCommerceVersion)' == '9.2' OR '$(SitecoreCommerceVersion)' == '9.3'">net471</TargetFramework>

```
#### 3) Using that version attribute to select the right references:
```
 <ItemGroup Condition="'$(SitecoreCommerceVersion)' == '9.0'">
    <PackageReference Include="Sitecore.Commerce.Plugin.Catalog" Version="2.0.*" />
    <PackageReference Include="Sitecore.Commerce.Plugin.Availability" Version="2.0.*" />
    <PackageReference Include="Sitecore.Commerce.Plugin.Customers" Version="2.0.*" />
  </ItemGroup>

  <ItemGroup Condition="'$(SitecoreCommerceVersion)' == '9.0.1'">
    <PackageReference Include="Sitecore.Commerce.Plugin.Catalog" Version="2.1.*" />
    <PackageReference Include="Sitecore.Commerce.Plugin.Availability" Version="2.1.*" />
    <PackageReference Include="Sitecore.Commerce.Plugin.Customers" Version="2.1.*" />
  </ItemGroup>

  <ItemGroup Condition="'$(SitecoreCommerceVersion)' == '9.0.2'">
    <PackageReference Include="Sitecore.Commerce.Plugin.Catalog" Version="2.2.*" />
    <PackageReference Include="Sitecore.Commerce.Plugin.Availability" Version="2.2.*" />
    <PackageReference Include="Sitecore.Commerce.Plugin.Customers" Version="2.2.*" />
  </ItemGroup>

  <ItemGroup Condition="'$(SitecoreCommerceVersion)' == '9.0.3'">
    <PackageReference Include="Sitecore.Commerce.Plugin.Catalog" Version="2.4.*" />
    <PackageReference Include="Sitecore.Commerce.Plugin.Availability" Version="2.4.*" />
    <PackageReference Include="Sitecore.Commerce.Plugin.Customers" Version="2.4.*" />
  </ItemGroup>

  <ItemGroup Condition="'$(SitecoreCommerceVersion)' == '9.1'">
    <PackageReference Include="Sitecore.Commerce.Plugin.Catalog" Version="3.0*" />
    <PackageReference Include="Sitecore.Commerce.Plugin.Availability" Version="3.0.*" />
    <PackageReference Include="Sitecore.Commerce.Plugin.Customers" Version="3.0.*" />
  </ItemGroup>

  <ItemGroup Condition="'$(SitecoreCommerceVersion)' == '9.2'">
    <PackageReference Include="Sitecore.Commerce.Plugin.Catalog" Version="4.0*" />
    <PackageReference Include="Sitecore.Commerce.Plugin.Availability" Version="4.0.*" />
    <PackageReference Include="Sitecore.Commerce.Plugin.Customers" Version="4.0.*" />
  </ItemGroup>

  <ItemGroup Condition="'$(SitecoreCommerceVersion)' == '9.3'">
    <PackageReference Include="Sitecore.Commerce.Plugin.Catalog" Version="5.0.*" />
    <PackageReference Include="Sitecore.Commerce.Plugin.Availability" Version="5.0.*" />
    <PackageReference Include="Sitecore.Commerce.Plugin.Customers" Version="5.0.*" />
  </ItemGroup>
```


Here's how you can contribute:
##### 1) Add your fix (Pipeline/Block).

##### 2) Determine the target version(s) for your fix. 
For example a bug might be fixed by Sitecore in 9.2, so the fix added to this plugin would have to apply to all previous versions.

##### 3) Based on targeted version(s), register your new blocks/pipelines as follow (Try adding some comments to identify what issue is being fixed):

```
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
```

