# Community.Commerce.Plugin.Hotfixes

This plugin is meant for the Sitecore Commerce Community.  
The goal is to collect as many hotfixes as possible that the community encountered during their implementations.  
The plugin is built to work with all >=9 versions by:

#### 1) Exposing a custom attribute [SitecoreCommerceVersion] to identify the version the plugin will be used with.
#### 2) Using that version attribute to select the right Target Framework:




Here's how you can contribute:
##### 1) Add your fix (Pipeline/Block).

##### 2) Determine the target version(s) for your fix. 
For example a bug might be fixed by Sitecore in 9.2, so the fix added to this plugin would have to apply to all previous versions.

##### 3) Based on targeted version(s), register your new blocks/pipelines as follow (Try adding some comments to identify what issue is being fixed):



