using System.Collections.Generic;
using Howl.Vendors.FMOD.Studio;
using DotNetSystem = System;

namespace Howl.Vendors.FMOD;

public class FmodAudioManager{

    /// <summary>
    /// Gets the FMOD Studio System created by this AudioManager.
    /// </summary>
    public Studio.System StudioSystem {get; private set;}
    
    /// <summary>
    /// Gets the FMOD Core System created by this AudioManager.
    /// </summary>
    public System CoreSystem {get; private set;}
        
    private Dictionary<string, Bank> loadedBanks;

    /// <summary>
    /// Creates a new AudioManager instance.
    /// </summary>
    /// <param name="fileDirectory">The file directory path, relative to the executable, where all audio files are located.</param>
    public FmodAudioManager(GUID masterBusGuid){
        
        loadedBanks = new();

        // load Masters.

        CreateFmodSystems();

        // Use Stereo Audio.
        
        HandleResult(CoreSystem.setSoftwareFormat(
            48000,
            SPEAKERMODE.STEREO,
            0 // <-- should never change.
        ));

        // initialise with all defined settings as it cannot be changed afterwards.

        HandleResult(StudioSystem.initialize(
            128, // how many simultaneous audio channels (voices) FMOD can mix and play at the same time.
            FMOD.Studio.INITFLAGS.NORMAL,
            FMOD.INITFLAGS.NORMAL,
            0
        ));

        // load master banks.
        LoadBank("Master");
        LoadBank("Master.strings");
        SetBusVolume(masterBusGuid, 1.0f);
    }



    /// <summary>
    /// Prints the result of an FMOD function call, if it is not RESULT.OK.
    /// </summary>
    /// <param name="result"></param>
    private void HandleResult(RESULT result){
        if(result != RESULT.OK){
            #if DEBUG
                throw new DotNetSystem.Exception($"{result}");
            #else
                // (todo)
                // add a logger here.
            #endif
            
        }
    }

    /// <summary>
    /// Creates instances for FMOD Studio and Core System.
    /// </summary>
    private void CreateFmodSystems(){
        HandleResult(FMOD.Studio.System.create(
            out FMOD.Studio.System system
        ));

        HandleResult(system.getCoreSystem(
            out FMOD.System coreSystem)
        );

        StudioSystem = system;
        CoreSystem = coreSystem; 

    }

    /// <summary>
    /// Disposes of the FMOD Studio and Core System.
    /// </summary>
    public void Dispose(){
        DotNetSystem.Diagnostics.Debug.WriteLine($"[AudioManager] Dispose");
        HandleResult(CoreSystem.release());
        HandleResult(StudioSystem.release());
    }

    /// <summary>
    /// Loads a bank instance into the FMOD Studio System to play sounds from.
    /// </summary>
    /// <param name="bankName">The name of a bank to load, without the ".bank" extension.</param>
    public void LoadBank(string bankName){
        // Load the bank from FMOD studio.
        
        HandleResult(StudioSystem.loadBankFile(
            GetBankPath(bankName),
            LOAD_BANK_FLAGS.NORMAL,
            out Bank bank)
        );

        loadedBanks.Add(bankName, bank);        
    }

    /// <summary>
    /// Unloads a bank instance from the FMOD Studio System.
    /// </summary>
    /// <param name="bankName">The name of a bank to load, without the ".bank" extension.</param>
    public void UnloadBank(string bankName){      
        
        HandleResult(loadedBanks[bankName].unload());
        
        // Remove the unloaded bank.

        loadedBanks.Remove(bankName);
    }


    /// <summary>
    /// Sets the volume of a bus, via a bus handle that has been loaded.
    /// </summary>
    /// <param name="guid">The guid of the bus.</param>
    /// <param name="volume">The specified volume to change to.</param>
    public void SetBusVolume(GUID guid, float volume){
        HandleResult(StudioSystem.getBusByID(guid, out Bus bus));
        HandleResult(bus.setVolume(volume));
        HandleResult(StudioSystem.update());
    }

    /// <summary>
    /// Gets the volume of a bus, via a bus handle that has been loaded.
    /// </summary>
    /// <param name="busHandleName">The name of the loaded bus handle to use when accessing a bus in the FMOD Studio System.</param>
    /// <returns></returns>
    public float GetBusVolume(GUID guid){
        HandleResult(StudioSystem.getBusByID(guid, out Bus bus));
        HandleResult(bus.getVolume(out float volume));
        return volume;
    }

    /// <summary>
    /// Plays a one-shot instance of a sound.
    /// </summary>
    /// <param name="guid">The GUID of the event to play.</param>
    public void PlayOneShot(GUID guid){

        // Get the loaded event description.

        HandleResult(StudioSystem.getEventByID(guid, out EventDescription desc));

        // Create and play an instance of the description.

        HandleResult(desc.createInstance(out EventInstance inst));
        HandleResult(inst.start());
        
        // Immediately release it, so when the sound has finished, FMOD Studio can garbage collect it.
        HandleResult(inst.release());

        // update the audio system to play the sound.
        HandleResult(StudioSystem.update());
    }

    /// <summary>
    /// Gets the full relative file directory path to a bank within this project.
    /// </summary>
    /// <param name="bankName">The specified name of the bank to get the path to.</param>
    /// <returns></returns>
    private string GetBankPath(string bankName){
        // NOTE: this will need to change when building to other platforms like consoles.
        // string folderPath = HowlApp.Content.RootDirectory+@"\bin\DesktopGL\Audio\";
        string filePath = DotNetSystem.IO.Path.Combine(AssetManagement.AssetManager.AudioFolder, bankName+".bank");
        return filePath;
    }
}