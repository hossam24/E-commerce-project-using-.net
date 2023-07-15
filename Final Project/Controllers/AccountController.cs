﻿using Final_Project.Models;

using Final_Project.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Text;
using System.Net;
using System.Net.Mail;

namespace Final_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;

        }

        #region Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerVM)
        {
            //Check if there is account with the same email
            var res = await _userManager.FindByEmailAsync(registerVM.Email);
            if (res != null)
            { ModelState.AddModelError("", "Account already exist"); }

            if (ModelState.IsValid)
            {
                ApplicationUser newUser = new ApplicationUser();
                newUser.Address = registerVM.Address;
                newUser.PhoneNumber = registerVM.PhoneNumber;
                newUser.Email = registerVM.Email;
                newUser.UserName = registerVM.UserName;
                newUser.PasswordHash = registerVM.Password;
                newUser.RoleId = _roleManager.Roles.FirstOrDefault(r => r.Name == "User").Id;


                var result = await _userManager.CreateAsync(newUser, registerVM.Password);
                if (result.Succeeded)
                {
                    //Assign to user role
                    await _userManager.AddToRoleAsync(newUser, "User");



                    //create cookie
                    await _signInManager.SignInAsync(newUser, isPersistent: false);
                    return RedirectToAction("Index", "Department");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                }
            }
            return View(registerVM);
        }
        #endregion


        #region Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Department");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel LoginVM)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(LoginVM.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "UserName or Password is wrong");
                }
                else
                {
                    var result = await _signInManager.PasswordSignInAsync(user, LoginVM.Password, LoginVM.RememberMe, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Department");
                    }
                    else
                    { ModelState.AddModelError("", "UserName or Password is wrong"); }
                }
            }
            return View();
        }
        #endregion

        #region Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
        #endregion



    }
}
