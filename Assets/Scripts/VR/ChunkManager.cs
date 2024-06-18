using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ChunkManager : NetworkBehaviour
{
    private List<float[]> receivedChunks;
    private int expectedChunks = 3000; // Total number of expected chunks
    private bool isReassembling = false;

    void Awake()
    {
        receivedChunks = new List<float[]>(expectedChunks);
    }

    [Rpc(SendTo.NotServer)]
    void SendChunkRpc(float[] chunk, bool isLastChunk, int index)
    {
        // Ensure the list is big enough
        if (receivedChunks.Count <= index)
        {
            receivedChunks.AddRange(new float[index - receivedChunks.Count + 1][]);
        }

        // Store the chunk
        receivedChunks[index] = chunk;

        // Check if all chunks are received
        if (isLastChunk && !isReassembling)
        {
            isReassembling = true;
            ReassembleChunks();
        }

        Debug.Log("Received chunk " + index + " of size " + chunk.Length);
    }

    private void ReassembleChunks()
    {
        int totalSize = 0;
        foreach (var chunk in receivedChunks)
        {
            totalSize += chunk.Length;
        }

        float[] reassembledData = new float[totalSize];
        int offset = 0;
        foreach (var chunk in receivedChunks)
        {
            System.Array.Copy(chunk, 0, reassembledData, offset, chunk.Length);
            offset += chunk.Length;
        }

        // Now reassembledData contains the full dataset
        Debug.Log("Reassembled data length: " + reassembledData.Length);
    }

    public void SendDatasetInChunks(float[] data)
    {
        int chunkSize = Mathf.CeilToInt((float)data.Length / expectedChunks);

        for (int i = 0; i < expectedChunks; i++)
        {
            int offset = i * chunkSize;
            int size = Mathf.Min(chunkSize, data.Length - offset);
            float[] chunk = new float[size];
            System.Array.Copy(data, offset, chunk, 0, size);

            SendChunkRpc(chunk, i == expectedChunks - 1, i);
        }
    }
}
