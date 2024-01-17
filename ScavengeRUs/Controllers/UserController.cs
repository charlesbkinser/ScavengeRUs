using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScavengeRUs.Models.Entities;
using ScavengeRUs.Models.Enums;
using ScavengeRUs.Services;
using System;
using System.Security.Claims;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Net;
using System.Drawing.Printing;





namespace ScavengeRUs.Controllers
{

    /// <summary>
    /// Anything in this controller (www.localhost.com/users) can only be viewed by Admin
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly Functions _functions;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ILogger<UserController> _logger;
        string defaultPassword = "Etsupass12!";

        /// <summary>
        /// This is the dependecy injection for the User Repository that connects to the database
        /// </summary>
        /// <param name="userRepo"></param>
        public UserController(UserManager<ApplicationUser> userManager, IUserRepository userRepo, IConfiguration configuration, ILogger<UserController> logger)
        {
            _userRepo = userRepo;
            _functions = new Functions(configuration);
            _logger = logger;
            _userManager = userManager;
        }
        /// <summary>
        /// This is the landing page for www.localhost.com/user/manage aka "Admin Portal"
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Manage(string searchString)
        {
            var users = await _userRepo.ReadAllAsync(); //Reads all the users in the db

            //if the admin didn't search for anything just return all the users
            if (string.IsNullOrEmpty(searchString))
                return View(users);  //Right click and go to view to see HTML

            //this line of code filters out all the users whose emails and phone numbers do not
            //contain the search string
            var searchResults = users.Where(user => user.Email.Contains(searchString)
            || !string.IsNullOrEmpty(user.PhoneNumber) && user.PhoneNumber.Contains(searchString));

            return View(searchResults);
        }
        /// <summary>
        /// This is the HtmlGet landing page for editing a User
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit([Bind(Prefix = "id")] string username)
        {
            //await _functions.SendEmail("4239006885@txt.att.net", "Hello, from ASP.NET", "Body");
            //await CreateUsers("testdata.csv");
            var user = await _userRepo.ReadAsync(username);
            return View(user);
        }
        /// <summary>
        /// This is the method that executes when hitting the submit button on a edit user form.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Edit(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                await _userRepo.UpdateAsync(user.Id, user);
                return RedirectToAction("Manage");
            }
            return View(user);
        }
        /// <summary>
        /// This is the landing page to delete a user aka "Are you sure you want to delete user X?"
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete([Bind(Prefix = "id")] string username)
        {
            var user = await _userRepo.ReadAsync(username);
            if (user == null)
            {
                return RedirectToAction("Manage");
            }
            return View(user);
        }
        /// <summary>
        /// This is the method that gets executed with hitting submit on deleteing a user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        /// 
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed([Bind(Prefix = "id")] string username)
        {
            await _userRepo.DeleteAsync(username);
            return RedirectToAction("Manage");
        }
        /// <summary>
        /// This is the landing page for viewing the details of a user (www.localhost.com/user/details/{username}
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details([Bind(Prefix = "id")] string username)
        {
            var user = await _userRepo.ReadAsync(username);

            return View(user);
        }
        /// <summary>
        /// This is the landing page to create a new user from the admin portal
        /// </summary>
        /// <returns></returns>

        public IActionResult Create()
        {
            return View();
        }
        /// <summary>
        /// This is the method that is executed when hitting submit on creating a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                user.UserName = user.Email;
                await _userRepo.CreateAsync(user, defaultPassword);
                return RedirectToAction("Details", new { id = user.UserName });
            }
            return View(user);

        }
        /// <summary>
        /// This is the profile page for a user /user/profile/
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public async Task<IActionResult> Profile()
        {
            var currentUser = await _userRepo.ReadAsync(User.Identity?.Name!);
            return View(currentUser);
        }

        /// <summary>
        /// HTTP GET action to display the CreateUsers view.
        /// </summary>
        /// <returns>The CreateUsers view.</returns>
        [HttpGet]
        public IActionResult CreateUsers()
        {
            return View();
        }


        /// <summary>
        /// HTTP POST action to create users from an uploaded CSV file.
        /// </summary>
        /// <param name="uploadedFile">The CSV file containing user data.</param>
        /// <returns>A JSON result indicating the success or failure of user creation.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateUsers(IFormFile uploadedFile)
        {
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                return Json(new { success = false, message = "Please upload a CSV file." });
            }

            var errorList = new List<string>();

            try
            {
                using (var stream = new StreamReader(uploadedFile.OpenReadStream()))
                {
                    string content = await stream.ReadToEndAsync();
                    var users = ParseCSVToUsers(content);

                    foreach (var user in users)
                    {
                        var result = await _userManager.CreateAsync(user, defaultPassword);
                        if (!result.Succeeded)
                        {
                            errorList.Add($"Error creating user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                }

                if (errorList.Any())
                {
                    return Json(new { success = false, message = "Some users could not be created.", errors = errorList });
                }
                else
                {
                    return Json(new { success = true, message = "All users created successfully." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing uploaded file.");
                return Json(new { success = false, message = "An error occurred while processing the file." });
            }
        }

        /// <summary>
        /// Parses a CSV string to create a list of ApplicationUser objects.
        /// </summary>
        /// <param name="csvContent">The CSV content as a string.</param>
        /// <returns>A list of ApplicationUser objects.</returns>

        private List<ApplicationUser> ParseCSVToUsers(string csvContent)
        {
            var users = new List<ApplicationUser>();
            var lines = csvContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var fields = line.Split(',');

                if (fields.Length == 5 && fields.All(field => !string.IsNullOrWhiteSpace(field)))
                {
                    var sanitizedEmail = WebUtility.HtmlEncode(fields[3].Trim());
                    var sanitizedPhone = WebUtility.HtmlEncode(fields[2].Trim());

                    if (!IsValidEmail(sanitizedEmail) || !IsValidPhoneNumber(sanitizedPhone))
                    {
                        continue; // Skip invalid entries
                    }

                    if (!Enum.TryParse<Carriers>(fields[4].Trim(), true, out var carrier))
                    {
                        continue; // Skip if carrier parsing fails
                    }

                    var user = new ApplicationUser
                    {
                        FirstName = fields[0].Trim(),
                        LastName = fields[1].Trim(),
                        PhoneNumber = sanitizedPhone,
                        Email = sanitizedEmail,
                        UserName = sanitizedEmail, // Assuming email as username
                        Carrier = carrier
                    };

                    users.Add(user);
                }
            }

            return users;
        }

        /// <summary>
        /// Validates an email address format.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email is valid, false otherwise.</returns>

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Validates a US phone number format.
        /// </summary>
        /// <param name="number">The phone number to validate.</param>
        /// <returns>True if the phone number is valid, false otherwise.</returns>
        private bool IsValidPhoneNumber(string number)
        {
            return Regex.IsMatch(number, @"^\d{10}$"); // US phone number format
        }



























    }
}
