window.simpleScan = window.simpleScan || {};

window.simpleScan.storage = {
    get: (key) => localStorage.getItem(key),
    set: (key, value) => localStorage.setItem(key, value)
};
