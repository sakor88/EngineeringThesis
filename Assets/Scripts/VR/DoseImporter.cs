using System;
using System.IO;
using UnityEngine;
using openDicom.Registry;
using openDicom.File;
using openDicom.DataStructure.DataSet;
using openDicom.DataStructure;
using System.Collections.Generic;
using System.Collections
using openDicom.Image;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine.SocialPlatforms;

namespace UnityVolumeRendering
{
    public class DoseImporter
    { 

        public AcrNemaFile LoadDoseFile(string filePath)
        {

            DataElementDictionary dataElementDictionary = new DataElementDictionary();
            UidDictionary uidDictionary = new UidDictionary();
            try
            {
                // Load .dic files from Resources
                TextAsset dcmElemAsset = (TextAsset)Resources.Load("dicom-elements-2007.dic");
                Debug.Assert(dcmElemAsset != null, "dicom-elements-2007.dic is missing from the Resources folder");
                TextAsset dcmUidsAsset = (TextAsset)Resources.Load("dicom-uids-2007.dic");
                Debug.Assert(dcmUidsAsset != null, "dicom-uids-2007.dic is missing from the Resources folder");

                dataElementDictionary.LoadFromMemory(new MemoryStream(dcmElemAsset.bytes), DictionaryFileFormat.BinaryFile);
                uidDictionary.LoadFromMemory(new MemoryStream(dcmUidsAsset.bytes), DictionaryFileFormat.BinaryFile);
            }
            catch (Exception dictionaryException)
            {
                Debug.LogError("Problems processing dictionaries:\n" + dictionaryException);
                return null;
            }

            AcrNemaFile file = null;
            Debug.Log(filePath);

            try
            {
                if (DicomFile.IsDicomFile(filePath))
                {
                    file = new DicomFile(filePath, false);
                }
                else if (AcrNemaFile.IsAcrNemaFile(filePath))
                    file = new AcrNemaFile(filePath, false);
                else
                {
                    Debug.LogError("Selected file is not a DICOM file.");
                }
            }
            catch (Exception dicomFileException)
            {
                Debug.LogError($"Problems processing the DICOM file {filePath} :\n {dicomFileException}");
                return null;
            }
            return file;

        }

        public VolumeDataset ReadDoseFile(string filePath)
        {
            AcrNemaFile file = LoadDoseFile(filePath);
            float[] CTdataset = getCTDataset();

            PixelData data = file.PixelData;
            Tag NumberOfFramesTag = new Tag("(0028, 0008)");
                
            VolumeDataset dataset = new VolumeDataset();
            dataset.datasetName = Path.GetFileName(filePath);
            dataset.dimX = file.PixelData.Columns;
            dataset.dimY = file.PixelData.Rows;
            if (file.DataSet.Contains(NumberOfFramesTag))
            {
                DataElement NoF = file.DataSet[NumberOfFramesTag];
                dataset.dimZ = (int)Convert.ToInt16(NoF.Value[0]);
                Debug.Log("dimZ: " + dataset.dimZ);
            }
            int dimension = dataset.dimX * dataset.dimY * dataset.dimZ;
            dataset.data = new float[dimension];

            
            PixelData pixelData = file.PixelData;
            int[] pixelArr = ToPixelArray(pixelData);

            for (int iZ = 0; iZ < dataset.dimZ; iZ++)
            {
                for (int iRow = 0; iRow < pixelData.Rows; iRow++)
                {
                    for (int iCol = 0; iCol < pixelData.Columns; iCol++)
                    {
                        int pixelIndex = (iZ * pixelData.Columns * pixelData.Rows) + (iRow * pixelData.Columns) + iCol;
                        int dataIndex = (iZ * pixelData.Columns * pixelData.Rows) + (iRow * pixelData.Columns) + iCol;

                        

                        int pixelValue = pixelArr[pixelIndex];
                        float grayValue = pixelValue * 6.3e-5f;

                        dataset.data[dataIndex] = grayValue;
                    }
                }
            }

/*                if (file.PixelData.pixelSpacing > 0.0f)
            {
                dataset.scaleX = file.PixelData.pixelSpacing * dataset.dimX;
                dataset.scaleY = file.PixelData.pixelSpacing * dataset.dimY;
                dataset.scaleZ = Mathf.Abs(files[files.Count - 1].location - files[0].location);
            }*/

            dataset.FixDimensions();

            return dataset;

        }

        public static float[] getCTDataset()
        {
            VolumeRenderedObject volObj = UnityEngine.Object.FindObjectsOfType<VolumeRenderedObject>()[0];
            VolumeDataset dataset = volObj.dataset;
            return dataset.data;
        }

        public static int[] ToPixelArray(PixelData pixelData)
        {
            int[] intArray;
            if (pixelData.Data.Value.IsSequence)
            {
                Sequence sq = (Sequence)pixelData.Data.Value[0];
                intArray = new int[sq.Count];
                for (int i = 0; i < sq.Count; i++)
                    intArray[i] = Convert.ToInt32(sq[i].Value[0]);
                return intArray;
            }
            else if (pixelData.Data.Value.IsArray)
            {
                byte[][] bytesArray = pixelData.ToBytesArray();
                if (bytesArray != null && bytesArray.Length > 0)
                {
                    byte[] bytes = bytesArray[0];

                    int cellSize = pixelData.BitsAllocated / 8;
                    int pixelCount = bytes.Length / cellSize;

                    intArray = new int[pixelCount];
                    int pixelIndex = 0;

                    // Byte array for a single cell/pixel value
                    byte[] cellData = new byte[cellSize];
                    for (int iByte = 0; iByte < bytes.Length; iByte++)
                    {
                        // Collect bytes for one cell (sample)
                        int index = iByte % cellSize;
                        cellData[index] = bytes[iByte];
                        // We have collected enough bytes for one cell => convert and add it to pixel array
                        if (index == cellSize - 1)
                        {
                            int cellValue = 0;
                            if (pixelData.BitsAllocated == 8)
                                cellValue = cellData[0];
                            else if (pixelData.BitsAllocated == 16)
                                cellValue = BitConverter.ToInt16(cellData, 0);
                            else if (pixelData.BitsAllocated == 32)
                                cellValue = BitConverter.ToInt32(cellData, 0);
                            else
                                Debug.LogError("Invalid format!");

                            intArray[pixelIndex] = cellValue;
                            pixelIndex++;
                        }
                    }
                    return intArray;
                }
                else
                    return null;
            }
            else
            {
                Debug.LogError("Pixel array is invalid");
                return null;
            }
        }
    }
}


