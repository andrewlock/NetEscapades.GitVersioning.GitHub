// Adapted from https://github.com/AArnott/Nerdbank.GitVersioning/blob/9880338a60/src/NerdBank.GitVersioning/VersionOptionsContractResolver.cs

using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NetEscapades.GitVersioning.GitHub
{
    internal class VersionOptionsContractResolver : CamelCasePropertyNamesContractResolver
    {
        private static readonly object TypeContractCacheLock = new object();

        private static readonly Dictionary<Tuple<bool, bool, Type>, JsonContract> contractCache = new Dictionary<Tuple<bool, bool, Type>, JsonContract>();

        public VersionOptionsContractResolver()
        {
        }

        internal bool IncludeSchemaProperty { get; set; }

        internal bool IncludeDefaults { get; set; } = true;

        /// <summary>
        /// Obtains a contract for a given type.
        /// </summary>
        /// <param name="type">The type to obtain a contract for.</param>
        /// <returns>The contract.</returns>
        /// <remarks>
        /// This override changes the caching policy from the base class, which caches based on this.GetType().
        /// The inherited policy is problematic because we have instance properties that change the contract.
        /// So instead, we cache with a complex key to capture the settings as well.
        /// </remarks>
        public override JsonContract ResolveContract(Type type)
        {
            var contractKey = Tuple.Create(this.IncludeSchemaProperty, this.IncludeDefaults, type);

            JsonContract contract;
            lock (TypeContractCacheLock)
            {
                if (contractCache.TryGetValue(contractKey, out contract))
                {
                    return contract;
                }
            }

            contract = base.CreateContract(type);

            lock (TypeContractCacheLock)
            {
                if (!contractCache.ContainsKey(contractKey))
                {
                    contractCache.Add(contractKey, contract);
                }
            }

            return contract;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(VersionOptions) && member.Name == nameof(VersionOptions.Schema))
            {
                property.ShouldSerialize = instance => this.IncludeSchemaProperty;
            }

            if (!this.IncludeDefaults)
            {
                if (property.DeclaringType == typeof(VersionOptions) && member.Name == nameof(VersionOptions.AssemblyVersion))
                {
                    property.ShouldSerialize = instance => !((VersionOptions)instance).AssemblyVersionOrDefault.IsDefault;
                }

#pragma warning disable CS0618 // Type or member is obsolete
                if (property.DeclaringType == typeof(VersionOptions) && member.Name == nameof(VersionOptions.BuildNumberOffset))
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    property.ShouldSerialize = instance => false; // always serialized by its new name
                }

                if (property.DeclaringType == typeof(VersionOptions) && member.Name == nameof(VersionOptions.VersionHeightOffset))
                {
                    property.ShouldSerialize = instance => ((VersionOptions)instance).VersionHeightOffsetOrDefault != 0;
                }

                if (property.DeclaringType == typeof(VersionOptions) && member.Name == nameof(VersionOptions.NuGetPackageVersion))
                {
                    property.ShouldSerialize = instance => !((VersionOptions)instance).NuGetPackageVersionOrDefault.IsDefault;
                }

                if (property.DeclaringType == typeof(VersionOptions) && member.Name == nameof(VersionOptions.CloudBuild))
                {
                    property.ShouldSerialize = instance => !((VersionOptions)instance).CloudBuildOrDefault.IsDefault;
                }

                if (property.DeclaringType == typeof(VersionOptions.CloudBuildOptions) && member.Name == nameof(VersionOptions.CloudBuildOptions.SetAllVariables))
                {
                    property.ShouldSerialize = instance => ((VersionOptions.CloudBuildOptions)instance).SetAllVariablesOrDefault != VersionOptions.CloudBuildOptions.DefaultInstance.SetAllVariables.Value;
                }

                if (property.DeclaringType == typeof(VersionOptions.CloudBuildOptions) && member.Name == nameof(VersionOptions.CloudBuildOptions.SetVersionVariables))
                {
                    property.ShouldSerialize = instance => ((VersionOptions.CloudBuildOptions)instance).SetVersionVariablesOrDefault != VersionOptions.CloudBuildOptions.DefaultInstance.SetVersionVariables.Value;
                }

                if (property.DeclaringType == typeof(VersionOptions.CloudBuildNumberOptions) && member.Name == nameof(VersionOptions.CloudBuildNumberOptions.IncludeCommitId))
                {
                    property.ShouldSerialize = instance => !((VersionOptions.CloudBuildNumberOptions)instance).IncludeCommitIdOrDefault.IsDefault;
                }

                if (property.DeclaringType == typeof(VersionOptions.CloudBuildNumberCommitIdOptions) && member.Name == nameof(VersionOptions.CloudBuildNumberCommitIdOptions.When))
                {
                    property.ShouldSerialize = instance => ((VersionOptions.CloudBuildNumberCommitIdOptions)instance).WhenOrDefault != VersionOptions.CloudBuildNumberCommitIdOptions.DefaultInstance.When.Value;
                }

                if (property.DeclaringType == typeof(VersionOptions.CloudBuildNumberCommitIdOptions) && member.Name == nameof(VersionOptions.CloudBuildNumberCommitIdOptions.Where))
                {
                    property.ShouldSerialize = instance => ((VersionOptions.CloudBuildNumberCommitIdOptions)instance).WhereOrDefault != VersionOptions.CloudBuildNumberCommitIdOptions.DefaultInstance.Where.Value;
                }
                
                if (property.DeclaringType == typeof(VersionOptions) && member.Name == nameof(VersionOptions.Release))
                {
                    property.ShouldSerialize = instance => !((VersionOptions)instance).ReleaseOrDefault.IsDefault;
                }
                
                if (property.DeclaringType == typeof(VersionOptions.ReleaseOptions) && member.Name == nameof(VersionOptions.ReleaseOptions.BranchName))
                {
                    property.ShouldSerialize = instance => ((VersionOptions.ReleaseOptions)instance).BranchNameOrDefault != VersionOptions.ReleaseOptions.DefaultInstance.BranchName;
                }

                if (property.DeclaringType == typeof(VersionOptions.ReleaseOptions) && member.Name == nameof(VersionOptions.ReleaseOptions.VersionIncrement))
                {
                    property.ShouldSerialize = instance => ((VersionOptions.ReleaseOptions)instance).VersionIncrementOrDefault != VersionOptions.ReleaseOptions.DefaultInstance.VersionIncrement.Value;
                }

                if (property.DeclaringType == typeof(VersionOptions.ReleaseOptions) && member.Name == nameof(VersionOptions.ReleaseOptions.FirstUnstableTag))
                {
                    property.ShouldSerialize = instance => ((VersionOptions.ReleaseOptions)instance).FirstUnstableTagOrDefault != VersionOptions.ReleaseOptions.DefaultInstance.FirstUnstableTag;
                }
            }

            return property;
        }
    }
}