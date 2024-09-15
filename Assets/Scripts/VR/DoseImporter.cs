using System;
using System.IO;
using UnityEngine;
using openDicom.Registry;
using openDicom.File;
using openDicom.DataStructure.DataSet;
using openDicom.DataStructure;
using System.Collections.Generic;
using openDicom.Image;
using System.Linq;
using System.Data;

namespace UnityVolumeRendering
{
    public class DoseImporter
    {

        public class DICOMSliceFile : IImageSequenceFile
        {
            public AcrNemaFile file;
            public string filePath;
            public float location = 0;
            public Vector3 position = Vector3.zero;
            public float intercept = 0.0f;
            public float slope = 1.0f;
            public float pixelSpacing = 0.0f;
            public string seriesUID = "";
            public bool missingLocation = false;

            public string GetFilePath()
            {
                return filePath;
            }
        }

        public class DICOMSeries : IImageSequenceSeries
        {
            public List<DICOMSliceFile> dicomFiles = new List<DICOMSliceFile>();

            public IEnumerable<IImageSequenceFile> GetFiles()
            {
                return dicomFiles;
            }
        }

        private int iFallbackLoc = 0;

        public IEnumerable<IImageSequenceSeries> LoadSeries(IEnumerable<string> fileCandidates)
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

            // Load all DICOM files
            List<DICOMSliceFile> files = new List<DICOMSliceFile>();

            IEnumerable<string> sortedFiles = fileCandidates.OrderBy(s => s);

            foreach (string filePath in sortedFiles)
            {
                DICOMSliceFile sliceFile = ReadDICOMFile(filePath);
                if (sliceFile != null)
                {
                    if (sliceFile.file.PixelData.IsJpeg)
                        Debug.LogError("DICOM with JPEG not supported by importer. Please enable SimpleITK from volume rendering import settings.");
                    else
                        files.Add(sliceFile);
                }
            }

            // Split parsed DICOM files into series (by DICOM series UID)
            Dictionary<string, DICOMSeries> seriesByUID = new Dictionary<string, DICOMSeries>();
            foreach (DICOMSliceFile file in files)
            {
                if (!seriesByUID.ContainsKey(file.seriesUID))
                {
                    seriesByUID.Add(file.seriesUID, new DICOMSeries());
                }
                seriesByUID[file.seriesUID].dicomFiles.Add(file);
            }

            Debug.Log($"Loaded {seriesByUID.Count} DICOM series");

            return new List<DICOMSeries>(seriesByUID.Values);
        }

        public VolumeDataset ImportSeries(IImageSequenceSeries series, string doseFilePath)
        {
            DICOMSeries dicomSeries = (DICOMSeries)series;
            List<DICOMSliceFile> CTfiles = dicomSeries.dicomFiles;

            // Check if the series is missing the slice location tag
            bool needsCalcLoc = false;
            foreach (DICOMSliceFile file in CTfiles)
            {
                needsCalcLoc |= file.missingLocation;
            }

            // Calculate slice location from "Image Position" (0020,0032)
            if (needsCalcLoc)
                CalcSliceLocFromPos(CTfiles);

            // Sort files by slice location
            CTfiles.Sort((DICOMSliceFile a, DICOMSliceFile b) => { return a.location.CompareTo(b.location); });

            Debug.Log($"Importing {CTfiles.Count} DICOM slices");

            if (CTfiles.Count <= 1)
            {
                Debug.LogError("Insufficient number of slices.");
                return null;
            }

            //Create Dose data --------------------------------------------------------------------------------

            AcrNemaFile doseFile = LoadDoseFile(doseFilePath);

            PixelData data = doseFile.PixelData;
            Tag NumberOfFramesTag = new Tag("(0028, 0008)");
            Tag Rows = new Tag("(0028, 0010)");
            Tag Columns = new Tag("(0028, 0011)");
            Tag PixelSpacing = new Tag("(0028, 0030)");
            Tag ImagePosition = new Tag("(0020, 0032)");
            float doseXPos = 0.0f;
            float doseYPos = 0.0f;
            float doseZStartPos = 0.0f;
            float doseZEndPos = 0.0f;

            int doseDimX = 0;
            int doseDimY = 0;
            int doseDimZ = 0;
            float sliceThickness = 2.5f;

            DataElement pixelSpacing = doseFile.DataSet[PixelSpacing];
            float pixelSpacingf = (float)Convert.ToDouble(pixelSpacing.Value[0]);

            // Get dose dimensions
            DataElement row = doseFile.DataSet[Rows];
            DataElement col = doseFile.DataSet[Columns];
            DataElement NoF = doseFile.DataSet[NumberOfFramesTag];
            // TU JEST NA RAZIE ZAMIENIONE -------------------------------------------------------------------
            doseDimX = (int)Convert.ToInt16(col.Value[0]);
            doseDimY = (int)Convert.ToInt16(row.Value[0]);
            doseDimZ = (int)Convert.ToInt16(NoF.Value[0]);


            //Adjust dimensions to take image position into account
            if (doseFile.DataSet.Contains(ImagePosition))
            {
                DataElement elemLoc = doseFile.DataSet[ImagePosition];
                doseXPos = (float)Convert.ToDouble(elemLoc.Value[0]);
                doseYPos = (float)Convert.ToDouble(elemLoc.Value[1]);
                doseZStartPos = (float)Convert.ToDouble(elemLoc.Value[2]);
                doseZEndPos = (float)Math.Round(doseZStartPos + sliceThickness * doseDimZ);
            }


            // Create dataset
            VolumeDataset dataset = new VolumeDataset();
            dataset.datasetName = Path.GetFileName(CTfiles[0].filePath);
            dataset.dimX = CTfiles[0].file.PixelData.Columns;
            dataset.dimY = CTfiles[0].file.PixelData.Rows;
            dataset.dimZ = CTfiles.Count;


            //Number of dose voxels
            int numberOfVoxels = dataset.dimX * dataset.dimY * dataset.dimZ;
            dataset.data = new float[numberOfVoxels];


            Tag ImagePositionPatient = new Tag("(0020, 0032)");

            DataElement imagePosCT = CTfiles[0].file.DataSet[ImagePositionPatient];

            int[] startingPixel = new int[2];

            //Pixel index for the first pixel in the CT image from negative values
            startingPixel[0] = Convert.ToInt32(imagePosCT.Value[0]);
            startingPixel[1] = Convert.ToInt32(imagePosCT.Value[1]);


            DataElement imagePosDose = doseFile.DataSet[ImagePositionPatient];


            //finding the starting pixel for the dose image
            int doseStartRow = Convert.ToInt32(imagePosDose.Value[1]);
            int doseStartCol = Convert.ToInt32(imagePosDose.Value[0]);

            float doseEndRow = doseStartRow + (int)Math.Ceiling(pixelSpacingf * doseDimY);
            float doseEndCol = doseStartCol + (int)Math.Ceiling(pixelSpacingf * doseDimX);

            int rowOffset = doseStartRow - startingPixel[0];
            int colOffset = doseStartCol - startingPixel[1];

            Debug.Log("rowOffset: " + rowOffset + ", colOffset: " + colOffset);


            //SUPER RES + CT DATA START---------------------------------------------------------------------------------------------------------------

            PixelData dosePixelData = doseFile.PixelData;
            int[] dosePixelArr = ToPixelArray(dosePixelData);
            StandardizeDoseArray(dosePixelArr);

            Debug.Log("DosePixelArrMax: " + dosePixelArr.Max() + ", DosePixelArrMin: " + dosePixelArr.Min() + ", DosePixelArrAvg: " + dosePixelArr.Average());

            int[,,] superResPixelArr = superRes(dosePixelArr, doseDimY, doseDimX, doseDimZ, pixelSpacingf, 1.0f);

            Debug.Log(superResPixelArr.GetLength(0) + " " + superResPixelArr.GetLength(1) + " " + superResPixelArr.GetLength(2));

            Debug.Log("SuperResPixelArrMax: " + superResPixelArr.Cast<int>().Max() + ", SuperResPixelArrMin: " + superResPixelArr.Cast<int>().Min() + ", SuperResPixelArrAvg: " + superResPixelArr.Cast<int>().Average());

            for (int iSlice = 0; iSlice < CTfiles.Count; iSlice++)
            {

                DICOMSliceFile slice = CTfiles[iSlice];
                PixelData CTpixelData = slice.file.PixelData;
                int[] CTpixelArr = ToPixelArray(CTpixelData);
                if (CTpixelArr == null) // This should not happen
                    CTpixelArr = new int[CTpixelData.Rows * CTpixelData.Columns];

                DataElement imageZPosEle = slice.file.DataSet[ImagePosition];
                float imageZPos = (float)Convert.ToDouble(imageZPosEle.Value[2]);

                if ((imageZPos <= doseZEndPos && imageZPos >= doseZStartPos) || (imageZPos >= doseZEndPos && imageZPos <= doseZStartPos))
                {
                    int doseZIndex = (int)Math.Round((imageZPos - doseZStartPos) / sliceThickness);
                    Debug.Log(doseZIndex);

                    for (int iRow = 0; iRow < CTpixelData.Rows; iRow++)
                    {
                        for (int iCol = 0; iCol < CTpixelData.Columns; iCol++)
                        {
                            int pixelIndex2D = (iRow * CTpixelData.Columns) + iCol;
                            int dataIndex = (iSlice * CTpixelData.Columns * CTpixelData.Rows) + (iRow * CTpixelData.Columns) + iCol;

                            int realRow = startingPixel[0] + iRow;
                            int realCol = startingPixel[1] + iCol;
                            int doseXIndex, doseYIndex;

                            if (realRow > doseStartRow && realRow < doseEndRow && realCol > doseStartCol && realCol < doseEndCol)
                            {
                                //Debug.Log("realRow:" + realRow + ", realCol: " + realCol + " pixelIndex2D:" + pixelIndex2D + " dataIndex:" + dataIndex + " iRow:" + iRow + " iCol:" + iCol);
                                if (iRow == 0)
                                {
                                    doseXIndex = pixelIndex2D - colOffset;
                                    doseYIndex = 0;
                                }
                                else
                                {
                                    doseXIndex = iCol - colOffset - 1;
                                    doseYIndex = iRow - rowOffset - 1;
                                }
                                //Debug.Log("Z:" + doseZIndex + "Y:" + doseYIndex + "X:" + doseXIndex);
                                int pixelValue = superResPixelArr[doseZIndex, doseYIndex, doseXIndex];
                                if (pixelValue < 3300)
                                {
                                    pixelValue = CTpixelArr[pixelIndex2D];
                                    float hounsfieldValue = pixelValue * slice.slope + slice.intercept;
                                    dataset.data[dataIndex] = Mathf.Clamp(hounsfieldValue, -1024.0f, 3071.0f);
                                }
                                else
                                {
                                    //float grayValue = pixelValue * 6.3e-5f;
                                    dataset.data[dataIndex] = pixelValue;
                                }
                            }
                            else
                            {
                                int pixelValue = CTpixelArr[pixelIndex2D];
                                float hounsfieldValue = pixelValue * slice.slope + slice.intercept;
                                dataset.data[dataIndex] = Mathf.Clamp(hounsfieldValue, -1024.0f, 3071.0f);
                            }
                        }
                    }
                }
                else
                {
                    for (int iRow = 0; iRow < CTpixelData.Rows; iRow++)
                    {
                        for (int iCol = 0; iCol < CTpixelData.Columns; iCol++)
                        {
                            int pixelIndex = (iRow * CTpixelData.Columns) + iCol;
                            int dataIndex = (iSlice * CTpixelData.Columns * CTpixelData.Rows) + (iRow * CTpixelData.Columns) + iCol;

                            int pixelValue = CTpixelArr[pixelIndex];
                            float hounsfieldValue = pixelValue * slice.slope + slice.intercept;

                            dataset.data[dataIndex] = Mathf.Clamp(hounsfieldValue, -1024.0f, 3071.0f);
                        }
                    }
                }



                //if (CTfiles[0].pixelSpacing > 0.0f)
                //{
                //    dataset.scaleX = CTfiles[0].pixelSpacing* dataset.dimX;
                //    dataset.scaleY = CTfiles[0].pixelSpacing* dataset.dimY;
                //    dataset.scaleZ = Mathf.Abs(CTfiles[CTfiles.Count - 1].location - CTfiles[0].location);
                //}

                
            }

            dataset.FixDimensions();

            return dataset;

            //SUPER RES + CT DATA END---------------------------------------------------------------------------------------------------------------

            ////ONLY DOSE TEST START---------------------------------------------------------------------------------------------------------------

            //AcrNemaFile doseFile = LoadDoseFile(doseFilePath);

            //PixelData data = doseFile.PixelData;
            //Tag NumberOfFramesTag = new Tag("(0028, 0008)");

            //VolumeDataset dataset = new VolumeDataset();
            //dataset.datasetName = Path.GetFileName(doseFilePath);
            //dataset.dimX = doseFile.PixelData.Columns * 3;
            //dataset.dimY = doseFile.PixelData.Rows * 3;
            //if (doseFile.DataSet.Contains(NumberOfFramesTag))
            //{
            //    DataElement NoF = doseFile.DataSet[NumberOfFramesTag];
            //    dataset.dimZ = (int)Convert.ToInt16(NoF.Value[0]);
            //    Debug.Log("dimZ: " + dataset.dimZ);
            //}
            //int dimension = dataset.dimX * dataset.dimY * dataset.dimZ;
            //dataset.data = new float[dimension];

            //Debug.Log("dimX: " + dataset.dimX + ", dimY: " + dataset.dimY + ", dimZ: " + dataset.dimZ + "dimension: " + dataset.data.Length);

            //Tag PixelSpacing = new Tag("(0028, 0030)");
            //DataElement pixelSpacing = doseFile.DataSet[PixelSpacing];
            //float pixelSpacingf = (float)Convert.ToDouble(pixelSpacing.Value[0]);


            //PixelData pixelData = file.PixelData;
            //int[] pixelArr = ToPixelArray(pixelData);

            //for (int iZ = 0; iZ < dataset.dimZ; iZ++)
            //{
            //    for (int iRow = 0; iRow < pixelData.Rows; iRow++)
            //    {
            //        for (int iCol = 0; iCol < pixelData.Columns; iCol++)
            //        {
            //            int pixelIndex = (iZ * pixelData.Columns * pixelData.Rows) + (iRow * pixelData.Columns) + iCol;
            //            int dataIndex = (iZ * pixelData.Columns * pixelData.Rows) + (iRow * pixelData.Columns) + iCol;



            //            int pixelValue = pixelArr[pixelIndex];
            //            float grayValue = pixelValue * 6.3e-5f;

            //            dataset.data[dataIndex] = grayValue;
            //        }
            //    }
            //}

            ///*                if (file.PixelData.pixelSpacing > 0.0f)
            //            {
            //                dataset.scaleX = file.PixelData.pixelSpacing * dataset.dimX;
            //                dataset.scaleY = file.PixelData.pixelSpacing * dataset.dimY;
            //                dataset.scaleZ = Mathf.Abs(files[files.Count - 1].location - files[0].location);
            //            }*/

            //dataset.FixDimensions();

            //return dataset;


            //ONLY DOSE TEST END---------------------------------------------------------------------------------------------------------------

            //SUPER RES TEST START---------------------------------------------------------------------------------------------------------------


            //int doseDimY = dataset.dimY / 3;
            //int doseDimX = dataset.dimX / 3;
            //int doseDimZ = dataset.dimZ;

            //PixelData dosePixelData = doseFile.PixelData;
            //int[] dosePixelArr = ToPixelArray(dosePixelData);
            //int[,,] superResPixelArr = superRes(dosePixelArr, doseDimY, doseDimX, doseDimZ, pixelSpacingf, 1.0f);

            //Debug.Log(superResPixelArr.GetLength(0) + " " + superResPixelArr.GetLength(1) + " " + superResPixelArr.GetLength(2));

            //for (int iZ = 0; iZ < 129; iZ++)
            //{
            //    for (int iRow = 0; iRow < 282; iRow++)
            //    {
            //        for (int iCol = 0; iCol < 477; iCol++)
            //        {
            //            int pixelIndex = (iZ * 282 * 477) + (iRow * 477) + iCol;
            //            int dataIndex = (iZ * 282 * 477) + (iRow * 477) + iCol;



            //            int pixelValue = superResPixelArr[iZ, iRow, iCol];
            //            float grayValue = pixelValue * 6.3e-5f;

            //            dataset.data[dataIndex] = grayValue;
            //        }
            //    }
            //}

            //dataset.FixDimensions();

            //return dataset;

            //SUPER RES TEST END---------------------------------------------------------------------------------------------------------------

        }

        void StandardizeDoseArray(int[] dosePixelArr)
        {
            // Find the min and max values above the CT data
            int ctMin = 3071;
            int ctMax = 4000;

            // Find the min and max values of the dose data
            int doseMin = dosePixelArr.Min();
            int doseMax = dosePixelArr.Max();

            // Standardize the dose values
            for (int i = 0; i < dosePixelArr.Length; i++)
            {
                // Scale the dose value to the CT range
                dosePixelArr[i] = (dosePixelArr[i] - doseMin) * (ctMax - ctMin) / (doseMax - doseMin) + ctMin;
            }
        }


        int[,,] superRes(int[] originalArr, int rows, int cols, int slices, float pxSizeBefore, float pxSizeAfter)
        {

            float scaleFactor = pxSizeBefore / pxSizeAfter;

            int newYDim = (int)Math.Ceiling(rows * scaleFactor);
            int newXDim = (int)Math.Ceiling(cols * scaleFactor);
            int newZDim = slices;

            int[,,] superResArr = new int[newZDim, newYDim, newXDim];

            Debug.Log("scaleFactor: " + scaleFactor + ",y dim: " + newYDim + ",x dim: " + newXDim + "");

            int floor = (int)Math.Floor(scaleFactor);
            int ceil = (int)Math.Ceiling(scaleFactor);

            int newIncrementX = floor;
            int newIncrementY = floor;

            //int newIncrementX = 2;
            //int newIncrementY = 2;

            for (int z = 0; z < newZDim; z++)
            {
                for (int y = 0, y_old = 0; y_old < rows; y += newIncrementY, y_old++)
                {
                    for (int x = 0, x_old = 0; x_old < cols; x += newIncrementX, x_old++)
                    {
                        if(newIncrementX == floor)
                        {
                            newIncrementX = ceil;
                        }
                        else
                        {
                            newIncrementX = floor;
                        }

                        if (newIncrementY == floor)
                        {
                            newIncrementY = ceil;
                        }
                        else
                        {
                            newIncrementY = floor;
                        }



                        int internalValue = originalArr[z * cols * rows + y_old * cols + x_old];
                        superResArr[z, y, x] = internalValue;
                        superResArr[z, y, x + 1] = internalValue;
                        superResArr[z, y + 1, x] = internalValue;
                        superResArr[z, y + 1, x + 1] = internalValue;

                        if (x_old < cols - 1 && y_old < rows - 1)
                        {

                            if(x_old % 2 == 0)
                            {
                                //to the right of the original 2x2 pixel
                                superResArr[z, y, x + 2] = (originalArr[z * cols * rows + y_old * cols + x_old] + originalArr[z * cols * rows + y_old * cols + x_old + 1]) / 2;
                                superResArr[z, y + 1, x + 2] = (originalArr[z * cols * rows + y_old * cols + x_old] + originalArr[z * cols * rows + y_old * cols + x_old + 1]) / 2;

                                if (y_old % 2 == 0)
                                {
                                    //in the bottom right corner of the original 2x2 pixel
                                    superResArr[z, y + 2, x + 2] = (originalArr[z * cols * rows + y_old * cols + x_old] + originalArr[z * cols * rows + y_old * cols + x_old + 1] +
                                    originalArr[z * cols * rows + (y_old + 1) * cols + x_old] + originalArr[z * cols * rows + (y_old + 1) * cols + x_old + 1]) / 4;
                                }
                            }
                            
                            
                            if(y_old % 2 == 0)
                            {
                                //under the original 2x2 pixel
                                superResArr[z, y + 2, x] = (originalArr[z * cols * rows + y_old * cols + x_old] + originalArr[z * cols * rows + (y_old + 1) * cols + x_old]) / 2;
                                superResArr[z, y + 2, x + 1] = (originalArr[z * cols * rows + y_old * cols + x_old] + originalArr[z * cols * rows + (y_old + 1) * cols + x_old]) / 2;
                            }
                            
                        }
                        else
                        {

                            if(cols % 2 != 0)
                            {
                                //to the right of the original 2x2 pixel
                                superResArr[z, y, x + 2] = (originalArr[z * cols * rows + y_old * cols + x_old]);
                                superResArr[z, y + 1, x + 2] = (originalArr[z * cols * rows + y_old * cols + x_old]);

                                if (rows % 2 != 0)
                                {
                                    //in the bottom right corner of the original 2x2 pixel
                                    superResArr[z, y + 2, x + 2] = (originalArr[z * cols * rows + y_old * cols + x_old]);
                                }
                            }
                            if(rows % 2 != 0)
                            {
                                //under the original 2x2 pixel
                                superResArr[z, y + 2, x] = (originalArr[z * cols * rows + y_old * cols + x_old]);
                                superResArr[z, y + 2, x + 1] = (originalArr[z * cols * rows + y_old * cols + x_old]);
                            }
                        }


                    }
                }
            }

            return superResArr;
        }

       

        private DICOMSliceFile ReadDICOMFile(string filePath)
        {
            AcrNemaFile file = LoadFile(filePath);

            if (file != null && file.HasPixelData)
            {
                DICOMSliceFile slice = new DICOMSliceFile();
                slice.file = file;
                slice.filePath = filePath;

                Tag locTag = new Tag("(0020,1041)");
                Tag posTag = new Tag("(0020,0032)");
                Tag interceptTag = new Tag("(0028,1052)");

                Tag slopeTag = new Tag("(0028,1053)");
                Tag pixelSpacingTag = new Tag("(0028,0030)");
                Tag seriesUIDTag = new Tag("(0020,000E)");

                // Read location (optional)
                if (file.DataSet.Contains(locTag))
                {
                    DataElement elemLoc = file.DataSet[locTag];
                    slice.location = (float)Convert.ToDouble(elemLoc.Value[0]);
                }
                // If no location tag, read position tag (will need to calculate location afterwards)
                else if (file.DataSet.Contains(posTag))
                {
                    DataElement elemLoc = file.DataSet[posTag];
                    Vector3 pos = Vector3.zero;
                    pos.x = (float)Convert.ToDouble(elemLoc.Value[0]);
                    pos.y = (float)Convert.ToDouble(elemLoc.Value[1]);
                    pos.z = (float)Convert.ToDouble(elemLoc.Value[2]);
                    slice.position = pos;
                    slice.missingLocation = true;
                }
                else
                {
                    Debug.LogError($"Missing location/position tag in file: {filePath}.\n The file will not be imported correctly.");
                    // Fallback: use counter as location
                    slice.location = (float)iFallbackLoc++;
                }

                // Read intercept
                if (file.DataSet.Contains(interceptTag))
                {
                    DataElement elemIntercept = file.DataSet[interceptTag];
                    slice.intercept = (float)Convert.ToDouble(elemIntercept.Value[0]);
                }
                else
                    Debug.LogWarning($"The file {filePath} is missing the intercept element. As a result, the default transfer function might not look good.");

                // Read slope
                if (file.DataSet.Contains(slopeTag))
                {
                    DataElement elemSlope = file.DataSet[slopeTag];
                    slice.slope = (float)Convert.ToDouble(elemSlope.Value[0]);
                }
                else
                    Debug.LogWarning($"The file {filePath} is missing the intercept element. As a result, the default transfer function might not look good.");

                // Read pixel spacing
                if (file.DataSet.Contains(pixelSpacingTag))
                {
                    DataElement elemPixelSpacing = file.DataSet[pixelSpacingTag];
                    slice.pixelSpacing = (float)Convert.ToDouble(elemPixelSpacing.Value[0]);
                }

                // Read series UID
                if (file.DataSet.Contains(seriesUIDTag))
                {
                    DataElement elemSeriesUID = file.DataSet[seriesUIDTag];
                    slice.seriesUID = Convert.ToString(elemSeriesUID.Value[0]);
                }

                return slice;
            }
            return null;
        }

        private AcrNemaFile LoadFile(string filePath)
        {
            AcrNemaFile file = null;
            try
            {
                if (DicomFile.IsDicomFile(filePath))
                    file = new DicomFile(filePath, false);
                else if (AcrNemaFile.IsAcrNemaFile(filePath))
                    file = new AcrNemaFile(filePath, false);
                else
                    Debug.LogError("Selected file is neither a DICOM nor an ACR-NEMA file.");
            }
            catch (Exception dicomFileException)
            {
                Debug.LogError($"Problems processing the DICOM file {filePath} :\n {dicomFileException}");
                return null;
            }
            return file;
        }

        private static int[] ToPixelArray(PixelData pixelData)
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

        private void CalcSliceLocFromPos(List<DICOMSliceFile> slices)
        {
            // We use the first slice as a starting point (a), andthe normalised vector (v) between the first and second slice as a direction.
            Vector3 v = (slices[1].position - slices[0].position).normalized;
            Vector3 a = slices[0].position;
            slices[0].location = 0.0f;

            for (int i = 1; i < slices.Count; i++)
            {
                // Calculate the vector between a and p (ap) and dot it with v to get the distance along the v vector (distance when projected onto v)
                Vector3 p = slices[i].position;
                Vector3 ap = p - a;
                float dot = Vector3.Dot(ap, v);
                slices[i].location = dot;
            }
        }


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
    }
}