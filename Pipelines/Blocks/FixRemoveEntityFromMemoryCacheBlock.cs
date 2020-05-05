namespace XCentium.Commerce.Plugin.Hotfixes.Pipelines.Blocks
{
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Caching;
    using Sitecore.Framework.Pipelines;
    using System;
    using System.Linq;
    using System.Threading.Tasks;


    /// <summary>
    /// This block fixes this issue:
    /// After making changes to a Product or a Variant it takes a long time for the changes to display on the live site.
    /// Removing items from the cache to be reloaded after Persist fixes this issue.
    /// </summary>
    public class FixRemoveEntityFromMemoryCacheBlock : PipelineBlock<PersistEntityArgument, PersistEntityArgument, CommercePipelineExecutionContext>
    {
        private readonly IGetEnvironmentCachePipeline _cachePipeline;

        public FixRemoveEntityFromMemoryCacheBlock(IGetEnvironmentCachePipeline cachePipeline)
        {
            _cachePipeline = cachePipeline;
        }

        public override async Task<PersistEntityArgument> Run(PersistEntityArgument arg, CommercePipelineExecutionContext context)
        {
            var itemKey = string.IsNullOrEmpty(arg.Entity.CompositeKey) ? $"{ arg.Entity.Id}" : $"{ arg.Entity.CompositeKey}";
            var model = context.GetModel((Func<CacheRequest, bool>)(x => x.EntityId.Equals(arg.Entity.Id, StringComparison.OrdinalIgnoreCase)));
            if (model?.Version != null)
            {
                itemKey += $"-{model.Version}";
            }

            context.RemoveModel(model);

            EntityMemoryCachingPolicy memoryCachingPolicy1 = null;
            if (!string.IsNullOrEmpty(arg.Entity?.GetType().FullName))
            {
                memoryCachingPolicy1 = context.CommerceContext.Environment.Policies.OfType<EntityMemoryCachingPolicy>().FirstOrDefault(p => p.EntityFullName.Equals(arg.Entity.GetType().FullName, StringComparison.OrdinalIgnoreCase));
            }

            var memoryCachingPolicy2 = memoryCachingPolicy1 ?? context.CommerceContext.Environment.Policies.OfType<EntityMemoryCachingPolicy>().FirstOrDefault(p => p.EntityFullName.Equals("*", StringComparison.OrdinalIgnoreCase));
            if (memoryCachingPolicy2 != null)
            {
                var cachePipeline = _cachePipeline;
                var environmentCacheArgument = new EnvironmentCacheArgument
                {
                    CacheName = memoryCachingPolicy2.CacheName
                };
                var pipelineContextOptions = context.CommerceContext.GetPipelineContextOptions();
                await (await cachePipeline.Run(environmentCacheArgument, pipelineContextOptions)).Remove(itemKey);
                context.Logger.LogDebug($"Core.MemCache.Remove: {itemKey}", Array.Empty<object>());
            }
            return arg;
        }
    }
}
