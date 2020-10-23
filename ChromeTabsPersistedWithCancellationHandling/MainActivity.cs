namespace PersistedWebAuthenticatorExample
{
   [Activity(Label = "Your App", LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static bool CustomTabIsOpen;
        
        protected override void OnResume()
        {
            base.OnResume();

            Xamarin.Essentials.Platform.OnResume();

            //CallbackInterceptorActivity is never triggered since there is not a callback when a chrome custom tab activity is closed prematurely.
            //This is a hack that mimics that behavior by manually triggering the on resume process when the window is closed prematurely.
            if (CustomTabIsOpen)
            {
                CustomTabIsOpen = false;

                Identity.ChromeCustomTabsWebView.OnResume(null);
            }
        }
    }
}