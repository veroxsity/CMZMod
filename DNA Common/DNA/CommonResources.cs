using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace DNA
{
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[CompilerGenerated]
	internal class CommonResources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(resourceMan, null))
				{
					ResourceManager resourceManager = new ResourceManager("DNA.CommonResources", typeof(CommonResources).Assembly);
					resourceMan = resourceManager;
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal static string Address_not_available
		{
			get
			{
				return ResourceManager.GetString("Address_not_available", resourceCulture);
			}
		}

		internal static string Awards
		{
			get
			{
				return ResourceManager.GetString("Awards", resourceCulture);
			}
		}

		internal static string Cancel
		{
			get
			{
				return ResourceManager.GetString("Cancel", resourceCulture);
			}
		}

		internal static string Couldn_t_get_local_address
		{
			get
			{
				return ResourceManager.GetString("Couldn_t_get_local_address", resourceCulture);
			}
		}

		internal static string Error
		{
			get
			{
				return ResourceManager.GetString("Error", resourceCulture);
			}
		}

		internal static string Error_getting_address_dyndns_returned
		{
			get
			{
				return ResourceManager.GetString("Error_getting_address_dyndns_returned", resourceCulture);
			}
		}

		internal static string Invalid_Login
		{
			get
			{
				return ResourceManager.GetString("Invalid_Login", resourceCulture);
			}
		}

		internal static string Invalid_username_or_password_
		{
			get
			{
				return ResourceManager.GetString("Invalid_username_or_password_", resourceCulture);
			}
		}

		internal static string LauncherControl_ValidateLicenseFacebook_There_was_a_problem_authenticating_your_facebook_account_
		{
			get
			{
				return ResourceManager.GetString("LauncherControl_ValidateLicenseFacebook_There_was_a_problem_authenticating_your_facebook_account_", resourceCulture);
			}
		}

		internal static string No_network_is_available
		{
			get
			{
				return ResourceManager.GetString("No_network_is_available", resourceCulture);
			}
		}

		internal static string Not_connected_to_internet
		{
			get
			{
				return ResourceManager.GetString("Not_connected_to_internet", resourceCulture);
			}
		}

		internal static string Off
		{
			get
			{
				return ResourceManager.GetString("Off", resourceCulture);
			}
		}

		internal static string OK
		{
			get
			{
				return ResourceManager.GetString("OK", resourceCulture);
			}
		}

		internal static string On
		{
			get
			{
				return ResourceManager.GetString("On", resourceCulture);
			}
		}

		internal static string There_was_an_error_
		{
			get
			{
				return ResourceManager.GetString("There_was_an_error_", resourceCulture);
			}
		}

		internal static string Unsupported_Hardware
		{
			get
			{
				return ResourceManager.GetString("Unsupported_Hardware", resourceCulture);
			}
		}

		internal static string We_re_sorry_but_your_video_hardware_is_not_currently_supported__We_are_currently_working_on_supporting_more_hardware__and_we_will_have_a_solution_for_you_soon_
		{
			get
			{
				return ResourceManager.GetString("We_re_sorry_but_your_video_hardware_is_not_currently_supported__We_are_currently_working_on_supporting_more_hardware__and_we_will_have_a_solution_for_you_soon_", resourceCulture);
			}
		}

		internal CommonResources()
		{
		}
	}
}
