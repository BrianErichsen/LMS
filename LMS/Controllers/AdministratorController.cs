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
            var is_department_real = db.Departments.FirstOrDefault(d => d.Subject == subject);
            if (is_department_real != null) {
                return Json(new { success = false});
            }

            // creates new department if not
            var department = new Department
            {
                Subject = subject,
                Name = name
            };
            db.Departments.Add(department);
            db.SaveChanges();
            
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
        {
            var professors = db.Professors.Where(p => p.WorksIn == subject)
            .Select(p => new
            {
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
        {
            var is_course_real = db.Courses.FirstOrDefault(c => c.Department == subject && c.Number == number);
            if (is_course_real != null)
            {
                return Json(new { success = false });
            }
            var course = new Course
            {
                Department = subject,
                Number = (ushort) number,
                Name = name
            };
            db.Courses.Add(course);
            db.SaveChanges();

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
            // Find the course
            var course = db.Courses.FirstOrDefault(c => c.Department == subject && c.Number == number);
            if (course == null)
            {
                Console.WriteLine("ERROR: No such course.");
                return Json(new { success = false });
            }

            
            var existingClass = db.Classes.FirstOrDefault(c =>
                c.ClassId == course.CatalogId &&
                c.Season == season &&
                c.Year == year);

            if (existingClass != null)
            {
                Console.WriteLine("ERROR: Class already exists.");
                return Json(new { success = false });
            }

            
            TimeOnly startTimeOnly = TimeOnly.FromDateTime(start);
            TimeOnly endTimeOnly = TimeOnly.FromDateTime(end);

            
            var classes = db.Classes.Where(c =>
                c.Location == location &&
                c.Season == season &&
                c.Year == year).ToList();

            
            var conflictingClass = classes.FirstOrDefault(c =>
            {
                
                return !(startTimeOnly.CompareTo(c.EndTime) >= 0 || endTimeOnly.CompareTo(c.StartTime) <= 0);
            });

            if (conflictingClass != null)
            {
                Console.WriteLine("ERROR: Same location time conflict.");
                return Json(new { success = false });
            }

            
            var professor = db.Professors.FirstOrDefault(p => p.UId == instructor);
            if (professor == null)
            {
                Console.WriteLine("ERROR: No such professor.");
                return Json(new { success = false });
            }

            
            var conflictingProfessorClass = db.Classes.FirstOrDefault(c =>
                c.TaughtBy == instructor &&
                c.Season == season &&
                c.Year == year &&
                !(startTimeOnly.CompareTo(c.EndTime) >= 0 || endTimeOnly.CompareTo(c.StartTime) <= 0));

            if (conflictingProfessorClass != null)
            {
                Console.WriteLine("ERROR: Same instructor time conflict.");
                return Json(new { success = false });
            }

            
            var classOffering = new Class
            {
                ClassId = course.CatalogId,
                TaughtBy = instructor,
                Year = (ushort)year,
                Season = season,
                StartTime = new TimeOnly(start.Hour, start.Minute, start.Second),
                EndTime = new TimeOnly(end.Hour, end.Minute, end.Second),
                Location = location
            };

            db.Classes.Add(classOffering);
            db.SaveChanges();

            return Json(new { success = true });
        }
        /*******End code to modify********/

    }
}

