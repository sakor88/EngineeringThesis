using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tilia.Interactions.Interactables.Interactables;
using UnityEngine;

namespace UnityVolumeRendering
{
    /// <summary>
    /// This is a basic runtime GUI, that can be used during play mode.
    /// You can import datasets, and edit them.
    /// Add this component to an empty GameObject in your scene (it's already in the test scene) and click play to see the GUI.
    /// </summary>
    public class RuntimeGUI : MonoBehaviour
    {
        private GameObject interactablePrefab;
        [SerializeField]
        private const string assetName = "Interactions.Interactable";
        [SerializeField]
        private const string assetSuffix = ".prefab";
        private List<string> CTFilePaths;

        public void OnOpenDICOMDatasetResultVR(RuntimeFileBrowser.DialogResult result)
        {
            if (!result.cancelled)
            {
                DespawnAllDatasetsVR();

                bool recursive = true;

                IEnumerable<string> fileCandidates = Directory.EnumerateFiles(result.path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase));

                // Get all files in DICOM directory
                CTFilePaths = Directory.GetFiles(result.path).ToList();
                // Create importer
                IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.DICOM);
                // Load list of DICOM series (normally just one series)
                IEnumerable<IImageSequenceSeries> seriesList = importer.LoadSeries(CTFilePaths);
                // There will usually just be one series
                foreach (IImageSequenceSeries series in seriesList)
                {
                    // Import single DICOm series
                    VolumeDataset dataset = importer.ImportSeries(series);
                    VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                    obj.transform.position = new Vector3(-0.1f, 2.7f, 0.0f);
                    var child = obj.gameObject.transform.GetChild(0).gameObject;


                    GameObject helper = new GameObject("DicomCTVolumeRenderedObject");
                    obj.transform.SetParent(helper.transform);
                    child.transform.SetParent(helper.transform);

                    child.AddComponent<BoxCollider>();
                    prepareInteractablePrefab();
                    ConvertSelectedGameObject(child.gameObject);

                    obj.CreateSlicingPlane();
                    SlicingPlane plane = FindObjectsOfType<SlicingPlane>()[0];
                    GameObject planeObj = plane.gameObject;
                    obj.gameObject.transform.position = new Vector3(-2.25f, 2.5f, -2f);
                    obj.gameObject.transform.localEulerAngles = new Vector3(90f, 0f, 90f);
                    plane.gameObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

                }

            }

        }

        public void OnOpenDoseResultVR(RuntimeFileBrowser.DialogResult result)
        {

            if (!result.cancelled)
            {

                bool recursive = true;
                IEnumerable<string> fileCandidates = Directory.EnumerateFiles(result.path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(p => p.EndsWith("suma.dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith("suma.dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith("suma.dicm", StringComparison.InvariantCultureIgnoreCase));

                string doseFilePath = fileCandidates.First();

                // Create importer
                DoseImporter importer = new DoseImporter();
                // Load list of DICOM series (normally just one series)
                IEnumerable<IImageSequenceSeries> seriesList = importer.LoadSeries(CTFilePaths);
                // There will usually just be one series
                foreach (IImageSequenceSeries series in seriesList)
                {
                    // Import single DICOM series along with DICOM RT Dose
                    VolumeDataset dataset = importer.ImportSeries(series, doseFilePath);

                    DoseVolumeRenderedObject obj = VolumeObjectFactory.CreateDoseObject(dataset);
                    obj.transform.position = new Vector3(2.245f, 2.63f, -1.95f);
                    obj.transform.Rotate(0, 0, -90);

                }

            }


            //if (!result.cancelled)
            //{
            //    bool recursive = true;
            //    //DespawnAllDatasetsVR();

            //    IEnumerable<string> fileCandidates = Directory.EnumerateFiles(result.path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            //        .Where(p => p.EndsWith("suma.dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith("suma.dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith("suma.dicm", StringComparison.InvariantCultureIgnoreCase));

            //    string doseFilePath = fileCandidates.First();

            //    // Create importer
            //    DoseImporter importer = new DoseImporter();

            //    // Import single DICOm series
            //    VolumeDataset dataset = importer.ReadDoseFile(doseFilePath);
            //    DoseVolumeRenderedObject obj = VolumeObjectFactory.CreateDoseObject(dataset);
            //    obj.transform.position = new Vector3(2.245f, 2.63f, -1.95f);
            //    obj.transform.Rotate(0, 0, -90);


            //}

        }

        public void prepareInteractablePrefab()
        {
            interactablePrefab = (GameObject)Resources.Load("Interactions.Interactable");
        }

        public void CreateInteractable(GameObject objectToWrap)
        {
            int siblingIndex = objectToWrap.transform.GetSiblingIndex();
            GameObject newInteractable = Instantiate(interactablePrefab);   
            newInteractable.name += "_" + objectToWrap.name;
            InteractableFacade facade = newInteractable.GetComponent<InteractableFacade>();

            newInteractable.transform.SetParent(objectToWrap.transform.parent);
            newInteractable.transform.localPosition = objectToWrap.transform.localPosition;
            newInteractable.transform.localRotation = objectToWrap.transform.localRotation;
            newInteractable.transform.localScale = objectToWrap.transform.localScale;

            foreach (MeshRenderer defaultMeshes in facade.Configuration.MeshContainer.GetComponentsInChildren<MeshRenderer>())
            {
                defaultMeshes.gameObject.SetActive(false);
            }

            objectToWrap.transform.SetParent(facade.Configuration.MeshContainer.transform);
            objectToWrap.transform.localPosition = Vector3.zero;
            objectToWrap.transform.localRotation = Quaternion.identity;
            objectToWrap.transform.localScale = Vector3.one;

            newInteractable.transform.SetSiblingIndex(siblingIndex);

        }

        public void ConvertSelectedGameObject(GameObject objectToConvert)
        {
            InteractableFacade interactable = objectToConvert.GetComponentInParent<InteractableFacade>();
            if (interactable == null)
            {
                CreateInteractable(objectToConvert);
            }
        }


    

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            
            // Show dataset import buttons
            /*if(GUILayout.Button("Import RAW dataset"))
            {
                RuntimeFileBrowser.ShowOpenFileDialog(OnOpenRAWDatasetResult, "DataFiles");
            }*/

            /*if(GUILayout.Button("Import PARCHG dataset"))
            {
                RuntimeFileBrowser.ShowOpenFileDialog(OnOpenPARDatasetResult, "DataFiles");
            }*/

            if (GUILayout.Button("Import DICOM dataset"))
            {
                RuntimeFileBrowser.ShowOpenDirectoryDialog(OnOpenDICOMDatasetResultVR);
            }


            if (GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Import Dose file"))
            {
                RuntimeFileBrowser.ShowOpenDirectoryDialog(OnOpenDoseResultVR);
            }

            // Show button for opening the dataset editor (for changing the visualisation)
            if (GameObject.FindObjectOfType<VolumeRenderedObject>() != null && GUILayout.Button("Edit imported dataset"))
            {
                EditVolumeGUI.ShowWindow(GameObject.FindObjectOfType<VolumeRenderedObject>());
            }

            // Show button for opening the slicing plane editor (for changing the orientation and position)
            if (GameObject.FindObjectOfType<SlicingPlane>() != null && GUILayout.Button("Edit slicing plane"))
            {
                EditSliceGUI.ShowWindow(GameObject.FindObjectOfType<SlicingPlane>());
            }

            GUILayout.EndVertical();
        }

        private void OnOpenPARDatasetResult(RuntimeFileBrowser.DialogResult result)
        {
            if (!result.cancelled)
            {
                DespawnAllDatasets();
                string filePath = result.path;
                IImageFileImporter parimporter = ImporterFactory.CreateImageFileImporter(ImageFileFormat.VASP);
                VolumeDataset dataset = parimporter.Import(filePath);
                if (dataset != null)
                {
                        VolumeObjectFactory.CreateObject(dataset);
                }
            }
        }
        
        private void OnOpenRAWDatasetResult(RuntimeFileBrowser.DialogResult result)
        {
            if(!result.cancelled)
            {

                // We'll only allow one dataset at a time in the runtime GUI (for simplicity)
                DespawnAllDatasets();

                // Did the user try to import an .ini-file? Open the corresponding .raw file instead
                string filePath = result.path;
                if (System.IO.Path.GetExtension(filePath) == ".ini")
                    filePath = filePath.Replace(".ini", ".raw");

                // Parse .ini file
                DatasetIniData initData = DatasetIniReader.ParseIniFile(filePath + ".ini");
                if(initData != null)
                {
                    // Import the dataset
                    RawDatasetImporter importer = new RawDatasetImporter(filePath, initData.dimX, initData.dimY, initData.dimZ, initData.format, initData.endianness, initData.bytesToSkip);
                    VolumeDataset dataset = importer.Import();
                    // Spawn the object
                    if (dataset != null)
                    {
                        VolumeObjectFactory.CreateObject(dataset);
                    }
                }
            }
        }

        private void OnOpenDICOMDatasetResult(RuntimeFileBrowser.DialogResult result)
        {
            if (!result.cancelled)
            {
                // We'll only allow one dataset at a time in the runtime GUI (for simplicity)
                DespawnAllDatasets();

                bool recursive = true;

                // Read all files
                IEnumerable<string> fileCandidates = Directory.EnumerateFiles(result.path, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase));

                // Import the dataset
                IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.DICOM);
                IEnumerable<IImageSequenceSeries> seriesList = importer.LoadSeries(fileCandidates);
                float numVolumesCreated = 0;
                foreach (IImageSequenceSeries series in seriesList)
                {
                    VolumeDataset dataset = importer.ImportSeries(series);
                    // Spawn the object
                    if (dataset != null)
                    {
                        VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                        //obj.transform.position = new Vector3(numVolumesCreated, 0, 0);
                        numVolumesCreated++;
                    }
                }
            }
        }

        private void DespawnAllDatasets()
        {
            VolumeRenderedObject[] volobjs = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            foreach(VolumeRenderedObject volobj in volobjs)
            {
                GameObject.Destroy(volobj.gameObject);
            }
        }

        private void DespawnAllDatasetsVR()
        {
            VolumeRenderedObject[] volobjs = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            foreach (VolumeRenderedObject volobj in volobjs)
            {
                GameObject.Destroy(volobj.transform.parent.gameObject);
            }
        }
    }
}
