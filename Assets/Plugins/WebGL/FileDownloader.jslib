// This function allows adding custom JavaScript functions to the library that your C# code can call.
mergeInto(LibraryManager.library, {
    /**
     * Downloads a file created in Unity's memory to the user's computer.
     * This function is called from C# using the DllImport("__Internal") attribute.
     * * @param {string} fileName - The name of the file to be downloaded. This is passed as a pointer to a C-style string.
     * @param {number} array - A pointer to the beginning of the byte array in the Emscripten HEAP (HEAPU8).
     * @param {number} size - The total number of bytes in the array.
     */
    DownloadFile: function(fileName, array, size) {
        // UTF8ToString is a utility function from Emscripten to convert a C-style string pointer to a JavaScript string.
        const fileNameStr = UTF8ToString(fileName);
        // HEAPU8 is a typed array view (Uint8Array) into the Emscripten memory heap.
        // We create a new Uint8Array that is a copy of the relevant part of the heap.
        // Using .slice() creates a copy, which is safer as the memory region in the heap might be reused by Unity.
        const fileData = HEAPU8.slice(array, array + size);
        // Create a Blob from the file data. A Blob represents file-like objects of immutable, raw data.
        // The MIME type 'application/octet-stream' is a generic default for binary data.
        const blob = new Blob([fileData], {
            type: "application/octet-stream"
        });
        // To trigger a download, we create a temporary anchor (<a>) element.
        const link = document.createElement('a');
        // Create a URL that points to our Blob object in the browser's memory.
        const url = URL.createObjectURL(blob);
        // Set the href of the anchor to the blob's URL.
        link.href = url;
        // Set the download attribute to the desired file name. This tells the browser to download the file instead of navigating to the URL.
        link.download = fileNameStr;
        // The link doesn't need to be visible, so we hide it.
        link.style.display = 'none';
        // Add the link to the document's body. It needs to be in the DOM to be 'clicked'.
        document.body.appendChild(link);
        // Programmatically click the link to start the download.
        link.click();
        // Clean up by removing the link from the body.
        document.body.removeChild(link);
        // Revoke the object URL to free up browser memory. This is an important step.
        URL.revokeObjectURL(url);
    },
});