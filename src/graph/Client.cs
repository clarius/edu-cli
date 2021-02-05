using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GraphUser = Microsoft.Graph.User;
using GraphGroup = Microsoft.Graph.Group;
using Log = Serilog.Log;
using System.IO;
using System.Diagnostics;

namespace Clarius.Edu.Graph
{
    public class Client
    {
        private const string SelectUserClause = "id, displayname, userprincipalname,LastPasswordChangeDateTime, surname, givenname, preferredLanguage, accountEnabled";
        private const string SelectGroupClause = "id, displayname, mailnickname, groupTypes, description, createddatetime, mail";
        GraphServiceClient client;
        List<GraphUser> graphUsers = new List<GraphUser>();
        List<EducationUser> educationUsers = new List<EducationUser>();
        List<GraphGroup> graphGroups = new List<GraphGroup>();
        List<VUser> users = new List<VUser>();
        Dictionary<string, GraphUser> usersByUserPrincipalName = new Dictionary<string, GraphUser>();
        EducationConfig eduConfig;
        bool useCache;
#if USE_BETA
        public AssignmentsManager AssignmentsManager { get; private set; }
#endif

        public Client(string config, bool rawFormat)
        {
            try
            {
                this.eduConfig = System.Text.Json.JsonSerializer.Deserialize(System.IO.File.ReadAllText(config), typeof(EducationConfig)) as EducationConfig;
            }
            catch(FileNotFoundException)
            {
                throw new ArgumentException($"Config file {config} not found. Please specify one using the /config: parameter");
            }
            catch(Exception)
            {
                throw new ArgumentException($"Error opening {config} file.");
            }

            if (!rawFormat)
            {
                Log.Logger.Information($"CLI for managing Microsoft Education 365 tenants");
                Log.Logger.Information($"(C) 2020, 2021, Fundacion Clarius (hello@clarius.org)");
                Log.Logger.Information("");
                Log.Logger.Information($"Using Default domain: '{eduConfig.DefaultDomainName}'");
                Log.Logger.Information("");

            }
        }

        public async Task Connect(bool useAppPermissions = true, bool useCache = true, bool retrieveEducationUsers = false)
        {
            this.useCache = useCache;

            IAuthenticationProvider authProvider;
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            if (useAppPermissions)
            {
                var app = ConfidentialClientApplicationBuilder.Create(eduConfig.Application)
                        .WithClientSecret(eduConfig.ClientSecret)
                        .WithAuthority(AzureCloudInstance.AzurePublic, eduConfig.Directory)
                        .Build();


                var authenticationResult = await app.AcquireTokenForClient(scopes)
                    .ExecuteAsync();

                authProvider = new DelegateAuthenticationProvider(x =>
                {
                    x.Headers.Authorization = new AuthenticationHeaderValue(
                        "Bearer", authenticationResult.AccessToken);

                    return Task.FromResult(0);
                });
            }
            else
            {
                var app = PublicClientApplicationBuilder.Create(eduConfig.Application)
                    .WithRedirectUri("http://localhost")
                    .WithTenantId(this.eduConfig.Directory)
                    .Build();

                var authenticationResult = await app.AcquireTokenInteractive(scopes)
                    .ExecuteAsync();

                authProvider = new InteractiveAuthenticationProvider(app, scopes);
            }

            client = new GraphServiceClient(authProvider);


            if (useCache)
            {
                var getUsers = await client.Users.Request().Top(999).Select(SelectUserClause).Expand("extensions").GetAsync();

                graphUsers.AddRange(getUsers.CurrentPage);
                while (getUsers.NextPageRequest != null)
                {
                    getUsers = await getUsers.NextPageRequest.GetAsync();
                    graphUsers.AddRange(getUsers.CurrentPage);
                }

                var getGroups = await client.Groups.Request().Top(999).Select(SelectGroupClause).Expand("extensions").GetAsync();
                graphGroups.AddRange(getGroups.CurrentPage);
                while (getGroups.NextPageRequest != null)
                {
                    getGroups = await getGroups.NextPageRequest.GetAsync();
                    graphGroups.AddRange(getGroups.CurrentPage);
                }


                if (retrieveEducationUsers)
                {
                    var eduUsers = await client.Education.Users.Request().Top(999).GetAsync();
                    educationUsers.AddRange(eduUsers.CurrentPage);
                    while (eduUsers.NextPageRequest != null)
                    {
                        eduUsers = await eduUsers.NextPageRequest.GetAsync();
                        educationUsers.AddRange(eduUsers.CurrentPage);
                    }
                }

                foreach (var gu in graphUsers)
                {
                    users.Add(new VUser(this, gu));
                }
            }

#if USE_BETA
            AssignmentsManager = new AssignmentsManager(this);
#endif
        }

        // TODO: this should be internal
        public GraphServiceClient Graph
        {
            get { return client; }
        }

        public VUser GetUser(GraphUser graphUser)
        {
            return new VUser(this, graphUser);
        }
        public VGroup GetGroup(GraphGroup graphGroup)
        {
            return new VGroup(this, graphGroup);
        }

        public async Task<IGraphServiceGroupsCollectionPage> GetTeams(int top = 999)
        {
            // TODO: look for Team-specific attribute to filter on  ---> resourceProvisioningOptions on AdditionalMetadata
            return await client.Groups.Request().Top(top).GetAsync();
        }
        public async Task<EducationClass> AddTeacher(EducationClass eduClass, EducationUser teacher)
        {
            await client.Education.Classes[eduClass.Id.ToString()].Teachers.References.Request().AddAsync(teacher);
            return eduClass;
        }

        public async Task<EducationSchool> AddSchool(EducationSchool school)
        {
            return await client.Education.Schools
                .Request()
                .AddAsync(school);
        }

        public async Task<EducationClass> CreateClass(string displayName, string description, string mailNickname, string grade = null, string division = null, string level = null)
        {
            if (division != null && grade == null)
            {
                throw new Exception("When you specify a division, a grade is required");    // TODO: assuming it doesn't make sense to create a class Team composed of all "A" divisions in the entire school
            }

            var educationClass = new EducationClass
            {
                Description = description,
                ClassCode = displayName,
                DisplayName = displayName,
                MailNickname = mailNickname,
            };

            var eduData = new Dictionary<string, object>();

            List<GraphUser> usersToAdd = null;

            if (grade != null)
            {
                usersToAdd = GetFilteredUsers(graphUsers, grade, division, level);
            }

            var newClass = await client.Education.Classes.Request().AddAsync(educationClass);
            Log.Logger.Information($"CreateClass: {newClass.DisplayName}");

            if (usersToAdd != null)
            {
                foreach (var student in usersToAdd)
                {
                    var eduStudent = await client.Education.Users[student.Id.ToString()].Request().GetAsync();
                    await AddStudent(newClass, eduStudent);
                    Log.Logger.Information($"Added student '{RemoveDomainPart(eduStudent.UserPrincipalName)}'");
                }
            }

            return newClass;
        }

        public IEnumerable<Microsoft.Graph.User> GetTeachers(string userLevel)
        {
            var foo = graphUsers.Where(p => new VUser(this, p).IsTeacher);
            var bar = foo.Where(p => new VUser(this, p).Level == userLevel);

            return bar;
        }

        public List<Microsoft.Graph.User> GetUsers(IEnumerable<Microsoft.Graph.User> users, string grade, string division, string languageLevel, string level, string type)
        {
            List<Microsoft.Graph.User> userList = new List<Microsoft.Graph.User>();

            foreach (var p in users)
            {
                if (p.Extensions == null)
                {
                    continue; // TODO: an user without an extension... this should be logged somewhere...
                }
                try
                {
                    if (type != null && !type.Equals((string)p.Extensions[0].AdditionalData[Constants.PROFILE_USERTYPE], StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    if (grade != null && !grade.Equals((string)p.Extensions[0].AdditionalData[Constants.PROFILE_USERGRADE], StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    if (level != null && !level.Equals((string)p.Extensions[0].AdditionalData[Constants.PROFILE_USERLEVEL], StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    if (division != null && !division.Equals((string)p.Extensions[0].AdditionalData[Constants.PROFILE_USERDIVISION], StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    if (languageLevel != null && !languageLevel.Equals((string)p.Extensions[0].AdditionalData[Constants.PROFILE_USERENGLISHLEVEL], StringComparison.InvariantCultureIgnoreCase))
                        continue;
                }
                catch (Exception ex)
                {
                }

                userList.Add(p);
            }

            return userList;
        }

        public List<Microsoft.Graph.User> GetFilteredUsers(IEnumerable<Microsoft.Graph.User> users, string grade, string division = null, string languageLevel = null, string level = null)
        {
            List<Microsoft.Graph.User> userList = new List<Microsoft.Graph.User>();

            foreach (var p in users)
            {
                if (p.Extensions != null)
                {
                    if (p.Extensions.Count > 0)
                    {
                        try
                        {
                            if ((string)p.Extensions[0].AdditionalData[Constants.PROFILE_USERTYPE] == Clarius.Edu.Graph.Constants.USER_TYPE_STUDENT)
                            {
                                if ((string)p.Extensions[0].AdditionalData[Constants.PROFILE_USERLEVEL] == level)     
                                {
                                    if ((string)p.Extensions[0].AdditionalData[Constants.PROFILE_USERGRADE] == grade)
                                    {
                                        if (division == null)
                                        {
                                            if (languageLevel == null)
                                            {
                                                userList.Add(p);
                                            }
                                            else if (languageLevel == (string)p.Extensions[0].AdditionalData[Constants.PROFILE_USERENGLISHLEVEL])
                                            {
                                                userList.Add(p);
                                            }
                                        }
                                        else if ((string)p.Extensions[0].AdditionalData[Constants.PROFILE_USERDIVISION] == division)
                                        {
                                            userList.Add(p);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // TODO: log this
                            var str = ex.Message;
                        }
                    }
                }
            }

            return userList;
        }


        public async Task<EducationClass> AddStudent(EducationClass eduClass, EducationUser student)
        {
            await client.Education.Classes[eduClass.Id.ToString()].Members.References.Request().AddAsync(student);
            return eduClass;
        }

        public async Task AddUserToGroup(Guid groupId, Guid userId, bool addAsOwner = false)
        {

            var dirObject = await client.DirectoryObjects[userId.ToString()].Request().GetAsync();

            if (addAsOwner)
            {
                await client.Groups[groupId.ToString()].Owners.References.Request().AddAsync(dirObject);
            }
            else
            {
                await client.Groups[groupId.ToString()].Members.References.Request().AddAsync(dirObject);
            }
        }

        public async Task RemoveUserFromGroup(Guid groupId, Guid userId, bool removeOwner = false)
        {
            if (removeOwner)
            {
                await client.Groups[groupId.ToString()].Owners[userId.ToString()].Reference.Request().DeleteAsync();
            }
            else
            {
                await client.Groups[groupId.ToString()].Members[userId.ToString()].Reference.Request().DeleteAsync();
            }
        }

        public GraphGroup GetGroupByIdAndLevel(string id, string level)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(level))
            {
                throw new ArgumentException("You must provide an id and level");
            }

            foreach (var p in graphGroups)
            {
                if (p.Extensions != null)
                {
                    if (p.Extensions.Count > 0)
                    {
                        try
                        {
                            if (String.Compare((string)p.Extensions[0].AdditionalData[Constants.PROFILE_GROUPID], id, true) == 0)
                            {
                                if (String.Compare((string)p.Extensions[0].AdditionalData[Constants.PROFILE_GROUPLEVEL], level, true) == 0)
                                {
                                    return p;
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            // TODO: log this
                            var str = ex.Message;
                        }
                    }
                }
            }

            return null;

        }
        public List<GraphGroup> GetGroups(string groupType, string groupLevel, string groupGrade = null, string groupDivision = null)
        {
            List<GraphGroup> groupList = new List<GraphGroup>();

            foreach (var p in graphGroups)
            {
                if (p.Extensions != null)
                {
                    if (p.Extensions.Count > 0)
                    {
                        try
                        {
                            if (String.Compare((string)p.Extensions[0].AdditionalData[Constants.PROFILE_GROUPTYPE], groupType, true) == 0)
                            {
                                if (groupLevel != null && String.Compare((string)p.Extensions[0].AdditionalData[Constants.PROFILE_GROUPLEVEL], groupLevel, true) != 0)
                                {
                                    continue;
                                }

                                if (!string.IsNullOrEmpty(groupGrade) && String.Compare((string)p.Extensions[0].AdditionalData[Constants.PROFILE_GROUPGRADE], groupGrade, true) != 0)
                                {
                                    continue;
                                }

                                if (!string.IsNullOrEmpty(groupDivision) && String.Compare((string)p.Extensions[0].AdditionalData[Constants.PROFILE_GROUPDIVISION], groupDivision, true) != 0)
                                {
                                    continue;
                                }

                                groupList.Add(p);
                            }
                        }
                        catch (Exception ex)
                        {
                            // TODO: log this
                            var str = ex.Message;
                        }
                    }
                }
            }

            return groupList;
        }

        public List<Microsoft.Graph.User> GetUsers(string userType, string userLevel = null)
        {
            List<Microsoft.Graph.User> userList = new List<Microsoft.Graph.User>();

            foreach (var p in graphUsers)
            {
                if (p.Extensions != null)
                {
                    if (p.Extensions.Count > 0)
                    {
                        try
                        {
                            if ((string)p.Extensions[0].AdditionalData[Constants.PROFILE_USERTYPE] == userType)
                            {
                                if (userLevel != null && (string)p.Extensions[0].AdditionalData[Constants.PROFILE_USERLEVEL] == userLevel)   
                                {
                                    userList.Add(p);
                                }
                                else if (userLevel == null)
                                {
                                    userList.Add(p);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // TODO: log this
                            var str = ex.Message;
                        }
                    }
                }
            }

            return userList;
        }

        public string RemoveDomainPart(string userPrincipalName)
        {
            var at = userPrincipalName.IndexOf("@");
            if (at != -1)
            {
                return userPrincipalName.Substring(0, at);
            }

            return userPrincipalName;
        }


        public async Task<GraphGroup> CreateGroup(string displayName, string description, string mailNickname, IEnumerable<DirectoryObject> members, IEnumerable<DirectoryObject> owners, string groupVisibility = "HiddenMembership")
        {
            Log.Logger.Information("Entering CreateGroup");

            var group = new GraphGroup
            {
                Description = description,
                DisplayName = displayName,
                GroupTypes = new List<String>()
                {
                    "Unified"
                },
                Visibility = groupVisibility,
                MailEnabled = true,
                MailNickname = mailNickname,
                SecurityEnabled = false,
            };

            var newGroup = await client.Groups
                .Request()
                .AddAsync(group);

            foreach (var member in members)
            {
                await client.Groups[newGroup.Id.ToString()].Members.References.Request().AddAsync(member);
                // Log.Logger.Information($"Adding member '{RemoveDomainPart(member.UserPrincipalName)}'");
            }

            foreach (var owner in owners)
            {
                await client.Groups[newGroup.Id.ToString()].Owners.References.Request().AddAsync(owner);
                // Log.Logger.Information($"Adding owner '{RemoveDomainPart(owner.UserPrincipalName)}'");
            }

            return newGroup;
        }

//        public async Task<GraphUser> CreateUser(string username, string password, string firstname, string lastname, string userType, string userLevel, string userGrade, string userDivision, string userLanguagelevel, string usageLocation = "AR")
        public async Task<GraphUser> CreateUser(string username, string password, string firstname, string lastname, string userType, string userLevel, string userGrade, string userDivision, string userLanguagelevel, string userYear, string userNationalID)
        {
            var user = new GraphUser
            {
                AccountEnabled = true,
                Surname = lastname,
                GivenName = firstname,
                DisplayName = $"{firstname} {lastname}",
                MailNickname = username,
                UserPrincipalName = $"{username}@{eduConfig.DefaultDomainName}",
                UsageLocation = Config.UsageLocation,
                PreferredLanguage = Config.PreferredLanguage,    // TODO: this should come from a settings file
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = true,
                    Password = password
                }
            };

            // for now we only care about Licenses for Student and Teachers, and do not disable any services/etc
            // users of type Preceptor and Fundacion will get Teacher Licenses for now
            Guid license = userType == Constants.USER_TYPE_STUDENT ? Constants.STUDENT_LICENSE : Constants.TEACHER_LICENSE;
            var addLicenses = new List<AssignedLicense>()
                {
                    new AssignedLicense
                    {
                        SkuId = license
                    }
                };

            var removeLicenses = new List<Guid>()
            {
            };

            // create the user
            var addedUser = await client.Users.Request().AddAsync(user);

            Log.Logger.Information($"User {username} created");

            // assign the license
            await client.Users[addedUser.Id].AssignLicense(addLicenses, removeLicenses).Request().PostAsync();
            Log.Logger.Information($"Assigned license '{GetLicenseFriendlyName(license)}' to '{username}'");

            var customMetadata = new Dictionary<string, object>
                        {
                            { Constants.PROFILE_USERTYPE, userType },
                            { Constants.PROFILE_USERLEVEL, userLevel },
                            { Constants.PROFILE_USERGRADE, userGrade },
                            { Constants.PROFILE_USERDIVISION, userDivision },
                            { Constants.PROFILE_USERENGLISHLEVEL, userLanguagelevel }
                        };



            await GetUser(addedUser).CreateExtension(Constants.PROFILE_USEREXTENSION_ID, customMetadata);

            Log.Logger.Information($"Added internal profile to '{username}'");

            return addedUser;
        }

        public async Task<EducationUser> CreateEducationUser(string username, string password, string firstname, string lastname, string userType, string userLevel, string userGrade, string userDivision, string userLanguagelevel, string userYear, string userNationalID)
        {
            var user = new Microsoft.Graph.EducationUser
            {
                AccountEnabled = true,
                PrimaryRole = userType == Constants.USER_TYPE_STUDENT ? EducationUserRole.Student : EducationUserRole.Teacher,
                Surname = lastname,
                GivenName = firstname,
                DisplayName = $"{firstname} {lastname}",
                MailNickname = username,
                UserPrincipalName = $"{username}@{eduConfig.DefaultDomainName}",
                UsageLocation = Config.UsageLocation,
                PreferredLanguage = Config.PreferredLanguage,
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = true,
                    Password = password
                },
            };

            // for now we only care about Licenses for Student and Teachers, and do not disable any services/etc
            Guid license = userType == Constants.USER_TYPE_STUDENT ? Constants.STUDENT_LICENSE : Constants.TEACHER_LICENSE;
            var addLicenses = new List<AssignedLicense>()
                {
                    new AssignedLicense
                    {
                        SkuId = license
                    }
                };

            var removeLicenses = new List<Guid>()
            {
            };

            // create the user
            var addedUser = await client.Education.Users.Request().AddAsync(user);

            Log.Logger.Information($"Education User {username} created");

            // assign the license
            await client.Users[addedUser.Id].AssignLicense(addLicenses, removeLicenses).Request().PostAsync();
            Log.Logger.Information($"Assigned license '{GetLicenseFriendlyName(license)}' to '{username}'");

            var customMetadata = new Dictionary<string, object>
                        {
                            { Constants.PROFILE_USERTYPE, userType },
                            { Constants.PROFILE_USERLEVEL, userLevel },
                            { Constants.PROFILE_USERGRADE, userGrade },
                            { Constants.PROFILE_USERDIVISION, userDivision },
                            { Constants.PROFILE_USERYEAR, userYear },
                            { Constants.PROFILE_USERNATIONALID, userNationalID },
                            { Constants.PROFILE_USERENGLISHLEVEL, userLanguagelevel }
                        };


            // TODO: education users don't have extensions...
            await GetUser(GetUserFromId(new Guid(addedUser.Id))).CreateExtension(Constants.PROFILE_USEREXTENSION_ID, customMetadata);

            Log.Logger.Information($"Added internal profile to '{username}'");

            return addedUser;
        }

        string GetLicenseFriendlyName(Guid license)
        {
            if (license == Constants.STUDENT_LICENSE)
                return Constants.STUDENT_LICENSE_NAME;
            else if (license == Constants.TEACHER_LICENSE)
                return Constants.TEACHER_LICENSE_NAME;

            return Constants.UNKNOWN_LICENSE_NAME;
        }

        public bool UserAlreadyExists(string username)
        {
            if (!username.Contains("@"))
            {
                username += "@" + eduConfig.DefaultDomainName;
            }

            if (graphUsers.Where(p => p.UserPrincipalName == username).Count() == 0)
            {
                return false;
            }

            return true;
        }


        public IEnumerable<GraphUser> GetUsers()
        {
            return graphUsers;
        }

        public IEnumerable<GraphGroup> GetGroups()
        {
            return graphGroups;
        }

        public EducationConfig Config
        {
            get
            {
                return eduConfig;
            }
        }

        public async Task<GraphGroup> GetGroupFromAlias(string groupAlias)
        {
            //// if no domain is specified, use default one
            //if (!groupAlias.Contains("@"))
            //{
            //    groupAlias += "@" + eduConfig.DefaultDomainName;
            //}

            if (useCache)
            {
                return graphGroups.Where(p => string.Compare(p.MailNickname, groupAlias, true) == 0).SingleOrDefault();
            }
            else
            {
                // TODO: use msgraph to query by alias
                return null;
            }
        }
        public async Task<GraphUser> GetUserFromUserPrincipalName(string userPrincipalName)
        {
            // if no domain is specified, use default one
            if (!userPrincipalName.Contains("@"))
            {
                userPrincipalName += "@" + eduConfig.DefaultDomainName;
            }

            if (useCache)
            {
                return graphUsers.Where(p => string.Compare(p.UserPrincipalName, userPrincipalName, true) == 0).SingleOrDefault();
            }
            else
            {
                return await client
                    .Users[userPrincipalName]
                    .Request().Select(SelectUserClause).Expand("extensions").GetAsync();
            }
        }

        public GraphUser GetUserFromId(Guid userId)
        {
            return graphUsers.Where(p => new Guid(p.Id) == userId).Single();
        }

        public GraphGroup GetGroupFromId(Guid groupId)
        {
            return graphGroups.Where(p => new Guid(p.Id) == groupId).Single();
        }

        public GraphGroup GetGroupFromEmail(string groupEmail)
        {
            if (!groupEmail.Contains("@"))
            {
                groupEmail += "@" + eduConfig.DefaultDomainName;
            }

            return graphGroups.Where(p => string.Compare(p.Mail, groupEmail, true) == 0).SingleOrDefault();
        }


        public EducationUser GetEducationUserFromId(Guid userId)
        {
            if (educationUsers == null)
            {
                throw new Exception("educationUsers not initialized, please call Client.Connect with a flag to do so");
            }

            return educationUsers.Where(p => new Guid(p.Id) == userId).Single();
        }

        public bool ValidateUserType(string userType)
        {
            foreach (var ut in SchoolManager.UserTypes)
            {
                if (string.Equals(userType, ut, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        public bool ValidateGroupType(string type)
        {
            type = type.ToLower();

            if (type.Equals(Constants.GROUP_TYPE_CLASS, StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
        }

        public bool ValidateLevel(string level)
        {
            foreach (var l in SchoolManager.Levels)
            {
                if (string.Equals(level, l, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }

        public bool ValidateDivision(string division)
        {
            foreach (var d in SchoolManager.Divisions)
            {
                if (string.Equals(division, d, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            // allow empty string as a way to specifiy a null division
            if (division == "")
                return true;

            return false;
        }

        public bool ValidateGrade(string grade)
        {
            foreach (var g in SchoolManager.Grades)
            {
                if (string.Equals(grade, g, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            // allow empty string as a way to specifiy a null grade
            if (grade == "")
                return true;

            return false;
        }

        public bool ValidateEnglishLevel(string englishLevel)
        {
            foreach (var el in SchoolManager.EnglishLevels)
            {
                if (string.Equals(englishLevel, el, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
