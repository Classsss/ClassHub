window.addEventListener("resize", function () {
    resizeEditor();
    setUniformRowHeight();
});

window.onresize = function () {
    resizeEditor();
    setUniformRowHeight();
};

function resizeEditor() {
    try {
        var editorElement = document.getElementById("editor");
        var style = window.getComputedStyle(editorElement);
        var height = window.innerHeight - 300 - parseFloat(style.borderTopWidth) - parseFloat(style.borderBottomWidth) - parseFloat(style.paddingTop) - parseFloat(style.paddingBottom);
        var width = window.innerWidth - 300 - parseFloat(style.borderLeftWidth) - parseFloat(style.borderRightWidth) - parseFloat(style.paddingLeft) - parseFloat(style.paddingRight);
        editorElement.style.height = height + "px";
        editorElement.style.width = width + "px";
    } catch (e) {

    }
}

function setPaginationInfo(key, value) {
    localStorage.setItem(key, value);
}

function getPaginationInfo(key) {
    return localStorage.getItem(key);
}
