using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            // Query to get the classes the student is enrolled in
            var classes = from e in db.Enrolleds
                        join c in db.Classes on e.Class equals c.ClassId
                        join co in db.Courses on c.Listing equals co.CatalogId
                        where e.Student == uid
                        select new
                        {
                            subject = co.Department,
                            number = co.Number,
                            name = co.Name,
                            season = c.Season,
                            year = c.Year,
                            grade = e.Grade == null ? "--" : e.Grade
                        };

            return Json(classes.ToArray());
        }



        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            // Query to get the class the student is enrolled in
            var query = from c in db.Classes
                        join co in db.Courses on c.Listing equals co.CatalogId
                        join ac in db.AssignmentCategories on c.ClassId equals ac.InClass
                        join a in db.Assignments on ac.CategoryId equals a.Category
                        join e in db.Enrolleds on c.ClassId equals e.Class
                        where co.Department == subject && co.Number == num && c.Season == season && c.Year == year && e.Student == uid
                        select new
                        {
                            aname = a.Name,
                            cname = ac.Name,
                            due = a.Due,
                            score = (from s in db.Submissions
                                    where s.Assignment == a.AssignmentId && s.Student == uid
                                    select (int?)s.Score).FirstOrDefault() // Cast to int? to allow nulls
                        };

            return Json(query.ToArray());
        }





        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
    string category, string asgname, string uid, string contents)
    {
        // Find the class the student is enrolled in
        var classQuery = from c in db.Classes
                        join co in db.Courses on c.Listing equals co.CatalogId
                        where co.Department == subject && co.Number == num && c.Season == season && c.Year == year
                        join e in db.Enrolleds on c.ClassId equals e.Class
                        where e.Student == uid
                        select c;

        var classObj = classQuery.FirstOrDefault();
        if (classObj == null)
        {
            // The student is not enrolled in the class
            return Json(new { success = false });
        }

        // Find the assignment category
        var categoryQuery = from ac in db.AssignmentCategories
                            where ac.Name == category && ac.InClass == classObj.ClassId
                            select ac;

        var categoryObj = categoryQuery.FirstOrDefault();
        if (categoryObj == null)
        {
            // The assignment category does not exist
            return Json(new { success = false });
        }

        // Find the assignment
        var assignmentQuery = from a in db.Assignments
                            where a.Name == asgname && a.Category == categoryObj.CategoryId
                            select a;

        var assignmentObj = assignmentQuery.FirstOrDefault();
        if (assignmentObj == null)
        {
            // The assignment does not exist
            return Json(new { success = false });
        }

        // Check if the student has already submitted
        var submissionQuery = from s in db.Submissions
                            where s.Assignment == assignmentObj.AssignmentId && s.Student == uid
                            select s;

        var submissionObj = submissionQuery.FirstOrDefault();

        if (submissionObj != null)
        {
            // Update the existing submission
            submissionObj.SubmissionContents = contents;
            submissionObj.Time = DateTime.Now;
        }
        else
        {
            // Create a new submission
            Submission newSubmission = new Submission
            {
                Assignment = assignmentObj.AssignmentId,
                Student = uid,
                Score = 0, // Initial score
                SubmissionContents = contents,
                Time = DateTime.Now
            };
            db.Submissions.Add(newSubmission);
        }

        // Save changes to the database
        try
        {
            db.SaveChanges();
            return Json(new { success = true });
        }
        catch (Exception)
        {
            return Json(new { success = false });
        }
    }



        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
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

            // Check if the student is already enrolled in the class
            var enrollmentQuery = from e in db.Enrolleds
                                where e.Class == classObj.ClassId && e.Student == uid
                                select e;

            var enrollmentObj = enrollmentQuery.FirstOrDefault();
            if (enrollmentObj != null)
            {
                // The student is already enrolled in the class
                return Json(new { success = false });
            }

            // Enroll the student in the class
            Enrolled newEnrollment = new Enrolled
            {
                Student = uid,
                Class = classObj.ClassId,
                Grade = "--" // Initial grade
            };
            db.Enrolleds.Add(newEnrollment);

            // Save changes to the database
            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }




        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            // Mapping of letter grades to grade points
            var gradePoints = new Dictionary<string, double>
            {
                { "A", 4.0 },
                { "A-", 3.7 },
                { "B+", 3.3 },
                { "B", 3.0 },
                { "B-", 2.7 },
                { "C+", 2.3 },
                { "C", 2.0 },
                { "C-", 1.7 },
                { "D+", 1.3 },
                { "D", 1.0 },
                { "D-", 0.7 },
                { "E", 0.0 }
            };

            // Query to get the grades for the student
            var grades = from e in db.Enrolleds
                        where e.Student == uid && e.Grade != "--"
                        select e.Grade;

            if (!grades.Any())
            {
                // The student is not enrolled in any classes or has no valid grades
                return Json(new { gpa = 0.0 });
            }

            // Calculate the GPA
            double totalPoints = 0;
            int count = 0;
            
            foreach (var grade in grades)
            {
                if (gradePoints.ContainsKey(grade))
                {
                    totalPoints += gradePoints[grade];
                    count++;
                }
            }

            double gpa = totalPoints / count;

            return Json(new { gpa });
        }

                
        /*******End code to modify********/

    }
}

