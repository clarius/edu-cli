// EXPERIMENTAL WIP

#if USE_BETA
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clarius.Edu.Graph
{
    public class AssignmentsManager
    {
        Dictionary<User, List<TeacherAssignment>> teacherList;
        Client client;

        internal AssignmentsManager(Client client)
        {
            teacherList = new Dictionary<User, List<TeacherAssignment>>();
            this.client = client;
        }

        public IEnumerable<User> GetTeachers()
        {
            return teacherList.Keys;
        }

#if USE_BETA
        public async Task<IEducationClassAssignmentsCollectionPage> GetAssignments(string classId)
        {
            return await client.Graph.Education.Classes[classId].Assignments.Request().GetAsync();
        }

        public async Task<IEducationAssignmentResourcesCollectionPage> GetResources(string classId, string assignmentId)
        {
            return await client.Graph.Education.Classes[classId].Assignments[assignmentId].Resources.Request().GetAsync();
        }

        public async Task<IEducationAssignmentSubmissionsCollectionPage> GetSubmissions(string classId, string assignmentId)
        {
            return await client.Graph.Education.Classes[classId].Assignments[assignmentId].Submissions.Request().GetAsync();
        }
#endif

        void AddAssignment(User user, TeacherAssignment assignment)
        {
            List<TeacherAssignment> assignments;

            if (!teacherList.TryGetValue(user, out assignments))
            {
                assignments = new List<TeacherAssignment>();
                teacherList.Add(user, assignments);
            }

            assignments.Add(assignment);
        }

        public List<TeacherAssignment> GetAssignments(User teacher)
        {
            List<TeacherAssignment> assigments;

            teacherList.TryGetValue(teacher, out assigments);

            if (assigments != null)
                return assigments;

            assigments = new List<TeacherAssignment>();
            teacherList.Add(teacher, assigments);



            return assigments;
        }

        public bool IsDocumentFile(string name)
        {
            if (name.EndsWith(".docx"))
                return true;

            return false;
        }

        bool IsKnownFileType(string name)
        {
            if (name.EndsWith(".docx") || name.EndsWith(".mp3"))
            {
                return true;
            }

            return false;
        }
    }

    public class TeacherAssignment
    {
        private readonly List<EducationAssignmentResource> resources;
        private readonly List<EducationSubmission> submissions;
        private readonly EducationAssignment assignment;


        public List<EducationAssignmentResource> Resources => resources;
        public List<EducationSubmission> Submissions => submissions;

        public TeacherAssignment(EducationAssignment assignment)
        {
            this.assignment = new EducationAssignment();
            this.submissions = new List<EducationSubmission>();
            this.resources = new List<EducationAssignmentResource>();
            this.assignment = assignment;
        }

        public void AddResource(EducationAssignmentResource resource)
        {
            resources.Add(resource);
        }

        public void AddSubmissions(EducationSubmission submission)
        {
            submissions.Add(submission);
        }

        public EducationAssignment Assignment { get { return this.assignment; } }

        public int SubmissionsReleased
        {
            get
            {
                return submissions.Where(p => p.Status.HasValue && p.Status.Value == EducationSubmissionStatus.Released).Count();
            }
        }
        public int SubmissionsReturned
        {
            get
            {
                return submissions.Where(p => p.Status.HasValue && p.Status.Value == EducationSubmissionStatus.Returned).Count();
            }
        }
        public int SubmissionsSubmitted
        {
            get
            {
                return submissions.Where(p => p.Status.HasValue && p.Status.Value == EducationSubmissionStatus.Submitted).Count();
            }
        }
        public int SubmissionsWorking
        {
            get
            {
                return submissions.Where(p => p.Status.HasValue && p.Status.Value == EducationSubmissionStatus.Working).Count();
            }
        }
    }

}
#endif