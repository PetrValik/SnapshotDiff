/**
 * SnapshotDiff File Download Module
 * Provides browser-based file download with File System Access API support
 * 
 * Features:
 * - File System Access API (Chrome 86+, Edge 86+) - user can choose save location
 * - Fallback to standard download for other browsers
 * - Streaming support for large files
 */

window.SnapshotDiff = window.SnapshotDiff || {};

/**
 * Download file using best available method
 * @param {string} fileName - Name of the file to download
 * @param {Uint8Array} byteArray - File content as byte array
 */
window.SnapshotDiff.downloadFile = async function (fileName, byteArray) {
    try {
        // Try File System Access API first (allows choosing save location)
        if (window.showSaveFilePicker) {
            await downloadWithFileSystemAccess(fileName, byteArray);
        } else {
            // Fallback to traditional download (auto-downloads to default location)
            downloadWithBlob(fileName, byteArray);
        }
    } catch (error) {
        // User cancelled save dialog
        if (error.name === 'AbortError') {
            console.log('User cancelled file save dialog');
            return;
        }
        
        // File System Access failed, try fallback
        console.warn('File System Access API failed, using fallback:', error);
        downloadWithBlob(fileName, byteArray);
    }
};

/**
 * Download using File System Access API (Chrome 86+, Edge 86+)
 * Shows native "Save as" dialog
 */
async function downloadWithFileSystemAccess(fileName, byteArray) {
    const blob = new Blob([byteArray], { type: 'application/octet-stream' });
    
    // Get file extension for filter
    const extension = fileName.split('.').pop() || '';
    const mimeType = getMimeType(extension);
    
    const options = {
        suggestedName: fileName,
        types: [{
            description: `${extension.toUpperCase()} File`,
            accept: { [mimeType]: ['.' + extension] }
        }]
    };

    // Show native save dialog
    const handle = await window.showSaveFilePicker(options);
    const writable = await handle.createWritable();
    
    // Write file
    await writable.write(blob);
    await writable.close();
    
    console.log(`✅ File saved: ${fileName} (${blob.size} bytes)`);
}

/**
 * Fallback download using Blob URLs
 * Auto-downloads to browser's default download location
 */
function downloadWithBlob(fileName, byteArray) {
    const blob = new Blob([byteArray], { type: 'application/octet-stream' });
    const url = URL.createObjectURL(blob);
    
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.style.display = 'none';
    
    document.body.appendChild(a);
    a.click();
    
    // Cleanup
    setTimeout(() => {
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }, 100);
    
    console.log(`✅ Download triggered: ${fileName} (${blob.size} bytes)`);
}

/**
 * Get MIME type based on file extension
 */
function getMimeType(extension) {
    const mimeTypes = {
        'csv': 'text/csv',
        'json': 'application/json',
        'txt': 'text/plain',
        'xml': 'application/xml'
    };
    return mimeTypes[extension.toLowerCase()] || 'application/octet-stream';
}

/**
 * Check if File System Access API is supported
 */
window.SnapshotDiff.isFileSystemAccessSupported = function () {
    return typeof window.showSaveFilePicker === 'function';
};

/**
 * Get browser support info for diagnostics
 */
window.SnapshotDiff.getBrowserSupport = function () {
    return {
        fileSystemAccess: typeof window.showSaveFilePicker === 'function',
        userAgent: navigator.userAgent,
        platform: navigator.platform
    };
};
