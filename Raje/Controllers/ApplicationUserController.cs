﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Raje.Data;
using Raje.Models;
using Raje.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Account.Internal;

namespace Raje.Controllers
{
    public class ApplicationUserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationUserController
        (
            ApplicationDbContext db, 
            IWebHostEnvironment webHostEnvironment,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager
        )
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public IActionResult Index()
        {
            IEnumerable<ApplicationUser> users = new List<ApplicationUser>();

            users = _db.ApplicationUser.ToList();

            return View(users);
        }

        //GET - UPSERT
        public IActionResult Upsert(String? id)
        {
            if (id == null)
            {
                ApplicationUserViewModel userNovo = new ApplicationUserViewModel();
                //this is for create
                return View(userNovo);
            }
            else
            {
                var user = _db.ApplicationUser.Find(id);

                if (user == null)
                {
                    return NotFound();
                }

                ApplicationUserViewModel userNovo = new ApplicationUserViewModel()
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    Birthdate = user.Birthdate,
                    City = user.City,
                    State = user.State,
                    ImagemURL = user.ImagemURL

                };

                return View(userNovo);
            }
        }

        //POST - UPSERT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<IActionResult> UpsertAsync(ApplicationUserViewModel user)
        {
            string returnUrl = Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ApplicationUser userInserir = new ApplicationUser();

            if (user.Id != null)
            {
                userInserir = _db.ApplicationUser.Find(user.Id);
            }
            else
            {
                userInserir.UserName = user.Email;
                userInserir.Email = user.Email;
            }
                
            userInserir.FullName = user.FullName;
            userInserir.PhoneNumber = user.PhoneNumber;
            userInserir.Birthdate = user.Birthdate;
            userInserir.City = user.City;
            userInserir.State = user.State;

            if (user.ImagemUpload != null)
            {
                var imgPrefixo = Guid.NewGuid() + "_";

                if (!Util.Util.UploadArquivo(user.ImagemUpload, imgPrefixo))
                {
                    return View(user);
                }
                userInserir.ImagemURL = imgPrefixo + user.ImagemUpload.FileName;
            }

       
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnvironment.WebRootPath;

                if (user.Id == null)
                {
                    //Creating
                    var result = await _userManager.CreateAsync(userInserir, user.Password);

                    if (result.Succeeded)
                    {
                        if (User.IsInRole(WC.AdminRole))
                        {
                            //an admin has logged in and they try to create a new user
                            await _userManager.AddToRoleAsync(userInserir, WC.AdminRole);
                        }
                        else
                        {
                            await _userManager.AddToRoleAsync(userInserir, WC.CustomerRole);
                        }

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(userInserir);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(userInserir.Email, "Confirme seu email",
                            $"Por favor, confirme sua conta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui</a>.");

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = userInserir.Email, returnUrl = returnUrl });
                        }
                        else
                        {
                            if (!User.IsInRole(WC.AdminRole))
                            {
                                await _signInManager.SignInAsync(userInserir, isPersistent: false);
                            }
                            else
                            {
                                return RedirectToAction("Index");
                            }
                            return LocalRedirect(returnUrl);
                        }
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                else
                {
                    //updating
                    _db.ApplicationUser.Update(userInserir);
                    _db.SaveChanges();
                }

            return LocalRedirect(returnUrl);
        }

        //GET - Details
        public IActionResult Details(String? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ApplicationUser user = _db.ApplicationUser.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        //GET - DELETE
        public IActionResult Delete(String? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ApplicationUser user = _db.ApplicationUser.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(String? id)
        {
            var obj = _db.ApplicationUser.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            _db.ApplicationUser.Remove(obj);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}