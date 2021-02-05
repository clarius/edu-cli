using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GraphUser = Microsoft.Graph.User;

namespace Clarius.Edu.Graph
{
    public class VUser
    {
        Client client;
        GraphUser graphUser;
        IUserLicenseDetailsCollectionPage licenses;
        Extension internalProfile = null;

        internal VUser(Client client, GraphUser user)
        {
            this.client = client;
            this.graphUser = user;

            // don't allow the creation of an User without this bit of data
            // as it is required to query some other data
            // maybe userId is just enough?
            if (user.Id == null)
            {
                throw new ArgumentException("user id should not be null on provided User");
            }

            // try to cache the InternalProfileExtension
            if (user.Extensions != null)
            {
                foreach (var ext in user.Extensions)
                {
                    if (string.Equals(ext.Id, Constants.PROFILE_USEREXTENSION_ID, StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.internalProfile = ext;
                    }
                }
            }
        }

        public Extension InternalProfile
        {
            get
            {
                return internalProfile;
            }
        }

        public async Task<bool?> HasSkuId(Guid skuId)
        {
            if (licenses == null)
            {
                await ReadLicenseData();

                // it may be the case this is still null? (no licenses at all asigned to the user?)
                if (licenses == null)
                {
                    return null;
                }
            }

            foreach (var lic in licenses)
            {
                if (lic.SkuId == Constants.STUDENT_LICENSE)
                    return true;
            }

            return false;
        }

        public string Type
        {
            get
            {
                object o;
                if (this.internalProfile.AdditionalData.TryGetValue(Constants.PROFILE_USERTYPE, out o))
                {
                    return (string)o;
                }

                return null;
            }
        }

        public string EnglishLevel
        {
            get
            {
                object o;
                if (this.internalProfile.AdditionalData.TryGetValue(Constants.PROFILE_USERENGLISHLEVEL, out o))
                {
                    return (string)o;
                }

                return null;
            }
        }

        public string Year
        {
            get
            {
                object o;
                if (this.internalProfile.AdditionalData.TryGetValue(Constants.PROFILE_USERYEAR, out o))
                {
                    return (string)o;
                }

                return null;
            }
        }


        public string Level
        {
            get
            {
                object o;
                if (this.internalProfile.AdditionalData.TryGetValue(Constants.PROFILE_USERLEVEL, out o))
                {
                    return (string)o;
                }

                return null;
            }
        }

        public string Grade
        {
            get
            {
                object o;
                if (this.internalProfile.AdditionalData.TryGetValue(Constants.PROFILE_USERGRADE, out o))
                {
                    return (string)o;
                }

                return null;
            }
        }

        public string Division
        {
            get
            {
                object o;
                if (this.internalProfile.AdditionalData.TryGetValue(Constants.PROFILE_USERDIVISION, out o))
                {
                    return (string)o;
                }

                return null;
            }
        }

        public string Id
        {
            get
            {
                return this.graphUser.Id;
            }
        }

        public bool IsStudent
        {
            get
            {
                if (internalProfile == null)
                {
                    return false;
                }

                return string.Equals((string)internalProfile.AdditionalData[Constants.PROFILE_USERTYPE],
                            Constants.USER_TYPE_STUDENT, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public bool IsTeacher
        {
            get
            {
                if (internalProfile == null)
                {
                    return false;
                }

                return string.Equals((string)internalProfile.AdditionalData[Constants.PROFILE_USERTYPE],
                            Constants.USER_TYPE_TEACHER, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public async Task CreateExtension(string extensionId, Dictionary<string, object> properties)
        {
            var ext = new OpenTypeExtension();
            ext.ExtensionName = extensionId;
            ext.Id = extensionId;
            ext.AdditionalData = properties;

            await client.Graph.Users[graphUser.Id.ToString()].Extensions.Request().AddAsync(ext);
        }

        public async Task DeleteExtension(string extensionId)
        {
            await client.Graph.Users[graphUser.Id.ToString()].Extensions[extensionId].Request().DeleteAsync();
        }

        public async Task<Extension> GetExtension(string extensionId)
        {
            var ext = new OpenTypeExtension();
            ext.ExtensionName = extensionId;
            ext.Id = extensionId;

            return await client.Graph.Users[graphUser.Id.ToString()].Extensions[ext.Id].Request().GetAsync();
        }

        public async Task UpdateExtension(string extensionId, Dictionary<string, object> properties)
        {
            var ext = new OpenTypeExtension();
            ext.ExtensionName = extensionId;
            ext.Id = extensionId;
            ext.AdditionalData = properties;

            await client.Graph.Users[graphUser.Id.ToString()].Extensions[ext.Id].Request().UpdateAsync(ext);
        }

        public async Task<bool> HasExtension(string extensionId)
        {
            var extList = await client.Graph.Users[graphUser.Id.ToString()].Extensions.Request().GetAsync();
            foreach (var ext in extList)
            {
                if (ext.Id == extensionId)
                {
                    return true;
                }
            }

            return false;
        }

        async Task ReadLicenseData()
        {
            licenses = await client.Graph.Users[graphUser.Id.ToString()].LicenseDetails.Request().GetAsync();
            return;
        }

        public async Task DownloadFile(DriveItem item, string path)
        {
            using (var stream = await client.Graph.Users[graphUser.Id].Drive.Items[item.Id].Content.Request().GetAsync())
            {
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    stream.CopyTo(fs);
                    fs.Flush();
                }
            }
        }
    }
}
