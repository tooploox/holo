# Holo Model Series

## Project location

Unity project is located in unity/Holo directory
To see initial model please load **AnimatedModels** scene located in *Assets/Scenes*

## Installation

### Prerequisites 
  * Setup Hololens with Windows Device Portal enabled: https://docs.microsoft.com/en-us/windows/mixed-reality/using-the-windows-device-portal
  * Install Holo Model Series application on Hololens (use relasese versions: https://github.com/MicroscopeIT/holo/releases) 
    Application instalation: https://docs.microsoft.com/en-us/hololens/hololens-install-apps#use-the-windows-device-portal-to-install-apps-on-hololens

  **IMPORTANT**
      If you got *.NET CoreRuntime* invalid or missing version error please install application with additional dependencies. To do this select option: *Allow me to select framework packages*, and in next step select the *Microsoft.NET.CoreRuntime.1.1.appx* file from release archive (Dependencies -> x86).

  * Convert VTK model series into Asset Bundles
  * Upload AssetBundles with models into Hololens headset
  * Run application
    
#### Converting VTK model series into Asset Bundles

Open Unity project from unity/Holo.

1. Run script: **Holo -> Convert model to a AssetBundle's GameObject**
2. Choose the directory with model time series in VTK files
3. Your resulting AssetBundle is located in _**<repository location>/unity/Holo/Assets/StreamingAssets/<name of converted directory>\_bundle(.manifest)**_

#### Uploading AssetBundles to Hololens headset

1. Go to Windows Device Portal of Hololens headset
2. Go to _**System -> File Explorer**_
3. Navigate to: _**User Folders \ LocalAppData \ HoloModelSeries\_1.0.0.0_x86\_\_<hash/crc> \ LocalState**_
4. Upload Asset Bundle files (**\_bundle** along with **\_bundle.manifet**) in to **LocalState** directory one by one with the *Upload* button (first you need to select file from hard drive, you can drag and drop it as well)

## Usage

Currently HoloModelSeries application loads only AssetBundles uploaded to Hololens headset. Models must be uploaded before application is run (rescanning of the collection's models is not yet implemented).

**IMPORTANT**: If you upload new models HoloModelSeries must be closed. For the moment you need to run other application for the system to unload completly HoloModelSeries application.
