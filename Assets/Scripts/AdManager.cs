using UnityEngine;

public class AdManager : MonoBehaviour
{

    public Time timeToDisplayAd;
    public int DisplayNumber = 0;

    public void TryToStartGame()
    {
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.StartGame();
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        // PlayerPrefs.SetFloat ("ShowAd", Time.time);
    }

    void Start()
    {
#if UNITY_EDITOR

        TryToStartGame();
        return;

#else

				Initialize ();

#endif

    }

    public void DisplayAd()
    {
        /*
        if (DisplayNumber < 3)
        {
            
        }
        else if (!PlayAVideo("vz2c57d5ac18b7471487"))
        {


            TryToStartGame();

            
                        var lastAd = PlayerPrefs.GetFloat ("ShowAd", Time.time); 	
                        if (Time.time - lastAd >= 80) {
				
                                AdBuddizBinding.ShowAd ();
                                return;
				
                        } else {
                        }
                        

        }
    */
        ++DisplayNumber;
        TryToStartGame();

    }

    public void Initialize()
    {

        FindObjectOfType<GameManager>().StartGame();

        // Assign any AdColony Delegates before calling Configure
        // AdColony.OnVideoFinished = this.OnVideoFinished;

        // If you wish to use a the customID feature, you should call  that now.
        // Then, configure AdColony:
        /*	
        AdColony.Configure
        (
            "com.aclick.infinitedigger", // Arbitrary app version and Android app store declaration.
            "app0cac6af8e63b4d159d",   // ADC App ID from adcolony.com
            "vz2c57d5ac18b7471487" // A zone ID from adcolony.com
            );
         * */
    }

    /*
        public bool PlayAVideo (string zoneID)
        {
                if (AdColony.IsVideoAvailable (zoneID)) {
                        AdColony.ShowVideoAd (zoneID); 
                        return true;
                } else {
                        return false;
                }
        }
    */

    /*
        private void OnVideoFinished (bool ad_was_shown)
        {
				
        }
    */

    /*
        void OnEnable ()
        {
                // Listen to AdBuddiz events
                AdBuddizManager.didFailToShowAd += DidFailToShowAd;
                AdBuddizManager.didCacheAd += DidCacheAd;
                AdBuddizManager.didShowAd += DidShowAd;
                AdBuddizManager.didClick += DidClick;
                AdBuddizManager.didHideAd += DidHideAd;
        }
	
        void OnDisable ()
        {
                AdBuddizManager.didFailToShowAd -= DidFailToShowAd;
                AdBuddizManager.didCacheAd -= DidCacheAd;
                AdBuddizManager.didShowAd -= DidShowAd;
                AdBuddizManager.didClick -= DidClick;
                AdBuddizManager.didHideAd -= DidHideAd;
        }
	
        void DidFailToShowAd (string adBuddizError)
        {
                AdBuddizBinding.LogNative ("DidFailToShowAd: " + adBuddizError);
                Debug.Log ("Unity: DidFailToShowAd: " + adBuddizError);
                FindObjectOfType<GameManager> ().StartGame ();
        }
	
        void DidCacheAd ()
        {
                AdBuddizBinding.LogNative ("DidCacheAd");
                Debug.Log ("Unity: DidCacheAd");
        }

        void DidShowAd ()
        {
                PlayerPrefs.SetFloat ("adbuddiz", Time.time);
                Debug.Log ("Unity: DidShowAd");
        }
	
        void DidClick ()
        {
                AdBuddizBinding.LogNative ("DidClick");
                Debug.Log ("Unity: DidClick");
        }
	
        void DidHideAd ()
        {
                AdBuddizBinding.LogNative ("DidHideAd");
                FindObjectOfType<GameManager> ().StartGame ();
        }
        */
}
