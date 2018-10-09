using System;

namespace Client
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(long bytesTransferred, long totalBytes, int transferSpeed)
        {
            BytesTransferred = bytesTransferred;
            TotalBytes = totalBytes;
            TransferSpeed = transferSpeed;
        }

        public long BytesTransferred { get; private set; }
        public long TotalBytes { get; private set; }
        public int TransferSpeed { get; private set; }
    }
}