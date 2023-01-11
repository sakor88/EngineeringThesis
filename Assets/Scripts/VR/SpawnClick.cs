using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tilia.Interactions.Interactables.Interactables;
using UnityEngine;
/*using UnityEditor;
using System.Runtime.InteropServices.ComTypes;
using Tilia.Interactions.Interactables.Utility;
using Tilia.Interactions.Interactables.Interactables.Grab;
using Tilia.Interactions.Interactables.Interactables.Grab.Action;*/

namespace UnityVolumeRendering{
    public class SpawnClick : MonoBehaviour
    {
        private GameObject interactablePrefab;
        [SerializeField]
        private const string assetName = "Interactions.Interactable";
        [SerializeField]
        private const string assetSuffix = ".prefab";

        public void OnButtonPress()
        {
            string dir = @"D:\Semestr6\Inzynierka\CT\CT";
            bool recursive = true;

            IEnumerable<string> fileCandidates = Directory.EnumerateFiles(dir, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Where(p => p.EndsWith(".dcm", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicom", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".dicm", StringComparison.InvariantCultureIgnoreCase));

            // Get all files in DICOM directory
            List<string> filePaths = Directory.GetFiles(dir).ToList();
            // Create importer
            IImageSequenceImporter importer = ImporterFactory.CreateImageSequenceImporter(ImageSequenceFormat.DICOM);
            // Load list of DICOM series (normally just one series)
            IEnumerable<IImageSequenceSeries> seriesList = importer.LoadSeries(filePaths);
            // There will usually just be one series
            foreach (IImageSequenceSeries series in seriesList)
            {
                // Import single DICOm series
                VolumeDataset dataset = importer.ImportSeries(series);
                VolumeRenderedObject obj = VolumeObjectFactory.CreateObject(dataset);
                obj.transform.position = new Vector3(-0.1f, 2.7f, 0.0f);
                var child = obj.gameObject.transform.GetChild(0).gameObject;

                
                GameObject test = new GameObject();
                /*Instantiate(test, transform.position, Quaternion.identity);*/
                obj.transform.SetParent(test.transform);
                child.transform.SetParent(test.transform);

                child.AddComponent<BoxCollider>();
                prepareInteractablePrefab();
                ConvertSelectedGameObject(child.gameObject);

                
                obj.CreateSlicingPlane();
                SlicingPlane plane = FindObjectsOfType<SlicingPlane>()[0];
                obj.gameObject.transform.position = new Vector3(-2.25f, 2.5f, -2f);
                obj.gameObject.transform.localEulerAngles = new Vector3(90f, 0f, 90f);
                plane.gameObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

            }

        }

        public void prepareInteractablePrefab()
        {
            interactablePrefab = (GameObject) Resources.Load("Interactions.Interactable");
        }

        public void CreateInteractable(GameObject objectToWrap)
        {
            int siblingIndex = objectToWrap.transform.GetSiblingIndex();
            GameObject newInteractable = Instantiate(interactablePrefab);    //(GameObject)PrefabUtility.InstantiatePrefab(interactablePrefab);
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


    }
}
