using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/
        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var studentsInclass = from e in db.Enrolleds where e.ClassNavigation.ListingNavigation.Department == subject &&
            e.ClassNavigation.ListingNavigation.Number == num && e.ClassNavigation.Season == season && e.ClassNavigation.Year == year
            select new
            {
                fname = e.StudentNavigation.FName,
                lname = e.StudentNavigation.LName,
                uid = e.StudentNavigation.UId,
                dob = e.StudentNavigation.Dob,
                grade = e.Grade
            };

            return Json(studentsInclass);
        }

        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class,
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            var assignmentInCategory = from a in db.Assignments where a.CategoryNavigation.InClassNavigation.ListingNavigation.Department
            == subject && a.CategoryNavigation.InClassNavigation.ListingNavigation.Number ==
            num && a.CategoryNavigation.InClassNavigation.Season == season && a.CategoryNavigation.InClassNavigation.Year == year &&
            (category == null || a.CategoryNavigation.Name == category)
            select new
            {
                aname = a.Name,
                cname = a.CategoryNavigation.Name,
                due = a.Due,
                //using count to get the number of submissions to the assignment
                submissions = a.Submissions.Count()
            };
            return Json(assignmentInCategory);
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var assignmentCategory = from ac in db.AssignmentCategories where
            ac.InClassNavigation.ListingNavigation.Department == subject &&
            ac.InClassNavigation.ListingNavigation.Number == num &&
            ac.InClassNavigation.Season == season &&
            ac.InClassNavigation.Year == year
            select new
            {
                name = ac.Name,
                weight = ac.Weight
            };

            return Json(assignmentCategory);
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, uint catweight)
        {
            // Find the class based on the provided parameters
            var classQuery = from c in db.Classes
                            join co in db.Courses on c.Listing equals co.CatalogId
                            where co.Department == subject && co.Number == num && c.Season == season && c.Year == year
                            select c;
            var cls = classQuery.FirstOrDefault();
            // If the class doesn't exist, return success = false
            if (cls == null)
            {
                return Json(new { success = false });
            }
            // Check if the category already exists for this class
            var categoryQuery = from ac in db.AssignmentCategories
                                where ac.InClass == cls.ClassId && ac.Name == category
                                select ac;
            if (categoryQuery.Any())
            {
                return Json(new { success = false });
            }
            // Create the new assignment category
            var newCategory = new AssignmentCategory
            {
                Name = category,
                Weight = catweight,
                InClass = cls.ClassId
            };
            db.AssignmentCategories.Add(newCategory);
            db.SaveChanges();
            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            // Find the class based on the provided parameters
            var classQuery = from c in db.Classes
                            join co in db.Courses on c.Listing equals co.CatalogId
                            where co.Department == subject && co.Number == num && c.Season == season && c.Year == year
                            select c;

            var classObj = classQuery.FirstOrDefault();
            if (classObj == null)
            {
                // The class does not exist
                return Json(new { success = false });
            }

            // Find the assignment category based on the provided category name and the class found above
            var categoryQuery = from ac in db.AssignmentCategories
                                where ac.Name == category && ac.InClass == classObj.ClassId
                                select ac;

            var categoryObj = categoryQuery.FirstOrDefault();
            if (categoryObj == null)
            {
                // the assignment category does not exist
                return Json(new { success = false });
            }

            // Create a new assignment with the given parameters
            Assignment newAssignment = new Assignment
            {
                Name = asgname,
                Contents = asgcontents,
                Due = asgdue,
                MaxPoints = (uint)asgpoints,
                Category = categoryObj.CategoryId
            };

            db.Assignments.Add(newAssignment);
            db.SaveChanges();
            UpdateAllStudentGrades(classObj.ClassId);

            return Json(new { success = true });
        }



        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            // Find the class based on the provided parameters
            var classQuery = from c in db.Classes
                            join co in db.Courses on c.Listing equals co.CatalogId
                            where co.Department == subject && co.Number == num && c.Season == season && c.Year == year
                            select c;

            var classObj = classQuery.FirstOrDefault();
            if (classObj == null)
            {
                // The class does not exist
                return Json(null);
            }

            // Find the assignment category based on the provided category name and the class found above
            var categoryQuery = from ac in db.AssignmentCategories
                                where ac.Name == category && ac.InClass == classObj.ClassId
                                select ac;

            var categoryObj = categoryQuery.FirstOrDefault();
            if (categoryObj == null)
            {
                // The assignment category does not exist
                return Json(null);
            }

            // Find the assignment based on the provided assignment name and the category found above
            var assignmentQuery = from a in db.Assignments
                                where a.Name == asgname && a.Category == categoryObj.CategoryId
                                select a;

            var assignmentObj = assignmentQuery.FirstOrDefault();
            if (assignmentObj == null)
            {
                // The assignment does not exist
                return Json(null);
            }

            // Get all the submissions to the found assignment
            var submissionsQuery = from s in db.Submissions
                                join st in db.Students on s.Student equals st.UId
                                where s.Assignment == assignmentObj.AssignmentId
                                select new
                                {
                                    fname = st.FName,
                                    lname = st.LName,
                                    uid = st.UId,
                                    time = s.Time,
                                    score = s.Score
                                };

            return Json(submissionsQuery.ToArray());
        }



        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            // Find the class based on the provided parameters
            var classQuery = from c in db.Classes
                            join co in db.Courses on c.Listing equals co.CatalogId
                            where co.Department == subject && co.Number == num && c.Season == season && c.Year == year
                            select c;

            var classObj = classQuery.FirstOrDefault();
            if (classObj == null)
            {
                // The class does not exist
                return Json(new { success = false });
            }

            // Find the assignment category based on the provided category name and the class found above
            var categoryQuery = from ac in db.AssignmentCategories
                                where ac.Name == category && ac.InClass == classObj.ClassId
                                select ac;

            var categoryObj = categoryQuery.FirstOrDefault();
            if (categoryObj == null)
            {
                // The assignment category does not exist
                return Json(new { success = false });
            }

            // Find the assignment based on the provided assignment name and the category found above
            var assignmentQuery = from a in db.Assignments
                                where a.Name == asgname && a.Category == categoryObj.CategoryId
                                select a;

            var assignmentObj = assignmentQuery.FirstOrDefault();
            if (assignmentObj == null)
            {
                // the assignment does not exist
                return Json(new { success = false });
            }

            // Find the submission based on the assignment and student's UID
            var submissionQuery = from s in db.Submissions
                                where s.Assignment == assignmentObj.AssignmentId && s.Student == uid
                                select s;

            var submissionObj = submissionQuery.FirstOrDefault();
            if (submissionObj == null)
            {
                // The submission does not exist
                return Json(new { success = false });
            }

            // Update the score of the submission
            submissionObj.Score = (uint)score;

            // Save the changes to the database
            db.SaveChanges();
            UpdateGrade(uid, classObj.ClassId);
            return Json(new {success = true});
        }
        //helper method to upgrade the enrolled table grade field based on students
        //assignment submission
        private void UpdateGrade(string studentId, uint classId) {
            var enrolled = db.Enrolleds.FirstOrDefault(e =>
            e.Class == classId && e.Student == studentId);

            if (enrolled != null) {
                var classObject = enrolled.ClassNavigation;

                var assignmentCategories = db.AssignmentCategories.Where(ac => ac.InClass == classObject.ClassId).ToList();

                double cumulative = 0.0;
                double totalPoints = 0.0;

                foreach (var category in assignmentCategories) {
                    var assignments = db.Assignments.Where(a => a.Category == category.CategoryId).ToList();
                    bool hasAssignment = assignments.Count() != 0;

                    double categoryTotalPoints = 0.0;
                    double categoryMaxPoints = 0.0;

                    foreach (var assignment in assignments) {
                        double assignmentMaxPoints = assignment.MaxPoints;
                        categoryMaxPoints += assignmentMaxPoints;

                        var submission = db.Submissions.SingleOrDefault(s => s.Assignment == assignment.AssignmentId &&
                        s.Student == studentId);
                        
                        if (submission != null) {
                            double assignmentTotalPoints = (double) submission.Score;
                            categoryTotalPoints += assignmentTotalPoints;
                        }
                    }

                    if (hasAssignment) {
                        double categoryPercentage = categoryMaxPoints > 0 ? categoryTotalPoints / categoryMaxPoints : 0.0;
                        double scaledCategory = categoryPercentage * category.Weight;

                        cumulative += scaledCategory;
                        totalPoints += category.Weight;
                    }
                }
                string grade = ToGradePoint(cumulative, totalPoints);
                enrolled.Grade = grade;
                db.SaveChanges();
            }
        }//end of update grade method

        public static string ToGradePoint(double cumulative, double totalPoints) {
            double gradePoint = cumulative / totalPoints * 100;

            if (gradePoint >= 93)
            {
                return "A";
            }
            else if (gradePoint >= 90)
            {
                return "A-";
            }
            else if (gradePoint >= 87)
            {
                return "B+";
            }
            else if (gradePoint >= 83)
            {
                return "B";
            }
            else if (gradePoint >= 80)
            {
                return "B-";
            }
            else if (gradePoint >= 77)
            {
                return "C+";
            }
            else if (gradePoint >= 73)
            {
                return "C";
            }
            else if (gradePoint >= 70)
            {
                return "C-";
            }
            else if (gradePoint >= 67)
            {
                return "D+";
            }
            else if (gradePoint >= 63)
            {
                return "D";
            }
            else if (gradePoint >= 60)
            {
                return "D-";
            }
            else
            {
                return "E";
            }
        }

        public void UpdateAllStudentGrades(uint classId) {
            var enrolleds = db.Enrolleds.Where(e => e.Class == classId).ToList();
            foreach (var enrolled in enrolleds) {
                UpdateGrade(enrolled.Student, classId);
            }
        }
        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            // Query to get the classes taught by the specified professor
            var classes = from c in db.Classes
                        join co in db.Courses on c.Listing equals co.CatalogId
                        where c.TaughtBy == uid
                        select new
                        {
                            subject = co.Department,
                            number = co.Number,
                            name = co.Name,
                            season = c.Season,
                            year = c.Year
                        };

            return Json(classes.ToArray());
        }
    }
}