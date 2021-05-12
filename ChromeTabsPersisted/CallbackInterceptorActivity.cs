using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Demo.Mobile.Droid.Identity;

namespace Demo.Mobile.Droid
{
    [Activity(NoHistory = false, LaunchMode = LaunchMode.SingleTask)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "com.yourapp.mobile")]
    public class CallbackInterceptorActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Trigger the task completion flow to send off to OIDC
            ChromeCustomTabsWebView.OnResume(Intent);

            Finish();
        }
    }
}
