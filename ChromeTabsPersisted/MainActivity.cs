namespace PersistedWebAuthenticatorExample
{
   [Activity(Label = "Your App", LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnResume()
        {
            base.OnResume();

            Xamarin.Essentials.Platform.OnResume();
        }
    }
}