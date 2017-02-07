using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace bank.web.models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        public bool IsModal { get; set; }

        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
        [Display(Name = "ReturnUrl")]
        public string ReturnUrl { get; set; }
        public string Source { get; set; }
    }

    public class RegisterViewModel
    {
        public bool IsPremium { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        public string StripeToken { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public bool IsModal { get; internal set; }
        public string ReturnUrl { get; set; }
        public string Source { get; set; }

        [Display(Name = "ZIP Code (U.S. Only)")]
        public string ZIP { get; set; }

        [Display(Name = "Last Name")]
        [Required]
        [StringLength(50, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 3)]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string FirstName { get; set; }

        //[Required]
        //[Display(Name = "Role")]
        //public string Role { get; set; }
        //public string SubRole { get; set; }
        //public string SubRoleRisk { get; set; }

        [StringLength(40, ErrorMessage = "'Other' must be at least {2} characters long.", MinimumLength = 4)]
        public string OtherText { get; set; }

        public SelectList LegalOptions
        {
            get
            {
                return new SelectList(
                    new List<SelectListItem>
                    {
                        new SelectListItem { Value = "Business Lawyer" },
                        new SelectListItem { Value = "Real Estate Lawyer" },
                        new SelectListItem { Value = "IP Lawyer" },
                        new SelectListItem { Value = "M&A Lawyer" },
                        new SelectListItem { Value = "Employment & Labor Lawyer" },
                        new SelectListItem { Value = "Finance & Securities Lawyer" },
                        new SelectListItem { Value = "Estate Planning Lawyer" },
                        new SelectListItem { Value = "Tax Lawyer" },
                        new SelectListItem { Value = "Criminal Defense Lawyer" },
                        new SelectListItem { Value = "Bankruptcy Lawyer" },
                        new SelectListItem { Value = "Digital Media & Internet Lawyer" },
                        new SelectListItem { Value = "Paralegal or other legal aide" },
                        new SelectListItem { Value = "Other" }
                    }, "Value", "Value"
                    );
            }
        }


        public SelectList RiskOptions
        {
            get
            {
                return new SelectList(
                    new List<SelectListItem>
                    {
                        new SelectListItem { Value = "Insurance Company" },
                        new SelectListItem { Value = "Financial Institution" },
                        new SelectListItem { Value = "Government Agency" },
                        new SelectListItem { Value = "Other" }
                    }, "Value", "Value"
                    );
            }
        }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        //[Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string Password { get; set; }

        //[DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Code received in email")]
        public string Code { get; set; }

        [Display(Name = "State")]
        public string State { get; set; }

        public bool IsPending()
        {
            return State == "pending";
        }

        public bool IsCompleted()
        {
            return State == "completed";
        }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
