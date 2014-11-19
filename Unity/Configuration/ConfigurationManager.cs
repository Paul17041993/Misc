
// comment out if you dont have playmaker
#define PLAYMAKER_COMPATIBILITY

using UnityEngine;
using System.Collections;
using System.Xml;
using System;
using System.IO;

public class ConfigurationManager : MonoBehaviour
{
    static ConfigurationManager instance;

    public string filename = "configuration";
    public bool useAppData = false;
    public string subDir = "";

    public float defaultVolumeMain = 1f;
    public float defaultVolumeMusic = 1f;
    public float defaultVolumeEffects = 1f;
    public float defaultVolumeVoice = 1f;

    public int defaultResolutionX = 1280;
    public int defaultResolutionY = 720;
    public bool defaultFullscreen = true;
    public int defaultAA = 4;
    public int defaultQualityIndex = 2;
    public int defaultVsync = 0;
    public int defaultTargetframeRate = 120;

    void Start()
    { instance = this; }

    bool loadFile = true;
    bool saveFile = true;
    void Update()
    {
        if (loadFile)
        {
            //load config this cycle
            LoadConfigurationFile();
            loadFile = false;
            return; // return as we dont want to load and save the same cycle
        }
        if (saveFile)
        {
            //save config this cycle
            SaveConfigurationFile();
            saveFile = false;
            return;
        }
    }

    public static void MarkForLoad()
    {
        if (instance) instance.loadFile = true;
        else Debug.LogError("try call MarkForLoad when uninitialised!");
    }

    public static void MarkForSave()
    {
        if (instance) instance.saveFile = true;
        else Debug.LogError("try call MarkForSave when uninitialised!");
    }


    static string filenameAndDir
    {
        get
        {
            if (instance.useAppData && instance.subDir.Length > 0)
            {
                string path = Environment.ExpandEnvironmentVariables("%appdata%\\" + instance.subDir);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path + "\\" + instance.filename;
            }
            return instance.filename;
        }
    }

    public static void SaveConfigurationFile()
    {
        try
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            XmlWriter writer = XmlWriter.Create(filenameAndDir, settings);
            writer.WriteStartDocument();

            writer.WriteComment("Game configuration data, you shouldn't need to edit this by hand");


            writer.WriteStartElement("Settings");


            //// audio
            writer.WriteStartElement("Audio");

            // vol
            writer.WriteStartElement("Volume");
            writer.WriteAttributeString("Main", AudioManager.volumeMain.ToString());
            writer.WriteAttributeString("Music", AudioManager.volumeMusic.ToString());
            writer.WriteAttributeString("Effects", AudioManager.volumeEffects.ToString());
            writer.WriteAttributeString("Voice", AudioManager.volumeVoice.ToString());
            writer.WriteEndElement();

            writer.WriteEndElement(); //audio


            //// video
            writer.WriteStartElement("Video");

            // res
            writer.WriteStartElement("Resolution");
            writer.WriteAttributeString("X", Screen.currentResolution.width.ToString());
            writer.WriteAttributeString("Y", Screen.currentResolution.height.ToString());
            writer.WriteEndElement();

            // fullsc
            writer.WriteStartElement("Fullscreen");
            writer.WriteAttributeString("Value", Screen.fullScreen.ToString());
            writer.WriteEndElement();

            // AA
            writer.WriteStartElement("AA");
            writer.WriteAttributeString("Value", QualitySettings.antiAliasing.ToString());
            writer.WriteEndElement();

            // quality
            writer.WriteStartElement("Quality");
            writer.WriteAttributeString("Value", QualitySettings.GetQualityLevel().ToString());
            writer.WriteEndElement();

            // framerate
            writer.WriteStartElement("FrameRate");
            writer.WriteAttributeString("Vsync", QualitySettings.vSyncCount.ToString());
            writer.WriteAttributeString("Target", Application.targetFrameRate.ToString());
            writer.WriteEndElement();

            writer.WriteEndElement(); //video


            writer.WriteEndElement(); //settings


            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();

            Debug.Log("Configuration saved sucessfully");
        }
        catch (Exception e)
        {
            Debug.LogWarning("Something went wrong saving configuration data! \n"
                + e.Message + "\n this is bad...");
        }
    }

    public static void LoadConfigurationFile()
    {
        float ma = instance.defaultVolumeMain, mu = instance.defaultVolumeMusic, 
            ef = instance.defaultVolumeEffects, vo = instance.defaultVolumeVoice;
        int rx = instance.defaultResolutionX, ry = instance.defaultResolutionY,
            aa = instance.defaultAA, qu = instance.defaultQualityIndex;
        bool fulsc = instance.defaultFullscreen;
        int vsn = instance.defaultVsync, tfr = instance.defaultTargetframeRate;

        try
        {
            XmlReader reader = XmlReader.Create(filenameAndDir);

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Volume")
                {
                    float.TryParse(reader.GetAttribute("Main"), out ma);
                    float.TryParse(reader.GetAttribute("Music"), out mu);
                    float.TryParse(reader.GetAttribute("Effects"), out ef);
                    float.TryParse(reader.GetAttribute("Voice"), out vo);
                }

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Resolution")
                {
                    int.TryParse(reader.GetAttribute("X"), out rx);
                    int.TryParse(reader.GetAttribute("Y"), out ry);
                }

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Fullscreen")
                    bool.TryParse(reader.GetAttribute("Value"), out fulsc);

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "AA")
                    int.TryParse(reader.GetAttribute("Value"), out aa);

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "Quality")
                    int.TryParse(reader.GetAttribute("Value"), out qu);

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "FrameRate")
                {
                    int.TryParse(reader.GetAttribute("Vsync"), out vsn);
                    int.TryParse(reader.GetAttribute("Target"), out tfr);
                }
            }

            reader.Close();

            Debug.Log("Configuration loaded sucessfully");
        }
        catch (Exception e)
        {
            Debug.LogWarning("Something went wrong loading configuration data! \n"
                + e.Message + "\nDefaults will be used instead...");
        }

        AudioManager.volumeMain = ma;
        AudioManager.volumeMusic = mu;
        AudioManager.volumeEffects = ef;
        AudioManager.volumeVoice = vo;

        if (Screen.currentResolution.width != rx ||
            Screen.currentResolution.height != ry ||
            Screen.fullScreen != fulsc)
            Screen.SetResolution(rx, ry, fulsc);

        if (QualitySettings.GetQualityLevel() != qu)
            QualitySettings.SetQualityLevel(qu);

        if (QualitySettings.antiAliasing != aa)
            QualitySettings.antiAliasing = aa;

        if (QualitySettings.vSyncCount != vsn)
            QualitySettings.vSyncCount = vsn;
        if (Application.targetFrameRate != tfr)
            Application.targetFrameRate = tfr;

    }
}

///////////////////////////////////////////////////////////////////////////////////////////////////////
// playmaker

#if PLAYMAKER_COMPATIBILITY

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.ScriptControl)]
    [Tooltip("Flag configuration to load from file")]
    public class ConfigurationMarkForLoad : FsmStateAction
    {
        public override void OnEnter()
        {
            ConfigurationManager.MarkForLoad();
        }
    }

    [ActionCategory(ActionCategory.ScriptControl)]
    [Tooltip("Flag configuration to save to file")]
    public class ConfigurationMarkForSave : FsmStateAction
    {
        public override void OnEnter()
        {
            ConfigurationManager.MarkForSave();
        }
    }
}

#endif
