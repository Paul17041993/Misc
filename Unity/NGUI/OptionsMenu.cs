using UnityEngine;
using System.Collections;
using System;

public class OptionsMenu : MonoBehaviour
{
    public static OptionsMenu Instance { get; private set; }

    void Awake() { Instance = this; }


    public GameObject menu;
    public GameObject menuAudio;
    public GameObject menuVideo;

    public char multiRes = 'X';
    public char multiAA = 'X';
    public string novalAA = "NONE";

    /// <summary>
    /// object to set to active when exiting the menu
    /// </summary>
    public GameObject lastObject = null;

    /// <summary>
    /// set the menu active or not
    /// </summary>
    /// <param name="val">true or false</param>
    public void MenuActive(bool val)
    {
        if (menu) menu.SetActive(val);

        if (val)
        {
            if (menuAudio) menuAudio.SetActive(false);
            if (menuVideo) menuVideo.SetActive(false);
        }
    }

    /// <summary>
    /// alternate to boolean version, however takes an object, 
    /// sets this as the last object and activates if it's not null
    /// </summary>
    /// <param name="lastObj">object to set lastObject to</param>
    public void MenuActive(GameObject lastObj)
    {
        lastObject = lastObj;
        MenuActive(lastObject != null);
    }


    // main menu

    public void OnAudio()
    {
        if (menu) menu.SetActive(false);
        if (menuAudio) menuAudio.SetActive(true);

        GetAudioVals();
    }

    public void OnVideo()
    {
        if (menu) menu.SetActive(false);
        if (menuVideo) menuVideo.SetActive(true);

        GetVideoVals();
    }

    public void OnBack()
    {
        if (menu) menu.SetActive(false);
        if (menuAudio) menuAudio.SetActive(false);
        if (menuVideo) menuVideo.SetActive(false);

        if (lastObject)
        {
            lastObject.SetActive(true);
            PlayMakerFSM pm = lastObject.GetComponent<PlayMakerFSM>();
            if (pm != null) pm.SendEvent("Back");
        }
    }


    // sub menus

    public void OnCancel()
    {
        if (menuAudio) if (menuAudio.activeSelf)
                RevertAudioVals();

        if (menu) menu.SetActive(true);
        if (menuAudio) menuAudio.SetActive(false);
        if (menuVideo) menuVideo.SetActive(false);
    }

    public void OnApply()
    {
        if (menuAudio) if (menuAudio.activeSelf)
                ApplyAudioVals();

        if (menuVideo) if (menuVideo.activeSelf)
                ApplyVideoVals();

        if (menu) menu.SetActive(true);
        if (menuAudio) menuAudio.SetActive(false);
        if (menuVideo) menuVideo.SetActive(false);

        //ConfigurationManager.SaveConfigurationFile();
        ConfigurationManager.MarkForSave();
    }


    // audio

    public UISlider volMain;
    public UISlider volMusic;
    public UISlider volVoice;
    public UISlider volEffects;

    float pvMa, pvMu, pvVo, pvEf;

    void GetAudioVals()
    {
        if (volMain) pvMa = volMain.value = AudioManager.volumeMain;
        if (volMusic) pvMu = volMusic.value = AudioManager.volumeMusic;
        if (volVoice) pvVo = volVoice.value = AudioManager.volumeVoice;
        if (volEffects) pvEf = volEffects.value = AudioManager.volumeEffects;
    }

    public void UpdateAudioVals()
    {
        if (volMain) AudioManager.volumeMain = volMain.value;
        if (volMusic) AudioManager.volumeMusic = volMusic.value;
        if (volVoice) AudioManager.volumeVoice = volVoice.value;
        if (volEffects) AudioManager.volumeEffects = volEffects.value;
    }

    void ApplyAudioVals()
    {
        if (volMain) AudioManager.volumeMain = volMain.value;
        if (volMusic) AudioManager.volumeMusic = volMusic.value;
        if (volVoice) AudioManager.volumeVoice = volVoice.value;
        if (volEffects) AudioManager.volumeEffects = volEffects.value;
    }

    void RevertAudioVals()
    {
        if (volMain) AudioManager.volumeMain = pvMa;
        if (volMusic) AudioManager.volumeMusic = pvMu;
        if (volVoice) AudioManager.volumeEffects = pvVo;
        if (volEffects) AudioManager.volumeEffects = pvEf;
    }


    // video
    public ValueSelection resolution;
    public ValueSelection fullscreen;
    public ValueSelection antialiasing;
    public ValueSelection quality;


    void GetVideoVals()
    {
        //refresh eeeeverything just to be sure, shouldnt cause issues

        // res
        if (resolution)
        {
            resolution.words.Clear();
            int i = 0, v = 0;
            foreach (Resolution res in Screen.resolutions)
            {
                resolution.words.Add(res.width + " " + multiRes + " " + res.height);

                if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
                    v = i;

                ++i;
            }

            resolution.value = v;

        }

        // fulsc
        if (fullscreen)
            fullscreen.value = Screen.fullScreen? 1 : 0;

        // AA
        if (antialiasing)
        {
            antialiasing.words.Clear();
            antialiasing.words.Add(novalAA);
            antialiasing.words.Add(multiAA + "2");
            antialiasing.words.Add(multiAA + "4");
            antialiasing.words.Add(multiAA + "8");
            switch(QualitySettings.antiAliasing)
            {
                case 0: antialiasing.value = 0; break;
                case 2: antialiasing.value = 1; break;
                case 4: antialiasing.value = 2; break;
                case 8: antialiasing.value = 3; break;
                default: antialiasing.value = 0; break;
            }
        }

        // quality
        if (quality)
            quality.value = QualitySettings.GetQualityLevel();

    }

    void ApplyVideoVals()
    {
        // res and fullscreen
        if (resolution && fullscreen)
        {
            string sres = resolution.words[resolution.value];
            int i = sres.IndexOf(multiRes);

            string sw = sres.Substring(0, i - 1);
            string sh = sres.Substring(i+2);

            try { Screen.SetResolution(int.Parse(sw), int.Parse(sh), fullscreen.value == 1); }
            catch (Exception e)
            { Debug.LogError("A problem occured setting screen resolution! " + e.Message); }
        }

        // Quality
        if (quality)
            QualitySettings.SetQualityLevel(quality.value);

        // AA, do this after the quality so its not overwritten
        if (antialiasing)
        {
            switch (antialiasing.value)
            {
                case 0: QualitySettings.antiAliasing = 0; break;
                case 1: QualitySettings.antiAliasing = 2; break;
                case 2: QualitySettings.antiAliasing = 4; break;
                case 3: QualitySettings.antiAliasing = 8; break;
                default: Debug.LogError("Invalid AA value! " + antialiasing.value); break;
            }
        }
    }
}
