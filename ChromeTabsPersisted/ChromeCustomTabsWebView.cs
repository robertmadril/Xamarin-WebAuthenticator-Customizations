using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using System;
using Android.Support.CustomTabs;
using IdentityModel.OidcClient.Browser;
using Pyx.Core.Abstraction.Keys;
using Xamarin.Essentials;
using Android.Graphics;

namespace Pyx.Mobile.Droid.Identity
{
    public class ChromeCustomTabsWebView : IBrowser
    {
        private static TaskCompletionSource<WebAuthenticatorResult> _tcs = null;
        private static CustomTabsActivityManager _customTabs;
        private static Uri _uri = null;

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
        {

            WebAuthenticatorResult authResult =
                    await AuthenticateAsync(new Uri(options.StartUrl), new Uri(ApiConstants.RedirectUri));

            return new BrowserResult()
            {
                Response = ParseAuthenticatorResult(authResult)
            };
        }

        //The code below previously lived in the Xamarin.Essentials.WebAuthenticator (https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/WebAuthenticator/WebAuthenticator.android.cs)
        //In order to customize for our needs, we needed to bring this into our code, still using Xamarin web authenticator models since those are consistent
        internal static bool OnResume(Intent intent)
        {
            //Make extra certain this global flag is set to false in order to not induce any unexpected app behavior in MainActivity.OnResume()
            MainActivity.CustomTabIsOpen = false;
            // If we aren't waiting on a task, don't handle the url
            if (_tcs?.Task?.IsCompleted ?? true)
                return false;

            if (intent == null)
            {
                _tcs.TrySetCanceled();
                return false;
            }

            try
            {
                var intentUri = new Uri(intent.Data.ToString());

                _tcs?.TrySetResult(new WebAuthenticatorResult(intentUri));

                return true;
            }
            catch (Exception ex)
            {
                _tcs.TrySetException(ex);
                return false;
            }
        }

        private string ParseAuthenticatorResult(WebAuthenticatorResult result)
        {
            string code = result?.Properties["code"];
            string scope = result?.Properties["scope"];
            string state = result?.Properties["state"];
            string sessionState = result?.Properties["session_state"];
            return $"{ApiConstants.RedirectUri}#code={code}&scope={scope}&state={state}&session_state={sessionState}";
        }

        private Task<WebAuthenticatorResult> AuthenticateAsync(Uri url, Uri callbackUrl)
        {
            if (_tcs?.Task != null && !_tcs.Task.IsCompleted)
                _tcs.TrySetCanceled();

            _tcs = new TaskCompletionSource<WebAuthenticatorResult>();
            _tcs.Task.ContinueWith(t =>
            {
                // Cleanup when done
                if (_customTabs != null)
                {
                    _customTabs.CustomTabsServiceConnected -= CustomTabsActivityManager_CustomTabsServiceConnected;

                    try
                    {
                        _customTabs?.Client?.Dispose();
                    }
                    finally
                    {
                        _customTabs = null;
                    }
                }
            });

            _uri = url;

            _customTabs = CustomTabsActivityManager.From(Platform.CurrentActivity);
            _customTabs.CustomTabsServiceConnected += CustomTabsActivityManager_CustomTabsServiceConnected;

            if (!_customTabs.BindService())
            {
                // Fall back to opening the system browser if necessary
                var browserIntent = new Intent(Intent.ActionView, global::Android.Net.Uri.Parse(url.OriginalString));
                Platform.CurrentActivity.StartActivity(browserIntent);
            }

            return _tcs.Task;
        }

        private static void CustomTabsActivityManager_CustomTabsServiceConnected(ComponentName name, CustomTabsClient client)
        {
            var builder = new CustomTabsIntent.Builder(_customTabs.Session)
                    .SetToolbarColor(Color.Rgb(255, 255, 255))
                    .SetShowTitle(true)
                    .EnableUrlBarHiding();

            var customTabsIntent = builder.Build();
            //THESE FLAGS ARE VERY IMPORTANT. PLEASE READ DOCUMENTATION: https://developer.android.com/reference/android/content/Intent
            customTabsIntent.Intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

            var ctx = Platform.CurrentActivity;

            CustomTabsHelper.AddKeepAliveExtra(ctx, customTabsIntent.Intent);

            customTabsIntent.LaunchUrl(ctx, global::Android.Net.Uri.Parse(_uri.OriginalString));
        }
    }
}