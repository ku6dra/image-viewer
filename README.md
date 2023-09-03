# ImageViewer
A Synced Image Loading System for VRCSDK3 Worlds.

## Features
- Local Image Availability Check Before Sharing URL with Other Clients
  - Prevents triggering the 5-second loading interval for other clients in case of loading failure
- Support for Transparent Images
- Pixel Art Rendering Mode Toggle (PixelMode)
- Copy URL of Displayed Image


## Requirements
### VRChat Client Version
- 2023.1.2p4 Build 1290 (2023-03-22)
- 2023.3.2 Build 1340 (2023-08-22)

### VPM
- VRChat SDK - Base v3.1.11, v3.2.3 (or later)
- VRChat SDK - Worlds v3.1.11, v3.2.3 (or later)
- UdonSharp v1.1.7, v1.1.9 (or later)

Compatibility with future updates is not guaranteed.

## License
Licensed under the [MIT License](LICENSE).
