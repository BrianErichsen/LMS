using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            //first we check if given subject and name is already in database
            var is_department_real = db.Departments.FirstOrDefault(d => d.Subject == subject);
            if (is_department_real != null) {
                return Json(new { success = false});
            }
            // creates new department
            var department = new Department
            {
                Subject = subject,//sets Subject and Name
                Name = name
            };
            db.Departments.Add(department);//adds new department to Departments table
            db.SaveChanges();//save changes
            //changes success to true
            return Json(new { success = true});
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            //searches for specific courses with lambda expression where
            //in the courses table department if FK to subject
            var courses = db.Courses.Where(c => c.Department == subject)
            .Select(c => new
            {
                number = c.Number,
                name = c.Name
            }).ToList();
            return Json(courses);
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {   //same process used in previous method where we use lambda expression
        // and match given subject to specific professors
            var professors = db.Professors.Where(p => p.WorksIn == subject)
            .Select(p => new
            {   //sets proper fields for each object in the array
                lname = p.LName,
                fname = p.FName,
                uid = p.UId
            }).ToList();

            return Json(professors);
            
        }

        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {   //first check if the course already is in the system
            var is_course_real = db.Courses.FirstOrDefault(c => c.Department == subject && c.Number == number);
            //does not create new course if it already exists
            if (is_course_real != null)
            {
                return Json(new { success = false });
            }
            //creates new course
            var course = new Course
            {   //sets specific fields to the new course
                Department = subject,//department is fk to subject
                Number = (ushort) number,
                Name = name
            };//finally adds new course and save changes to the database
            db.Courses.Add(course);
            db.SaveChanges();
            //true since we added a new course
            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if another class occupies the same location during any time
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            // checks if specific course exists before hand
            var course = db.Courses.FirstOrDefault(c => c.Department == subject && c.Number == number);
            if (course == null)
            {
                Console.WriteLine("Non existing course!");
                return Json(new { success = false });
            }
            // checks if class is already existing
            var existingClass = db.Classes.FirstOrDefault(c =>
                c.Listing == course.CatalogId &&
                c.Season == season &&
                c.Year == year);
            //if specific class already exists then returns false
            if (existingClass != null)
            {
                Console.WriteLine("The class already exists!");
                return Json(new { success = false });
            }
            // after doing our checks - matches given location, season and year for c.
            var classes = db.Classes.Where(c =>
                c.Location == location &&
                c.Season == season &&
                c.Year == year).ToList();

            // converts given input for start and end date time to timeOnly type
            TimeOnly startTime = TimeOnly.FromDateTime(start);
            TimeOnly endTime = TimeOnly.FromDateTime(end);
            // checks for conflicting location and times as well
            var incompatibleClass = classes.FirstOrDefault(c =>
            {
                return !(startTime.CompareTo(c.EndTime) >= 0 || endTime.CompareTo(c.StartTime) <= 0);
            });
            //if incompatible class exists then returns false
            if (incompatibleClass != null)
            {
                Console.WriteLine("Same location time conflict!");
                return Json(new { success = false });
            }
            // all classes must have a specific professor -- searches for specific given
            //professor
            var professor = db.Professors.FirstOrDefault(p => p.UId == instructor);
            if (professor == null)
            {   //if no matches then writes error and returns false
                Console.WriteLine("No match for any professor given input!");
                return Json(new { success = false });
            }
            // checks for conflicting scheduling constrains of specific professor for class
            var incompatibleProfessor = db.Classes.FirstOrDefault(c =>
                c.TaughtBy == instructor && c.Season == season && c.Year == year &&
                !(startTime.CompareTo(c.EndTime) >= 0 || endTime.CompareTo(c.StartTime) <= 0));
            //if given professor cannot teach extra class due to time constrains
            if (incompatibleProfessor != null)
            {   //communicates error and returns false
                Console.WriteLine("No time availability due to time constraints for specific professor!");
                return Json(new { success = false });
            }
            // finally; after all those checks we can create a new class
            var newClass = new Class
            {   //sets each field properly
                Listing = course.CatalogId,
                Season = season,
                Year = (ushort)year,
                Location = location,
                StartTime = new TimeOnly(start.Hour, start.Minute, start.Second),
                EndTime = new TimeOnly(end.Hour, end.Minute, end.Second),
                TaughtBy = instructor,
            };
            //adds new class to database and save new changes
            db.Classes.Add(newClass);
            db.SaveChanges();
            //since we did add a new class then returns true
            return Json(new { success = true });
        }
    }
}

