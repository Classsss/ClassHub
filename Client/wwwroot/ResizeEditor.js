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

function toggleSidebar() {
    console.log(document.getElementById("sidebar"))
    let sidebar = document.getElementById("sidebar");
    if (sidebar.classList.contains("collapse")) {
        sidebar.classList.remove("collapse");
    } else {
        sidebar.classList.add("collapse");
    }
}

window.reloadVideo = function () {
    var video = document.querySelector("video");
    if (video) {
        video.load();
    }
}

function showConfirm(Title, message, check) {
    return Swal.fire({
        title: Title,
        text: message,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        confirmButtonText: check,
        cancelButtonColor: '#d33',
        cancelButtonText: '취소'

    }).then((result) => {
        return result.isConfirmed;
    });
}
