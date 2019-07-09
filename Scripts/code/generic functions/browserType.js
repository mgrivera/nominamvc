
function browserType() {
    // para regresar el browser que el usuario usó para abrir la aplicación 

    // Opera 8.0+ (UA detection to detect Blink/v8-powered Opera)
    var isOpera = !!window.opera || navigator.userAgent.indexOf(' OPR/') >= 0;
    // Firefox 1.0+
    var isFirefox = typeof InstallTrigger !== 'undefined';
    // Safari 3.0+
    var isSafari = Object.prototype.toString.call(window.HTMLElement).indexOf('Constructor') > 0 || (function (p) { return p.toString() === "[object SafariRemoteNotification]"; })(!window['safari'] || safari.pushNotification);
    // Internet Explorer 6-11
    var isIE = /*@cc_on!@*/false || !!document.documentMode;
    // Edge 20+
    var isEdge = !isIE && !!window.StyleMedia;
    // Chrome 1+
    var isChrome = !!window.chrome && !!window.chrome.webstore;
    // Blink engine detection
    var isBlink = (isChrome || isOpera) && !!window.CSS;


    if (isOpera) return "opera"; 

    if (isFirefox) return "fireFox";  

    if (isSafari) return "safari";  

    if (isIE) return "ie";  

    if (isEdge) return "edge";  

    if (isChrome) return "chrome";  
 
    if (isBlink) return "blink";

    return "undefined"; 
}