using System.Net;

public class ProgressStreamContent : HttpContent
{
    private readonly Stream _fileStream;
    private readonly string _fileName;
    private readonly int _bufferSize = 1048576;

    public event Action<long, long>? ProgressChanged;

    public ProgressStreamContent(Stream fileStream, string fileName)
    {
        _fileStream = fileStream;
        _fileName = fileName;
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        var buffer = new byte[_bufferSize];
        long totalBytes = _fileStream.Length;
        long bytesRead = 0;

        while (true)
        {
            int read = await _fileStream.ReadAsync(buffer, 0, buffer.Length);
            if (read <= 0) break;

            await stream.WriteAsync(buffer, 0, read);
            bytesRead += read;

            ProgressChanged?.Invoke(bytesRead, totalBytes);
        }
    }

    protected override bool TryComputeLength(out long length)
    {
        length = _fileStream.Length;
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        _fileStream.Dispose();
        base.Dispose(disposing);
    }
}