
///////////////////////////////////////////////////////////////////////////////////////////////////////
// Extensive Unity Audio manager, Developed by Paul Hancock
///////////////////////////////////////////////////////////////////////////////////////////////////////

// comment out if you dont have playmaker
#define PLAYMAKER_COMPATIBILITY

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    static AudioManager instance;
    public static bool Initialised
    { get { return instance; } set { } }

    /// <summary>
    /// the master volume
    /// </summary>
    public float volMain = 1f;
    /// <summary>
    /// the effects volume
    /// </summary>
    public float volEffects = 1f;
    /// <summary>
    /// the voice volume
    /// </summary>
    public float volVoice = 1f;
    /// <summary>
    /// the music volume
    /// </summary>
    public float volMusic = 1f;

    /// <summary>
    /// object that audiosource children will be attached to for centralised audio
    /// commonly the camera, otherwise if left null will use the pool
    /// </summary>
    public GameObject centralAudioObject;

    /// <summary>
    /// name to use for the pool
    /// </summary>
    public string sourcePoolName = "AudioSource Pool";

    /// <summary>
    /// attach the pool to the central object?
    /// </summary>
    public bool poolChildOfCenteralObject = false;

    /// <summary>
    /// attach the pool to the manager object?
    /// </summary>
    public bool poolChildOfManager = false;

    /// <summary>
    /// max audio sources to use, including the 2 used by music
    /// </summary>
    public int maxAudioSources = 32;

    /// <summary>
    /// fade time between music switching
    /// </summary>
    public float musicFadeTime = 1f;

    /// <summary>
    /// try to force the music to loop?
    /// </summary>
    public bool forceMusicLoop = true;


    /// <summary>
    /// main volume static access
    /// </summary>
    public static float volumeMain
    {
        get
        {
            if (instance) return instance.volMain;
            else Debug.LogError("try to get volumeMain when audio manager not initialised!");
            return 0f;
        }
        set
        {
            if (instance) instance.volMain = value;
            else Debug.LogError("try to set volumeMain when audio manager not initialised!");
        }
    }
    /// <summary>
    /// effects volume static access
    /// </summary>
    public static float volumeEffects
    {
        get
        {
            if (instance) return instance.volEffects;
            else Debug.LogError("try to get volumeEffects when audio manager not initialised!");
            return 0f;
        }
        set
        {
            if (instance) instance.volEffects = value;
            else Debug.LogError("try to set volumeEffects when audio manager not initialised!");
        }
    }
    /// <summary>
    /// voice volume static access
    /// </summary>
    public static float volumeVoice
    {
        get
        {
            if (instance) return instance.volVoice;
            else Debug.LogError("try to get volumeVoice when audio manager not initialised!");
            return 0f;
        }
        set
        {
            if (instance) instance.volVoice = value;
            else Debug.LogError("try to set volumeVoice when audio manager not initialised!");
        }
    }
    /// <summary>
    /// music volume static access
    /// </summary>
    public static float volumeMusic
    {
        get
        {
            if (instance) return instance.volMusic;
            else Debug.LogError("try to get volumeMusic when audio manager not initialised!");
            return 0f;
        }
        set
        {
            if (instance) instance.volMusic = value;
            else Debug.LogError("try to set volumeMusic when audio manager not initialised!");
        }
    }

    /// <summary>
    /// music fade time static access
    /// </summary>
    public static float fadeTime
    {
        get
        {
            if (instance) return instance.musicFadeTime;
            else Debug.LogError("try to get fadeTime when audio manager not initialised!");
            return 0f;
        }
        set
        {
            if (instance) instance.musicFadeTime = value;
            else Debug.LogError("try to set fadeTime when audio manager not initialised!");
        }
    }

    /// <summary>
    /// static access for forceMusicLoop
    /// </summary>
    public static bool musicLoopForced
    {
        get
        {
            if (instance) return instance.forceMusicLoop;
            else Debug.LogError("try to get musicLoopForced when audio manager not initialised!");
            return false;
        }
        set
        {
            if (instance) instance.forceMusicLoop = value;
            else Debug.LogError("try to set musicLoopForced when audio manager not initialised!");
        }
    }


    public enum PlaybackPriority
    { LOW, NORMAL, HIGH, SIZE }




    /// <summary>
    /// play an effects clip
    /// </summary>
    /// <param name="clip">the clip to play</param>
    /// <param name="trans">transform to play at, central if null</param>
    /// <param name="follow">should it follow the transform through playback?</param>
    /// <param name="volumeMod">volume modifier</param>
    /// <param name="priority">priority to play the clip</param>
    /// <returns>the active audiosource if sucessful, null otherwise</returns>
    public static AudioSource PlayEffect(AudioClip clip, Transform trans = null, bool follow = false,
        float volumeMod = 1f, PlaybackPriority priority = PlaybackPriority.NORMAL)
    {
        if (instance)
        {
            if (clip == null)
            {
                Debug.LogWarning("Try to play NULL effect! This is not allowed.");
                return null;
            }

            AuSoNode n = GetNode(priority);
            if (n == null) return null;

            activeEffectNodes.Add(n);
            activeNodes[(int)priority].Add(n);
            n.PlayClip(clip, trans, follow, volumeEffects * volumeMain * volumeMod);
            return n.source;
        }
        else
            Debug.LogError("Try to play effect when audiomanager uninitialised!");
        return null;
    }


    /// <summary>
    /// play a voice clip
    /// </summary>
    /// <param name="clip">the clip to play</param>
    /// <param name="trans">transform to play at, central if null</param>
    /// <param name="follow">should it follow the transform through playback?</param>
    /// <param name="volumeMod">volume modifier</param>
    /// <param name="priority">priority to play the clip</param>
    /// <returns>the active audiosource if sucessful, null otherwise</returns>
    public static AudioSource PlayVoice(AudioClip clip, Transform trans = null, bool follow = false,
        float volumeMod = 1f, PlaybackPriority priority = PlaybackPriority.NORMAL)
    {
        if (instance)
        {
            if (clip == null)
            {
                Debug.LogWarning("Try to play NULL voice! This is not allowed.");
                return null;
            }

            AuSoNode n = GetNode(priority);
            if (n == null) return null;

            activeVoiceNodes.Add(n);
            activeNodes[(int)priority].Add(n);
            n.PlayClip(clip, trans, follow, volumeVoice * volumeMain * volumeMod);
            return n.source;
        }
        else
            Debug.LogError("Try to play voice when audiomanager uninitialised!");
        return null;
    }


    /// <summary>
    /// sets the music to play and plays it
    /// </summary>
    /// <param name="clip"></param>
    public static void SetMusic(AudioClip clip, float startTime = 0f)
    {
        if (instance)
        {
            if (clip == null)
            {
                Debug.LogWarning("Try to switch to NULL music! This is not allowed.");
                return;
            }

            musicNodes.SwitchMusic(clip, centralObject ? centralObject.transform : audioSourcePool.transform, true, startTime);
        }
        else
            Debug.LogError("Try to set music when audiomanager uninitialised!");
    }

    /// <summary>
    /// plays the currently set music
    /// </summary>
    public static void PlayMusic()
    {
        if (instance)
        {
            musicNodes.PlayMusic();
        }
        else
            Debug.LogError("Try to play music when audiomanager uninitialised!");
    }

    /// <summary>
    /// pauses the current music
    /// </summary>
    public static void PauseMusic()
    {
        if (instance)
        {
            musicNodes.PauseMusic();
        }
        else
            Debug.LogError("Try to pause music when audiomanager uninitialised!");
    }

    /// <summary>
    /// stops the current music
    /// </summary>
    public static void StopMusic()
    {
        if (instance)
        {
            musicNodes.StopMusic();
        }
        else
            Debug.LogError("Try to stop music when audiomanager uninitialised!");
    }

    /// <summary>
    /// if the music is playing or not
    /// </summary>
    /// <returns>true if music playing, false otherwise</returns>
    public static bool IsMusicPlaying()
    {
        if (instance)
        {
            return musicNodes.isPlaying;
        }
        else
            Debug.LogError("Try to get IsMusicPlaying when audiomanager uninitialised!");
        return false;
    }

    /// <summary>
    /// fades the music to a target volume multiplier
    /// </summary>
    /// <param name="targVolMult">volume multiplier to fade to</param>
    /// <param name="time">time to take to fade, uses normal fade if -1f</param>
    public static void FadeMusic(float targVolMult = 1f, float time = -1f)
    {
        if (instance)
        {
            targVolMult = Mathf.Min(1f,Mathf.Max(0f,targVolMult));
            musicNodes.Fade(targVolMult, time);
        }
        else
            Debug.LogError("Try to call FadeMusic when audiomanager uninitialised!");
    }


    /// <summary>
    /// assistant wrapper to pause an active clip
    /// </summary>
    /// <param name="src"></param>
    public static void PauseClip(AudioSource src)
    {
        src.Pause();
    }

    /// <summary>
    /// assistant wrapper to stop an active clip,
    /// the source is then non-active next fixed update and used for a later clip
    /// </summary>
    /// <param name="src"></param>
    public static void StopClip(AudioSource src)
    {
        src.Stop();
    }



    /// <summary>
    /// play all paused effects
    /// </summary>
    public static void PlayAllEffects()
    {
        if (instance)
        {
            foreach (AuSoNode n in activeEffectNodes)
                if (n.source.isPlaying) n.source.Play();
        }
        else
            Debug.LogError("Try to play all effects when audiomanager uninitialised!");
    }

    /// <summary>
    /// pauses all effects playing
    /// </summary>
    public static void PauseAllEffects()
    {
        if (instance)
        {
            foreach (AuSoNode n in activeEffectNodes)
                if (n.source.isPlaying) n.source.Pause();
        }
        else
            Debug.LogError("Try to pause all effects when audiomanager uninitialised!");
    }

    /// <summary>
    /// stops all effects, will be cleared out next fixedupdate
    /// </summary>
    public static void StopAllEffects()
    {
        if (instance)
        {
            foreach (AuSoNode n in activeEffectNodes)
                if (n.source.isPlaying) n.source.Stop();
        }
        else
            Debug.LogError("Try to stop all effects when audiomanager uninitialised!");
    }

    /// <summary>
    /// plays all paused voices
    /// </summary>
    public static void PlayAllVoice()
    {
        if (instance)
        {
            foreach (AuSoNode n in activeVoiceNodes)
                if (n.source.isPlaying) n.source.Play();
        }
        else
            Debug.LogError("Try to play all voice when audiomanager uninitialised!");
    }

    /// <summary>
    /// pauses all voices playing
    /// </summary>
    public static void PauseAllVoice()
    {
        if (instance)
        {
            foreach (AuSoNode n in activeVoiceNodes)
                if (n.source.isPlaying) n.source.Pause();
        }
        else
            Debug.LogError("Try to pause all voice when audiomanager uninitialised!");
    }

    /// <summary>
    /// stops all voices, will be cleared out next fixedupdate
    /// </summary>
    public static void StopAllVoice()
    {
        if (instance)
        {
            foreach (AuSoNode n in activeVoiceNodes)
                if (n.source.isPlaying) n.source.Stop();
        }
        else
            Debug.LogError("Try to stop all voice when audiomanager uninitialised!");
    }


    /// <summary>
    /// returns total number of free channels, including if music is not playing
    /// </summary>
    /// <returns></returns>
    public static int CurrentFreeSources()
    {
        if (instance)
        {
            return unusedNodes.size + (musicNodes.isPlaying ? 0 : 2);
        }
        else
            Debug.LogError("Try get current free sources when audiomanager uninitialised!");
        return 0;
    }

    /// <summary>
    /// returns ratio of free sources, including music
    /// </summary>
    /// <returns>0f if all in use, 1f if all free</returns>
    public static float CurrentFreeRatio()
    {
        if (instance)
        {
            return (float)(unusedNodes.size + (musicNodes.isPlaying ? 0 : 2)) / (float)sourceCount;
        }
        else
            Debug.LogError("Try get current free ratio when audiomanager uninitialised!");
        return 0;
    }


    /// <summary>
    /// access for direct music source manipulation
    /// </summary>
    public static AudioSource activeMusicSource
    {
        get { return musicNodes.activeSource; }
        private set { musicNodes.activeSource = value; }
    }

    /// <summary>
    /// set the pitch for everything active, good with time control
    /// </summary>
    public static void SetPitchForAll(float value)
    {
        if (instance)
        {
            foreach (AuSoNode n in activeEffectNodes)
                n.source.pitch = value;

            foreach (AuSoNode n in activeVoiceNodes)
                n.source.pitch = value;

            activeMusicSource.pitch = value;
        }
        else
            Debug.LogError("Try set pitch for all when audiomanager uninitialised!");
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    // Internal code, doesnt concern most users

    static GameObject centralObject { get { return instance.centralAudioObject; } }

    static GameObject audioSourcePool;


    class AuSoNode
    {
        public AudioSource source;

        public AuSoNode(Transform parent)
        {
            GameObject obj = new GameObject("AudioSource");
            source = obj.AddComponent<AudioSource>();
            source.transform.parent = parent;
        }

        public void PlayClip(AudioClip clip, Transform trans, bool parent, float volume)
        {
            if (source.isPlaying)
                source.Stop();

            source.clip = clip;
            source.volume = volume;

            if (trans)
            {
                if (parent)
                {
                    source.transform.position = Vector3.zero;
                    source.transform.rotation = Quaternion.identity;
                    source.transform.parent = trans;
                }
                else
                {
                    source.transform.position = trans.position;
                    source.transform.rotation = trans.rotation;
                    source.transform.parent = audioSourcePool.transform;
                }
            }
            else
            {
                source.transform.position = Vector3.zero;
                source.transform.rotation = Quaternion.identity;
                source.transform.parent = centralObject.transform;
            }

            source.Play();
        }
    }

    class MusMan
    {
        AuSoNode activeNode;
        AuSoNode otherNode;

        public AudioSource activeSource
        {
            get { return activeNode.source; }
            set { activeNode.source = value; }
        }

        float currFade;
        bool musicPaused;
        float currVolumeMulti;
        float targVolumeMulti;
        float fadeTimeOverride;

        public MusMan(BetterList<AuSoNode> nodes)
        {
            activeNode = nodes.Pop();
            otherNode = nodes.Pop();

            currFade = 0f;
            musicPaused = false;
            currVolumeMulti = 1f;
            targVolumeMulti = 1f;
            fadeTimeOverride = -1f;
        }

        public void UpdVol()
        {
            activeNode.source.volume = volumeMusic * volumeMain * currVolumeMulti;
        }

        public void Update(float delta)
        {
            if (currVolumeMulti != targVolumeMulti)
            {
                float dir = currVolumeMulti > targVolumeMulti ? -1f : 1f;
                dir *= fadeTimeOverride == -1f ? fadeTime : fadeTimeOverride;

                currVolumeMulti += dir * delta;

                if (currVolumeMulti * dir >= targVolumeMulti * dir)
                    currVolumeMulti = targVolumeMulti;

                UpdVol();
            }

            if (currFade > 0f)
            {
                currFade -= delta;

                float v = volumeMusic * volumeMain * currVolumeMulti;

                activeNode.source.volume = Mathf.Lerp(v, 0f, currFade / fadeTime);
                otherNode.source.volume = Mathf.Lerp(0f, v, currFade / fadeTime);

                if (currFade <= 0f)
                {
                    activeNode.source.volume = v;
                    otherNode.source.volume = 0f;
                    otherNode.source.Stop();
                }
            }

            if (musicLoopForced)
                if (activeNode.source.clip)
                    if (!activeNode.source.isPlaying && !musicPaused)
                        activeNode.source.Play();
        }

        public void SwitchMusic(AudioClip clip, Transform trans, bool parent, float startTime)
        {
            AuSoNode a = activeNode;
            activeNode = otherNode;
            otherNode = a;

            activeNode.PlayClip(clip, trans, parent, 0f);

            if (startTime < 0f)
                Debug.LogWarning("Music starting time less than zero, behavior for this is undetermined.");

            if (startTime > 0f && clip.length > 0f)
            {
                int mult = (int)(startTime / clip.length);
                activeNode.source.time = startTime - ((float)mult * clip.length);
            }

            currFade = fadeTime;
        }

        public void PlayMusic()
        {
            activeNode.source.Play();
            musicPaused = false;
        }

        public void PauseMusic()
        {
            activeNode.source.Pause();
            musicPaused = true;
        }

        public void StopMusic()
        {
            activeNode.source.Stop();
        }

        public bool isPlaying
        { get { return activeNode.source ? activeNode.source.isPlaying : false; } }

        public void Fade(float targVolMult = 1f, float time = -1f)
        {
            targVolumeMulti = targVolMult;
            fadeTimeOverride = time;
        }
    }


    static int sourceCount;
    static BetterList<AuSoNode> unusedNodes;
    static BetterList<AuSoNode>[] activeNodes;

    static BetterList<AuSoNode> activeEffectNodes;
    static BetterList<AuSoNode> activeVoiceNodes;
    static MusMan musicNodes;

#if !UNITY_4_5
    static float lastTime;
#endif
    void Start()
    {

#if !UNITY_4_5
        lastTime = Time.timeSinceLevelLoad;
#endif

        /// instance
        if (instance)
        {
            Debug.LogError("Cannot have more than one AudioManager in a scene!");
            return;
        }

        instance = this;

        /// object setup
        audioSourcePool = new GameObject(sourcePoolName);

        if (poolChildOfCenteralObject && poolChildOfManager)
            Debug.LogWarning("pool cant be child of both central and manager objects, the later will take priority");

        if (poolChildOfCenteralObject)
        {
            if (centralObject)
                audioSourcePool.transform.parent = centralObject.transform.parent;
            else
                Debug.LogError("poolChildOfCenteralObject true but central object is null!");
        }

        if (poolChildOfManager)
            audioSourcePool.transform.parent = this.transform;

        if (maxAudioSources < 0)
        {
            Debug.LogError("max audio sources cannot be less than 0!");
            sourceCount = 32;
        }
        else
            sourceCount = maxAudioSources;

        /// lists and nodes setup
        unusedNodes = new BetterList<AuSoNode>();
        activeNodes = new BetterList<AuSoNode>[(int)PlaybackPriority.SIZE];
        for (int i = 0; i < (int)PlaybackPriority.SIZE; ++i)
            activeNodes[i] = new BetterList<AuSoNode>();

        for (int i = 0; i < sourceCount; ++i)
            unusedNodes.Add(new AuSoNode(audioSourcePool.transform));

        musicNodes = new MusMan(unusedNodes);

        activeEffectNodes = new BetterList<AuSoNode>();
        activeVoiceNodes = new BetterList<AuSoNode>();
    }

    float prvMain;
    float prvEffe;
    float prvMusi;
    float prvVoic;
    void Update()
    {
        // update volumes if needed
        if (volumeMain != prvMain)
        {
            foreach (AuSoNode n in activeEffectNodes)
                n.source.volume = volEffects * volMain;

            foreach (AuSoNode n in activeVoiceNodes)
                n.source.volume = volVoice * volMain;

            musicNodes.UpdVol();

            prvMain = volumeMain;
            prvEffe = volumeEffects;
            prvMusi = volumeMusic;
            prvVoic = volumeVoice;
        }
        else
        {
            if (volEffects != prvEffe)
            {
                foreach (AuSoNode n in activeEffectNodes)
                    n.source.volume = volEffects * volMain;
                prvEffe = volumeEffects;
            }

            if (volumeVoice != prvVoic)
            {
                foreach (AuSoNode n in activeVoiceNodes)
                    n.source.volume = volVoice * volMain;
                prvVoic = volumeVoice;
            }

            if (volMusic != prvMusi)
            {
                musicNodes.UpdVol();
                prvMusi = volMusic;
            }
        }

        // clear any finished active nodes
        for (int i = 0; i < (int)PlaybackPriority.SIZE; ++i)
        {
            for (int i2 = 0; i2 < activeNodes[i].size; )
            {
                if (activeNodes[i][i2].source.isPlaying)
                    ++i;
                else
                {
                    unusedNodes.Add(activeNodes[i][i2]);
                    activeNodes[i].RemoveAt(i2);
                }
            }
        }

#if UNITY_4_5
        musicNodes.Update(Time.unscaledDeltaTime);
#else
        musicNodes.Update(Time.timeSinceLevelLoad - lastTime);
        lastTime = Time.timeSinceLevelLoad;
#endif

    }


    static AuSoNode GetNode(PlaybackPriority priority)
    {
        AuSoNode n = null;
        if (unusedNodes.size > 0)
            n = unusedNodes.Pop();
        else
        {
            for (int i = 0; i <= (int)priority; ++i)
            {
                if (activeNodes[i].size > 0)
                {
                    n = activeNodes[i][0];
                    activeNodes[i].RemoveAt(0);
                    break;
                }
            }
        }
        return n;
    }


    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    // legacy compatibility

    [Obsolete("use the new system")]
    public static void PlayEffectAt(AudioClip clip, Transform trans)
    {
        Debug.Log("play " + clip.name);
        PlayEffect(clip, trans);
    }

}


///////////////////////////////////////////////////////////////////////////////////////////////////////
// playmaker

#if PLAYMAKER_COMPATIBILITY

namespace HutongGames.PlayMaker.Actions
{
// volumes
    [ActionCategory(ActionCategory.Audio)]
    [Tooltip("Get the audio volumes")]
    public class GetVolumes : FsmStateAction
    {
        [UIHint(UIHint.Variable)]
        public FsmFloat main;

        [UIHint(UIHint.Variable)]
        public FsmFloat music;

        [UIHint(UIHint.Variable)]
        public FsmFloat voice;

        [UIHint(UIHint.Variable)]
        public FsmFloat effects;

        public override void OnEnter()
        {
            if (!main.IsNone) main.Value = AudioManager.volumeMain;
            if (!music.IsNone) music.Value = AudioManager.volumeMusic;
            if (!voice.IsNone) voice.Value = AudioManager.volumeVoice;
            if (!effects.IsNone) effects.Value = AudioManager.volumeEffects;
        }
    }

    [ActionCategory(ActionCategory.Audio)]
    [Tooltip("Set the audio volumes")]
    public class SetVolumes : FsmStateAction
    {
        [UIHint(UIHint.Variable)]
        public FsmFloat main;

        [UIHint(UIHint.Variable)]
        public FsmFloat music;

        [UIHint(UIHint.Variable)]
        public FsmFloat voice;

        [UIHint(UIHint.Variable)]
        public FsmFloat effects;

        public override void OnEnter()
        {
            if (!main.IsNone) AudioManager.volumeMain = main.Value;
            if (!music.IsNone) AudioManager.volumeMusic = music.Value;
            if (!voice.IsNone) AudioManager.volumeVoice = voice.Value;
            if (!effects.IsNone) AudioManager.volumeEffects = effects.Value;
        }
    }



// music
    [ActionCategory(ActionCategory.Audio)]
    [Tooltip("Set Music")]
    public class SetMusic : FsmStateAction
    {
        [Tooltip("AudioClip to play")]
        public AudioClip clip;

        [UIHint(UIHint.Variable)]
        [Tooltip("starting time")]
        public FsmFloat startTime;

        [Tooltip("should trigger play?")]
        public bool play;

        public override void OnEnter()
        {
            AudioManager.SetMusic(clip, startTime.IsNone ? 0f : startTime.Value);
            if (play) AudioManager.PlayMusic();
            Finish();
        }
    }

    [ActionCategory(ActionCategory.Audio)]
    [Tooltip("Play the set Music")]
    public class PlayMusic : FsmStateAction
    {
        public override void OnEnter()
        {
            AudioManager.PlayMusic();
            Finish();
        }
    }

    [ActionCategory(ActionCategory.Audio)]
    [Tooltip("Pause the set Music")]
    public class PauseMusic : FsmStateAction
    {
        public override void OnEnter()
        {
            AudioManager.PauseMusic();
            Finish();
        }
    }

    [ActionCategory(ActionCategory.Audio)]
    [Tooltip("Stop the set Music")]
    public class StopMusic : FsmStateAction
    {
        public override void OnEnter()
        {
            AudioManager.StopMusic();
            Finish();
        }
    }

    [ActionCategory(ActionCategory.Audio)]
    [Tooltip("Get IsMusicPlaying")]
    public class GetIsMusicPlaying : FsmStateAction
    {
        [UIHint(UIHint.Variable)]
        public FsmBool isPlaying;

        public override void Reset()
        {
            isPlaying = false;
        }

        public override void OnEnter()
        {
            if (!isPlaying.IsNone) isPlaying.Value = AudioManager.IsMusicPlaying();
        }
    }



// effects
    [ActionCategory(ActionCategory.Audio)]
    [Tooltip("Play Effect")]
    public class PlayEffect : FsmStateAction
    {
        [Tooltip("AudioClip to play")]
        public AudioClip clip;

        [Tooltip("Transform to play at, will be center object if null")]
        public Transform transform = null;

        [Tooltip("should it follow the transform?")]
        public bool follow = false;

        [Tooltip("volume modification")]
        public float volumeMod = 1f;

        [Tooltip("playback priority")]
        AudioManager.PlaybackPriority priority = AudioManager.PlaybackPriority.NORMAL;

        public override void OnEnter()
        {
            AudioManager.PlayEffect(clip, transform, follow, volumeMod, priority);
            Finish();
        }
    }

    [ActionCategory(ActionCategory.Audio)]
    [Tooltip("Stops all playing Effects")]
    public class StopAllEffects : FsmStateAction
    {
        public override void OnEnter()
        {
            AudioManager.StopAllEffects();
            Finish();
        }
    }



// voice
    [ActionCategory(ActionCategory.Audio)]
    [Tooltip("Play Voice")]
    public class PlayVoice : FsmStateAction
    {
        [Tooltip("AudioClip to play")]
        public AudioClip clip;

        [Tooltip("Transform to play at, will be center object if null")]
        public Transform transform = null;

        [Tooltip("should it follow the transform?")]
        public bool follow = false;

        [Tooltip("volume modification")]
        public float volumeMod = 1f;

        [Tooltip("playback priority")]
        AudioManager.PlaybackPriority priority = AudioManager.PlaybackPriority.NORMAL;

        public override void OnEnter()
        {
            AudioManager.PlayVoice(clip, transform, follow, volumeMod, priority);
            Finish();
        }
    }

    [ActionCategory(ActionCategory.Audio)]
    [Tooltip("Stops all playing Voice")]
    public class StopAllVoice : FsmStateAction
    {
        public override void OnEnter()
        {
            AudioManager.StopAllVoice();
            Finish();
        }
    }
}

#endif
