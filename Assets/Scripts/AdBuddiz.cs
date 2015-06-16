using UnityEngine;

public class AdBuddiz : MonoBehaviour {
	
	void Start() // Init SDK on app start
	{ 

		#if UNITY_EDITOR

		FindObjectOfType<GameManager>().StartGame();

		#else

		AdBuddizBinding.SetLogLevel(AdBuddizBinding.ABLogLevel.Info);         // log level
		AdBuddizBinding.SetAndroidPublisherKey("d233bc19-a4c2-4e39-9ca3-2dc2f27e6ec9"); // replace with your Android app publisher key
		AdBuddizBinding.SetIOSPublisherKey("TEST_PUBLISHER_KEY_IOS");         // replace with your iOS app publisher key
		AdBuddizBinding.SetTestModeActive();                                  // to delete before submitting to store
		AdBuddizBinding.CacheAds();                                           // start caching ads
	
	
		AdBuddizBinding.ShowAd();

#endif

	}

	public void OnGUI()
	{
	}
	
	void OnEnable()
	{
		// Listen to AdBuddiz events
		AdBuddizManager.didFailToShowAd += DidFailToShowAd;
		AdBuddizManager.didCacheAd += DidCacheAd;
		AdBuddizManager.didShowAd += DidShowAd;
		AdBuddizManager.didClick += DidClick;
		AdBuddizManager.didHideAd += DidHideAd;
	}
	
	void OnDisable()
	{
		// Remove all event handlers
		AdBuddizManager.didFailToShowAd -= DidFailToShowAd;
		AdBuddizManager.didCacheAd -= DidCacheAd;
		AdBuddizManager.didShowAd -= DidShowAd;
		AdBuddizManager.didClick -= DidClick;
		AdBuddizManager.didHideAd -= DidHideAd;
	}
	
	void DidFailToShowAd(string adBuddizError) {
		AdBuddizBinding.LogNative("DidFailToShowAd: " + adBuddizError);
		Debug.Log ("Unity: DidFailToShowAd: " + adBuddizError);
		FindObjectOfType<GameManager>().StartGame();
	}
	
	void DidCacheAd() {
		AdBuddizBinding.LogNative("DidCacheAd");
		Debug.Log ("Unity: DidCacheAd");
	}

	void DidShowAd() {
		AdBuddizBinding.LogNative("DidShowAd");
		Debug.Log ("Unity: DidShowAd");
	}
	
	void DidClick() {
		AdBuddizBinding.LogNative("DidClick");
		Debug.Log ("Unity: DidClick");
	}
	
	void DidHideAd() {
		AdBuddizBinding.LogNative("DidHideAd");
		FindObjectOfType<GameManager>().StartGame();
	}
}
