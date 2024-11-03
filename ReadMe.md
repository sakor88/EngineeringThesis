The project aimed to create an immersive VR application for visualizing DICOM medical imaging using Unity and the Oculus Quest 2. By leveraging volume rendering and VR-specific assets from the VRTKv4 framework, the application effectively renders 3D models of patients from DICOM data. Key functionalities include object interaction, manipulation (rotation and scaling), teleportation, and dataset adjustments, such as slicing and dose visualization.



**Volume Rendering and Visualization Techniques** 

1. **Raymarching:** Allows projection of 3D data by sampling density along a ray, supporting modes like Maximum Intensity Projection, Direct Volume Rendering, and Isosurface Rendering. 
1. **UnityVolumeRendering** [https://github.com/mlavik1/UnityVolumeRendering:](https://github.com/mlavik1/UnityVolumeRendering): This project, modified in the application, provides essential rendering utilities and supports importing DICOM datasets, setting transfer functions, and visualizing cross-sections. 
3. **VR Interaction Setup with VRTK** 
- **Controllers and Interactions:** Utilizing VRTK components for grabbing, snapping, and spatial button interaction enables the user to manipulate the dataset as physical objects. 



- **Teleportation and Object Pointers:** The project uses curved and straight pointers for movement and interaction, respectively, enhancing the user experience within the virtual room. 



- **Dose Visualization:** An experimental feature allows dose data visualization with color - coded intensity (WIP). 



- **Further Developments:** Planned enhancements include verifying dose and patient data alignment and extending dose visualization onto CT models.

**Conclusion** 

The application demonstrates a successful integration of DICOM data visualization in VR, offering an interactive tool for exploring medical imaging data. The project showcases the potential of combining Unity, VRTK, and Oculus VR for developing functional and extensible medical imaging applications in a VR environment.
