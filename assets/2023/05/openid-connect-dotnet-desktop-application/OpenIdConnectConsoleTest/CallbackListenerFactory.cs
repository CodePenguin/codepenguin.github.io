namespace OpenIdConnectConsoleTest;

public static class CallbackListenerFactory
{
    public static ICallbackListener GetCallbackListener()
    {
        return new HttpCallbackListener();
    }
}
