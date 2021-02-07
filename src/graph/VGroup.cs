using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphGroup = Microsoft.Graph.Group;

namespace Clarius.Edu.Graph
{
    public class VGroup
    {
        readonly Client client;
        readonly GraphGroup graphGroup;
        readonly Extension internalProfile;

        public VGroup(Client client, GraphGroup group)
        {
            this.client = client;
            this.graphGroup = group;

            if (group.Id == null)
            {
                throw new ArgumentException("group id should not be null on provided Group");
            }

            // try to cache the InternalProfileExtension
            if (group.Extensions != null)
            {
                foreach (var ext in group.Extensions)
                {
                    if (string.Equals(ext.Id, Constants.PROFILE_GROUPEXTENSION_ID, StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.internalProfile = ext;
                    }
                }
            }
        }

        public string Type
        {
            get
            {
                return this.internalProfile.AdditionalData[Constants.PROFILE_GROUPTYPE] as string;
            }
        }

        public string Level
        {
            get
            {
                return this.internalProfile.AdditionalData[Constants.PROFILE_GROUPLEVEL] as string;
            }
        }
        public string Year
        {
            get
            {
                try
                {
                    return this.internalProfile.AdditionalData[Constants.PROFILE_GROUPYEAR] as string;
                }
                catch
                {
                    return null;
                }
            }
        }
        public string Id
        {
            get
            {
                try
                {
                    return this.internalProfile.AdditionalData[Constants.PROFILE_GROUPID] as string;
                }
                catch
                {
                    return null;
                }
            }
        }

        public string Division
        {
            get
            {
                try
                {
                    return this.internalProfile.AdditionalData[Constants.PROFILE_GROUPDIVISION] as string;
                }
                catch
                {
                    return null;
                }
            }
        }

        public string Grade
        {
            get
            {
                return this.internalProfile.AdditionalData[Constants.PROFILE_GROUPGRADE] as string;
            }
        }

        public async Task Archive (bool readOnlySharepoint)
        {
            await client.Graph.Teams[graphGroup.Id].Archive(readOnlySharepoint).Request().PostAsync();
        }

        public async Task ChangeDisplayName(string displayName)
        {
            var g = new Microsoft.Graph.Group();
            g.DisplayName = displayName;
            await client.Graph.Groups[graphGroup.Id].Request().UpdateAsync(g);
        }

        public async Task CreateExtension(string extensionId, Dictionary<string, object> properties)
        {
            var ext = new OpenTypeExtension();
            ext.ExtensionName = extensionId;
            ext.Id = extensionId;
            ext.AdditionalData = properties;

            await client.Graph.Groups[graphGroup.Id.ToString()].Extensions.Request().AddAsync(ext);
        }

        public async Task DeleteExtension(string extensionId)
        {
            await client.Graph.Groups[graphGroup.Id.ToString()].Extensions[extensionId].Request().DeleteAsync();
        }

        public async Task<Extension> GetExtension(string extensionId)
        {
            var ext = new OpenTypeExtension();
            ext.ExtensionName = extensionId;
            ext.Id = extensionId;

            return await client.Graph.Groups[graphGroup.Id.ToString()].Extensions[ext.Id].Request().GetAsync();
        }

        public async Task UpdateExtension(string extensionId, Dictionary<string, object> properties)
        {
            var ext = new OpenTypeExtension();
            ext.ExtensionName = extensionId;
            ext.Id = extensionId;
            ext.AdditionalData = properties;

            await client.Graph.Groups[graphGroup.Id.ToString()].Extensions[ext.Id].Request().UpdateAsync(ext);
        }

        public async Task<bool> HasExtension(string extensionId)
        {
            var extList = await client.Graph.Users[graphGroup.Id.ToString()].Extensions.Request().GetAsync();
            foreach (var ext in extList)
            {
                if (ext.Id == extensionId)
                {
                    return true;
                }
            }

            return false;
        }

        public Extension InternalProfile
        {
            get
            {
                return internalProfile;
            }
        }

    }
}
