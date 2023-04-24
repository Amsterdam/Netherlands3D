function startAuth(authorizationRequest) 
{
    let popup;
  
    window.addEventListener(
        'message', 
        function (e) {
            if (e.source != popup) {
                return;
            }
  
            unityInstance.SendMessage("OAuthCallback", "GetAuthResults", e.data.url);
        }, 
        false
    );
  
    popup = window.open(authorizationRequest, "Authenticate", 'height=300px, width=500px');
    popup.focus();
};
