window.simpleScan = window.simpleScan || {};

window.simpleScan.downloadFile = (url) => {
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = "";
    anchor.style.display = "none";

    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
};
