@interface UniRateiOS : NSObject<UIAlertViewDelegate>

@end

@implementation UniRateiOS

+ (UniRateiOS *)shared
{
    static dispatch_once_t once;
    static UniRateiOS *instance;
    dispatch_once(&once, ^ { instance = [[UniRateiOS alloc] init]; });
    return instance;
}

- (void) showRateAlertTitle:(NSString *)aTitle
                        msg:(NSString *)aMsg
            rateButtonTitle:(NSString *)aRateTitle
          cancelButtonTitle:(NSString *)aCancelTitle
          remindButtonTitle:(NSString *)aRemindTitle
{
    NSString *appName =[[NSBundle mainBundle] objectForInfoDictionaryKey:@"CFBundleDisplayName"];
    
    NSString *title = aTitle.length ? aTitle : [NSString stringWithFormat:@"Rate %@",appName];
    NSString *msg = aMsg.length ? aMsg : [NSString stringWithFormat:@"If you enjoy %@, would you mind taking a moment to rate it? It won’t take more than a minute. Thanks for your support!",appName];
    NSString *rateTitle = aRateTitle.length ? aRateTitle : @"Rate It Now";
    NSString *cancelTitle = aCancelTitle.length ? aCancelTitle : @"No, Thanks";
    NSString *remindTitle = aRemindTitle.length ? aRemindTitle : @"Remind Me Later";
    
    UIAlertView *view = [[UIAlertView alloc] initWithTitle:title message:msg delegate:self cancelButtonTitle:cancelTitle otherButtonTitles:rateTitle,remindTitle,nil];
    [view show];
}

-(void)alertView:(UIAlertView *)alertView clickedButtonAtIndex:(NSInteger)buttonIndex
{
    if (buttonIndex == alertView.cancelButtonIndex) {
        UnitySendMessage("UniRateManager", "UniRateUserDeclinedPrompt", "");
    } else if (buttonIndex == 1) {
        UnitySendMessage("UniRateManager", "UniRateUserWantToRate", "");
    } else if (buttonIndex == 2) {
        UnitySendMessage("UniRateManager", "UniRateUserWantRemind", "");
    }
}

@end

// Converts C style string to NSString
NSString* UniRateMakeNSString (const char* string)
{
	if (string)
		return [NSString stringWithUTF8String: string];
	else
		return [NSString stringWithUTF8String: ""];
}

// Helper method to create C string copy
char* UniRateMakeCString(NSString *str)
{
    const char* string = [str UTF8String];
	if (string == NULL)
		return NULL;
	
	char* res = (char*)malloc(strlen(string) + 1);
	strcpy(res, string);
	return res;
}


extern "C" {
    void _ShowPrompt(const char* title, 
                    const char* msg, 
                    const char* rateTitle, 
                    const char* canceltitle, 
                    const char* remindTitle) {
        [[UniRateiOS shared] showRateAlertTitle:UniRateMakeNSString(title) msg:UniRateMakeNSString(msg) rateButtonTitle:UniRateMakeNSString(rateTitle) cancelButtonTitle:UniRateMakeNSString(canceltitle) remindButtonTitle:UniRateMakeNSString(remindTitle)];
    }

    const char * _GetAppStoreCountry() {
        return UniRateMakeCString([[NSLocale currentLocale] objectForKey:NSLocaleCountryCode]);
    }
    
    const char * _GetApplicationName() {
        NSString *applicationName = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"CFBundleDisplayName"];
        if (applicationName.length == 0) {
            applicationName = [[NSBundle mainBundle] objectForInfoDictionaryKey:(NSString *)kCFBundleNameKey];
        }
        return UniRateMakeCString(applicationName);
    }
    
    const char * _GetApplicationVersion() {
        NSString *applicationVersion = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"CFBundleShortVersionString"];
        if (applicationVersion.length == 0) {
            applicationVersion = [[NSBundle mainBundle] objectForInfoDictionaryKey:(NSString *)kCFBundleVersionKey];
        }
        return UniRateMakeCString(applicationVersion);
    }
    
    const char * _GetApplicationBundleID() {
        return UniRateMakeCString([[NSBundle mainBundle] bundleIdentifier]);
    }
	
}

