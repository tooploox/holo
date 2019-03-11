using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;
using Microsoft.MixedReality.Toolkit.Core.Services;
using Microsoft.MixedReality.Toolkit.Core.Definitions;
using System.Reflection;

/* TODO: Work-in-progress, doesn't work yet. I tried numerous ways to disable DictationSystem at runtime,
 * but none works now.
 */

public class CustomizeMrtk : MonoBehaviour
{
    //public Microsoft.MixedReality.Toolkit.Core.Definitions.BaseMixedRealityProfile profileDefault;
    //public Microsoft.MixedReality.Toolkit.Core.Definitions.MixedRealityToolkitConfigurationProfile profileWithoutDictation;
    public MixedRealityRegisteredServiceProvidersProfile profileWithoutDictation;

    void Awake()
    {
        /* Avoid filling the console with NullPointerExceptions in case speech recognition is not supported
         * 
         * NullReferenceException: Object reference not set to an instance of an object
         * Microsoft.MixedReality.Toolkit.Providers.WindowsVoiceInput.WindowsDictationInputProvider.Update () (at Assets/MixedRealityToolkit.Providers/WindowsVoiceInput/WindowsDictationInputProvider.cs:113)
         * Microsoft.MixedReality.Toolkit.Core.Services.MixedRealityToolkit.UpdateAllServices () (at Assets/MixedRealityToolkit/Services/MixedRealityToolkit.cs:966)
         * Microsoft.MixedReality.Toolkit.Core.Services.MixedRealityToolkit.Update () (at Assets/MixedRealityToolkit/Services/MixedRealityToolkit.cs:570)
         */
        if (!PhraseRecognitionSystem.isSupported)
        {
            MixedRealityToolkit mrtk = GetComponent<MixedRealityToolkit>();

            // This seems like a cleaner solution, but it doesn't work unfortunately.
            //mrtk.GetService<Microsoft.MixedReality.Toolkit.Core.Interfaces.Devices.IMixedRealityDictationSystem>().Disable();

            // This doesn't compile, since RegisteredServiceProvidersProfile is read-only.
            //mrtk.ActiveProfile.RegisteredServiceProvidersProfile = profileWithoutDictation;

            //var field = typeof(MixedRealityToolkitConfigurationProfile).GetField("registeredServiceProvidersProfile", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
            //field.SetValue(mrtk.ActiveProfile, profileWithoutDictation);
        }
    }
}
