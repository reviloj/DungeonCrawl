var animationIndex = 0;
var interval;
var origHeight = -1;
var origWidth = -1;
let zoom;
let cellHeight;
let cellWidth;
let speed;
let startRow;
let goX;
let goY;

function updateVars(){
    zoom = document.getElementById('animZoom').value;
    if (zoom == "")
        zoom = 1;
    cellHeight = document.getElementById('cellHeight').value;
    if (cellHeight == "")
        cellHeight = 100;
    cellWidth = document.getElementById('cellWidth').value;
    if (cellWidth == "")
        cellWidth = 100;
    startRow = document.getElementById('startRow').value;
    if (startRow == "")
        startRow = 1;
    goX = document.getElementById('goX').value;
    if (goX == "")
        goX = 1;
    goY = document.getElementById('goY').value;
    if (goY == "")
        goY = 1;
    speed = document.getElementById('animSpeed').value;
    if (speed == "")
        speed = 100;
}

function loadSpriteSheet(event) {
    let image = document.getElementById('output');
    image.src = URL.createObjectURL(event.target.files[0]);
};

function updateSpriteSheetPos(){
    let image = document.getElementById('output');
    let wrapper = document.getElementById('animationBox');

    if(origWidth == -1 || origHeight == -1){
        origHeight = image.height;
        origWidth = image.width;
    }

    updateVars();

    image.style.height = origHeight * zoom + 'px';
    image.style.width = origWidth * zoom + 'px';
    image.style.top = -cellHeight * (startRow - 1) * zoom + 'px';
    image.style.left = -animationIndex * cellWidth * zoom + 'px';
    wrapper.style.width = cellWidth * zoom + 'px';
    wrapper.style.height = cellHeight * zoom + 'px';

}

function runAnimation(){
    updateSpriteSheetPos();
    clearInterval(interval);
    let image = document.getElementById('output');
    updateVars()
    interval = setInterval(function() {
        animationIndex++;
        if (animationIndex == image.width / zoom / cellWidth)
            animationIndex = 0;
        image.style.left = -animationIndex * cellWidth * zoom + 'px';
      }, speed);
}

function stopAnimation(){
    clearInterval(interval);
}

function gotoCell(){
    let image = document.getElementById('output');
    updateVars()
            
    animationIndex = goX - 1;
    image.style.top = -cellHeight * (goY - 1) * zoom + 'px';
    image.style.left = -animationIndex * cellWidth * zoom + 'px';
    document.getElementById('startRow').value = goY.value;
}
