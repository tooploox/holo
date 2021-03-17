# Essential Vision

Application to visualize animated 3D medical data using Hololens, with shared experience. See the [official webpage of Essential Vision](https://www.essential-vision.org/) for more detailed list of features and demo videos.

## Installation

### Prerequisites
  * Setup Hololens with Windows Device Portal enabled: https://docs.microsoft.com/en-us/windows/iot-core/manage-your-device/deviceportal
  * Install _Essential Vision_ application on Hololens (use relasese versions: https://github.com/MicroscopeIT/holo/releases)
    Application instalation: https://docs.microsoft.com/pl-pl/hololens/holographic-custom-apps

  **IMPORTANT**
      If you got *.NET CoreRuntime* invalid or missing version error please install application with additional dependencies. To do this select option: *Allow me to select framework packages*, and in next step select the *Microsoft.NET.CoreRuntime.1.1.appx* file from release archive (Dependencies -> x86).

  * Convert your input models into Asset Bundles.

		The whole pipeline for converting your models into uploadable Asset Bundles can be found [here](https://github.com/MicroscopeIT/holo-preprocess/)

  * Upload AssetBundles with models into Hololens headset
  * Run application

#### Uploading AssetBundles to Hololens headset

1. Go to Windows Device Portal of Hololens headset
2. Go to _**System -> File Explorer**_
3. Navigate to: _**User Folders \ LocalAppData \ EssentialVision\_1.0.\<version>.0\_\<architecture>\_\_<hash/crc> \ LocalState**_ (where `<architecture>` is `x86` for Hololens 1, `arm64` for Hololens 2)
4. Upload Asset Bundle files (**\_bundle** along with **\_bundle.manifet**) in to **LocalState** directory one by one with the *Upload* button (first you need to select file from hard drive, you can drag and drop it as well)

## Usage

After starting the application, choose the "Shared Experience" mode in the UI:

- Click the "Start Session" button to be the server (like a teacher in the classroom) that dictates the view of other participants.

- Or choose a session name (corresponding to your Hololens name) and then click "Join Session" to be the student (observing  what teacher sets).

- Or choose "Offline Mode" to test the application on your own, without being any server or client. The effect in principle looks similar to clicking "Start Session", but the application doesn't listen on any port, doesn't synchronize anything etc.

Teacher can then choose a model, choose layers to display inside, transform it and so on.

Note that currently the application loads only AssetBundles uploaded to Hololens headset. Models must be uploaded before application is run (rescanning of the collection's models is not yet implemented).

**IMPORTANT**: If you upload new models, the application must be closed. For the moment you need to run other application for the system to unload completly _Essential Vision_ application.

## Developing in Unity

Unity project is located in `unity/Holo` directory.

To see initial model please load **AnimatedModels** scene located in *Assets/Scenes*.
