mergeInto(LibraryManager.library, 
{
    startAuthentication: function (utf8String) 
    {
        var authRequest = UTF8ToString(utf8String);
        var authorizationRequest = authRequest;
        startAuth(authorizationRequest);
    }
});